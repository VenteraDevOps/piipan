using System;
using System.Threading;
using Dapper;
using Npgsql;

namespace Piipan.Shared.TestFixtures
{
    public class StateInfoDbFixture : IDisposable
    {
        public readonly string ConnectionString;
        public readonly NpgsqlFactory Factory;

        public StateInfoDbFixture()
        {
            ConnectionString = Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString");
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
            ClearStates();
        }

        private void ApplySchema()
        {
            string sqltext = System.IO.File.ReadAllText("state-record.sql", System.Text.Encoding.UTF8);

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DROP TABLE IF EXISTS state_info");
                conn.Execute(sqltext);
                conn.Execute("INSERT INTO state_info(id, state, state_abbreviation, email, phone, region) VALUES('0', 'zero', 'TT', 'test@test.com', '5551234', 'ZERO')");

                conn.Close();
            }
        }

        public void ClearStates()
        {

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("TRUNCATE TABLE state_info");

                conn.Close();
            }
        }


        public string GetLastStateId()
        {
            string result = "";
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = conn.ExecuteScalar<string>("SELECT id from state_info order by id DESC limit 1");
                conn.Close();
            }
            return result;
        }

        public string GetFirstStateId()
        {
            string result = "";
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = conn.ExecuteScalar<string>("SELECT id from state_info order by id ASC limit 1");
                conn.Close();
            }
            return result;
        }

        public string GetLastStateName()
        {
            string result = "";
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = conn.ExecuteScalar<string>("SELECT state from state_info order by id DESC limit 1");
                conn.Close();
            }
            return result;
        }
    }
}
