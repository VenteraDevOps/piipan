using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Match.Core.Models;
using Piipan.Participants.Core.Models;

namespace Piipan.Match.Func.Api.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly string CollabConnectionString;
        public readonly NpgsqlFactory Factory;

        public DbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            CollabConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

            Initialize(ConnectionString);
            Initialize(CollabConnectionString);
            ApplySchema();
        }

        /// <summary>
        /// Ensure the database is able to receive connections before proceeding.
        /// </summary>
        public void Initialize(string connectionString)
        {
            var retries = 10;
            var wait = 2000; // ms

            while (retries >= 0)
            {
                try
                {
                    using (var conn = Factory.CreateConnection())
                    {
                        conn.ConnectionString = connectionString;
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
                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Close();
            }

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();
                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string perstateSql = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);
            string matchesSql = System.IO.File.ReadAllText("match-record.sql", System.Text.Encoding.UTF8);

            // Participants DB
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Execute(perstateSql);
                conn.Execute("INSERT INTO uploads(created_at, publisher) VALUES(now() at time zone 'utc', current_user)");

                conn.Close();
            }

            // Collaboration DB
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                conn.Execute("DROP INDEX IF EXISTS index_match_id_on_match_res_events");
                conn.Execute("DROP TABLE IF EXISTS match_res_events");
                conn.Execute("DROP TABLE IF EXISTS matches");
                conn.Execute(matchesSql);

                conn.Close();
            }
        }

        public void ClearParticipants()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM participants");

                conn.Close();
            }
        }

        public void ClearMatchRecords()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM matches");

                conn.Close();
            }
        }

        public int CountMatchRecords()
        {
            int count;

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM matches");

                conn.Close();
            }

            return count;
        }

        public MatchRecordDbo GetMatchRecord(string matchId)
        {
            MatchRecordDbo record;

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                record = conn.QuerySingle<MatchRecordDbo>(
                    @"SELECT
                        match_id,
                        initiator,
                        states,
                        status::text,
                        hash,
                        hash_type::text,
                        input::jsonb,
                        data::jsonb
                    FROM matches
                    WHERE match_id=@matchId;",
                    new { matchId = matchId });

                conn.Close();
            }

            return record;
        }

        public void Insert(ParticipantDbo record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(record);
                parameters.Add("UploadId", lastval);

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_months, protect_location)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitMonths::date[], @ProtectLocation)",
                    parameters);

                conn.Close();
            }
        }
    }
}
