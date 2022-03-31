using System;
using System.Collections.Generic;
using System.Threading;
using Piipan.Etl.Func.BulkUpload.Models;
using Npgsql;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Utilities;
using NpgsqlTypes;
using System.Linq;
using Dapper;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
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

                using (var cmd = Factory.CreateCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "DROP TABLE IF EXISTS participants;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DROP TABLE IF EXISTS uploads;";
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        private void ApplySchema()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (var cmd = Factory.CreateCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "DROP TABLE IF EXISTS participants;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DROP TABLE IF EXISTS uploads;";
                    cmd.ExecuteNonQuery();

                    string sqltext = System.IO.File.ReadAllText("per-state.sql", System.Text.Encoding.UTF8);
                    cmd.CommandText = sqltext;
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public void ClearParticipants()
        {
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (var cmd = Factory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM participants";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public IEnumerable<IParticipant> QueryParticipants(string sql)
        {
            var records = new List<IParticipant>();

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();


                using (var cmd = Factory.CreateCommand())
                {

                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var record = new Participant
                        {
                            LdsHash = reader[1].ToString(),
                            CaseId = reader[3].ToString(),
                            ParticipantId = reader[4].ToString(),
                            ParticipantClosingDate = reader[5] is DBNull ? (DateTime?)null : Convert.ToDateTime(reader[5]),
                            RecentBenefitIssuanceDates = reader[reader.GetOrdinal("recent_benefit_issuance_dates")] is DBNull ? new List<DateRange>() : ((NpgsqlRange<DateTime>[])reader[reader.GetOrdinal("recent_benefit_issuance_dates")]).Select(user => new DateRange() { Start = user.LowerBound, End = user.UpperBound }).ToList(),
                            ProtectLocation = reader[reader.GetOrdinal("protect_location")] is DBNull ? (Boolean?)null : Convert.ToBoolean(reader[reader.GetOrdinal("protect_location")])
                        };
                        records.Add(record);
                    }

                    conn.Close();
                }
            }
            return records;
        }
    }
}
