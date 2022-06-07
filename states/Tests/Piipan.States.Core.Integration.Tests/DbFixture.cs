using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piipan.States.Core.Models;
using Npgsql;
using Piipan.Shared.TestFixtures;
using Dapper;

namespace Piipan.States.Core.Integration.Tests
{
    public class DbFixture : StateInfoDbFixture
    {

        public void Insert(StateInfoDbo state)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM state_info");
                DynamicParameters parameters = new DynamicParameters(state);
                parameters.Add("Id", lastval);

                conn.Execute(@"
                    INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
                    VALUES (@Id, @State, @StateAbbreviation, @Email, @Phone, @Region)",
                    parameters);

                conn.Close();
            }
        }

        public void InsertState()
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("INSERT INTO state_info(id, state, state_abbreviation, email, phone, region) VALUES('99', 'test' ,'TT', 'test@test.com', '5551234', 'TEST')");
                conn.Close();
            }
        }

        public void InsertStates()
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("INSERT INTO state_info(id, state, state_abbreviation, email, phone, region) VALUES('99', 'test' ,'TT', 'test@test.com', '5551234', 'TEST')");
                conn.Execute("INSERT INTO state_info(id, state, state_abbreviation, email, phone, region) VALUES('100', 'test2' ,'TS', 'test2@test2.com', '9999999', 'TWO')");
                conn.Close();
            }
        }

        public bool HasState(StateInfoDbo state)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var record = conn.Query<StateInfoDbo>(@"
                    SELECT id Id,
                        state State,
                        state_abbreviation StateAbbreviation,
                        email Email,
                        phone Phone,
                        region Region,
                    FROM state_info
                    WHERE id=@Id", state).FirstOrDefault();

                if (record == null)
                {
                    result = false;
                }
                else
                {
                    result = record.Equals(state);
                }

                conn.Close();
            }
            return result;
        }
    }
}
