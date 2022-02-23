using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for match records integration testing.
    /// Creates a fresh matches tables, dropping it when testing is complete.
    /// </summary>
    public class DbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public DbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            Initialize();
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        public void Initialize()
        {
            var retries = 10;
            var wait = 2000; // ms

            while (retries >= 0)
            {
                try
                {
                    using (var conn = Factory.CreateConnection())
                    {
                        conn.ConnectionString = ConnectionString;
                        conn.Open();
                        conn.Close();

                        return;
                    }
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    retries--;
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(wait);
                }
            }
        }

        public void Dispose()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TYPE IF EXISTS hash_type");
                conn.Execute("DROP TYPE IF EXISTS status");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("match-record.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute("DROP TYPE IF EXISTS hash_type");
                conn.Execute("DROP TYPE IF EXISTS status");
                conn.Execute(sqltext);

                conn.Close();
            }
        }

        public void ClearMatchRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM matches");

                conn.Close();
            }
        }

        public void Insert(MatchRecordDbo record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute(@"
                    INSERT INTO matches
                    (
                        created_at,
                        match_id,
                        initiator,
                        states,
                        status,
                        hash,
                        hash_type,
                        input,
                        data
                    )
                    VALUES
                    (
                        now() at time zone 'utc',
                        @MatchId,
                        @Initiator,
                        @States,
                        @Status::status,
                        @Hash,
                        @HashType::hash_type,
                        @Input::jsonb,
                        @Data::jsonb
                    )", record);

                conn.Close();
            }
        }

        public bool HasRecord(MatchRecordDbo record)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var row = conn.QuerySingle<MatchRecordDbo>(@"
                    SELECT match_id,
                        created_at,
                        initiator,
                        hash,
                        hash_type::text,
                        states,
                        input,
                        data,
                        invalid,
                        status::text
                    FROM matches
                    WHERE match_id=@MatchId", record);

                result = row.Equals(record);

                conn.Close();
            }

            return result;
        }
    }
}
