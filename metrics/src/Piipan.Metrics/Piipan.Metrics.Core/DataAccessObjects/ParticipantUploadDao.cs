using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Shared.Database;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public class ParticipantUploadDao : IParticipantUploadDao
    {
        private readonly IDbConnectionFactory<MetricsDb> _dbConnectionFactory;
        private readonly ILogger<ParticipantUploadDao> _logger;

        public ParticipantUploadDao(
            IDbConnectionFactory<MetricsDb> dbConnectionFactory,
            ILogger<ParticipantUploadDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<Int64> GetUploadCount(string? state)
        {
            var sql = "SELECT COUNT(*) from participant_uploads";
            if (!String.IsNullOrEmpty(state))
            {
                sql += $" WHERE lower(state) LIKE @state";
            }

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteScalarAsync<Int64>(sql, new { state = state });
            }
        }

        public async Task<IEnumerable<ParticipantUpload>> GetUploads(string? state, int limit, int offset = 0)
        {
            var sql = @"
                SELECT
                    state State,
                    uploaded_at UploadedAt
                FROM participant_uploads";

            if (!String.IsNullOrEmpty(state))
            {
                sql += $" WHERE lower(state) LIKE lower(@state)";
            }

            sql += " ORDER BY uploaded_at DESC";
            sql += $" LIMIT @limit";
            sql += $" OFFSET @offset";

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection
                    .QueryAsync<ParticipantUpload>(sql, new { state = state, limit = limit, offset = offset });
            }
        }

        public async Task<IEnumerable<ParticipantUpload>> GetLatestUploadsByState()
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return (await connection.QueryAsync(@"
                    SELECT 
                        state, 
                        max(uploaded_at) as uploaded_at
                    FROM participant_uploads
                    GROUP BY state
                    ORDER BY uploaded_at ASC
                ;")).Select(o => new ParticipantUpload
                {
                    State = o.state,
                    UploadedAt = o.uploaded_at
                });
            }
        }

        public async Task<int> AddUpload(string state, DateTime uploadedAt)
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteAsync(@"
                    INSERT INTO participant_uploads 
                    (
                        state, 
                        uploaded_at
                    ) 
                    VALUES
                    (
                        @state, 
                        @uploaded_at
                    );",
                    new
                    {
                        state = state,
                        uploaded_at = uploadedAt
                    });
            }
        }
    }
}
