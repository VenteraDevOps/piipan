using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Metrics.Api;
using Piipan.Shared.TestFixtures;

namespace Piipan.Metrics.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for metrics API database integration testing.
    /// Creates a fresh metrics database, dropping it when complete
    /// </summary>
    public class DbFixture : MetricsDbFixture
    {

        protected void Insert(string state, DateTime uploadedAt)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute("INSERT INTO participant_uploads(state, uploaded_at) VALUES (@state, @uploadedAt)", new { state = state, uploadedAt = uploadedAt });

                conn.Close();
            }
        }

        protected bool Has(string state, DateTime uploadedAt)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var record = conn.QuerySingle<ParticipantUpload>(@"
                    SELECT
                        state State,
                        uploaded_at UploadedAt
                    FROM participant_uploads
                    WHERE
                        lower(state) LIKE @state AND
                        uploaded_at = @uploadedAt",
                    new
                    {
                        state = state,
                        uploadedAt = uploadedAt
                    });

                result = record.State == state && record.UploadedAt == uploadedAt;
            }

            return result;
        }
    }
}
