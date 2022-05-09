using System;
using System.Threading;
// using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;

namespace Piipan.Shared.TestFixtures
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// Requires environment variable DatabaseConnectionString to be set.
    /// </summary>
    public class ParticipantsDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public ParticipantsDbFixture()
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
                conn.Execute("INSERT INTO uploads(created_at, publisher,upload_identifier, status) VALUES(now() at time zone 'utc', current_user,'test-etag', 'Complete')");

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
    }
}
