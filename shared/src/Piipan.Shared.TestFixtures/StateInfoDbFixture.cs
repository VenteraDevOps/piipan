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

                conn.Execute("TRUNCATE TABLE state_info");

                conn.Close();
            }

        }

        private void ApplySchema()
        {
            string comments = "This method is being kept in for book-keeping but is not needed because we are not dropping the tables in dispose()";
        }

        public void ClearStates()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("DELETE FROM state_info");

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
