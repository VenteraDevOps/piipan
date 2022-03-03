using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public DbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("DatabaseConnectionString");
            Factory = NpgsqlFactory.Instance;

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

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS participants");
                conn.Execute("DROP TABLE IF EXISTS uploads");
                conn.Execute(sqltext);
                conn.Execute("INSERT INTO uploads(created_at, publisher) VALUES(now() at time zone 'utc', current_user)");

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

        public void ClearUploads()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM uploads");

                conn.Close();
            }
        }

        public Int64 GetLastUploadId()
        {
            Int64 result = 0;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                conn.Close();
            }
            return result;
        }

        public void Insert(ParticipantDbo participant)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(participant);
                parameters.Add("UploadId", lastval);

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_months, protect_location)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitMonths::date[], @ProtectLocation)",
                    parameters);

                conn.Close();
            }
        }

        public void InsertUpload()
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("INSERT INTO uploads(created_at, publisher) VALUES(now() at time zone 'utc', current_user)");
                conn.Close();
            }
        }

        public bool HasParticipant(ParticipantDbo participant)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var record = conn.QuerySingle<ParticipantDbo>(@"
                    SELECT lds_hash LdsHash,
                        participant_id ParticipantId,
                        case_id CaseId,
                        participant_closing_date ParticipantClosingDate,
                        recent_benefit_months RecentBenefitMonths,
                        protect_location ProtectLocation,
                        upload_id UploadId
                    FROM participants
                    WHERE lds_hash=@LdsHash", participant);
                    
                result = record.Equals(participant);

                conn.Close();
            }
            return result;
        }
    }
}
