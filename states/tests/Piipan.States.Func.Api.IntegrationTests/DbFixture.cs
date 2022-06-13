using Dapper;
using Npgsql;
using Piipan.Shared.TestFixtures;
using Piipan.States.Core.Models;

namespace Piipan.States.Func.Api.IntegrationTests
{
    /// <summary>
    /// Test fixture for match records integration testing.
    /// Creates a fresh matches tables, dropping it when testing is complete.
    /// </summary>
    public class DbFixture : StateInfoDbFixture
    {
        public void Insert(StateInfoDbo state)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                DynamicParameters parameters = new DynamicParameters(state);

                conn.Execute(@"
                    INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
                    VALUES (@Id, @State, @StateAbbreviation, @Email, @Phone, @Region)",
                    parameters);

                conn.Close();
            }
        }
    }
}
