using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;
using Piipan.Shared.Database;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    /// <summary>
    /// Data access object for Participant Upload records in Metrics database
    /// </summary>
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

        private string GenerateWhereClause(ParticipantUploadRequestFilter filter)
        {
            string whereClause = "";
            if (!string.IsNullOrEmpty(filter.State))
            {
                whereClause += $"lower(state) = lower(@state) AND ";
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                whereClause += $"lower(status) = lower(@status) AND ";
            }
            if (filter.StartDate != null)
            {
                whereClause += $"uploaded_at >= @dateFrom AND ";
            }
            if (filter.EndDate != null)
            {
                whereClause += $"uploaded_at < @dateTo AND ";
            }
            if (whereClause.EndsWith(" AND "))
            {
                // " AND " takes up 5 characters. Remove the last " AND "
                whereClause = $" WHERE {whereClause[..^5]}";
            }
            return whereClause;
        }
        private object GenerateQueryObject(ParticipantUploadRequestFilter filter)
        {
            return new
            {
                state = filter.State,
                dateFrom = filter.StartDate?.AddHours(-1 * filter.HoursOffset),
                // We add a day here to make sure to grab all entries in the database for the day that they asked for
                dateTo = filter.EndDate?.AddDays(1).AddHours(-1 * filter.HoursOffset),
                status = filter.Status,
                limit = filter.PerPage,
                offset = filter.PerPage * (filter.Page - 1)
            };
        }



        /// <summary>
        /// Gets a count of uploads performed overall, or by a specific state
        /// </summary>
        /// <param name="state">The state being queried for a count of uploads performed</param>
        /// <returns>The number of uploads</returns>
        public async Task<Int64> GetUploadCount(ParticipantUploadRequestFilter filter)
        {
            var sql = "SELECT COUNT(*) from participant_uploads" + GenerateWhereClause(filter);

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteScalarAsync<Int64>(sql, GenerateQueryObject(filter));
            }
        }

        private string GenerateStatisticsWhereClause(ParticipantUploadStatisticsRequest request)
        {
            string whereClause = "";
            if (request.StartDate != null)
            {
                whereClause += $"uploaded_at >= @dateFrom AND ";
            }
            if (request.EndDate != null)
            {
                whereClause += $"uploaded_at < @dateTo AND ";
            }
            if (whereClause.EndsWith(" AND "))
            {
                // " AND " takes up 5 characters. Remove the last " AND "
                whereClause = $" WHERE {whereClause[..^5]}";
            }
            return whereClause;
        }

        private object GenerateStatisticsQueryObject(ParticipantUploadStatisticsRequest request)
        {
            return new
            {
                dateFrom = request.StartDate?.AddHours(-1 * request.HoursOffset),
                // We add a day here to make sure to grab all entries in the database for the day that they asked for
                dateTo = request.EndDate?.AddDays(1).AddHours(-1 * request.HoursOffset),
            };
        }

        private record ParticipantUploadStatisticsRow
        {
            public int Count { get; init; }
            public string Status { get; init; }
        }

        public async Task<ParticipantUploadStatistics> GetUploadStatistics(ParticipantUploadStatisticsRequest request)
        {
            var sql = @"
                SELECT
                    count(distinct State) Count,
                    status Status
                FROM participant_uploads";

            sql += GenerateStatisticsWhereClause(request);

            sql += " GROUP BY Status";

            using (var connection = await _dbConnectionFactory.Build())
            {
                var rows = await connection.QueryAsync<ParticipantUploadStatisticsRow>(sql, GenerateStatisticsQueryObject(request));
                ParticipantUploadStatistics statistics = new ParticipantUploadStatistics
                {
                    TotalComplete = rows.FirstOrDefault(n => n.Status.Equals("Complete", StringComparison.OrdinalIgnoreCase))?.Count ?? 0,
                    TotalFailure = rows.FirstOrDefault(n => n.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))?.Count ?? 0
                };
                return statistics;
            }
        }

        /// <summary>
        /// Retrieves all upload records, or all upload records for a specific state
        /// </summary>
        /// <param name="state">The state being queried for uploads</param>
        /// <param name="limit">The number or records desired to be retrieved</param>
        /// <param name="offset">The offset in the set of matching upload records to be used as the start for the set of records to return </param>
        /// <returns>Task of IEnumerable of PartcipantUploads</returns>
        public async Task<IEnumerable<ParticipantUpload>> GetUploads(ParticipantUploadRequestFilter filter)
        {
            var sql = @"
                SELECT
                    state State,
                    uploaded_at UploadedAt,
                    completed_at CompletedAt,
                    status Status,
                    upload_identifier UploadIdentifer,
                    participants_uploaded ParticipantsUploaded,
                    error_message ErrorMessage
                FROM participant_uploads";

            sql += GenerateWhereClause(filter);

            sql += " ORDER BY uploaded_at DESC";
            sql += $" LIMIT @limit";
            sql += $" OFFSET @offset";

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.QueryAsync<ParticipantUpload>(sql, GenerateQueryObject(filter));
            }
        }

        /// <summary>
        /// Returns the latest successful Participant Upload record for each State
        /// </summary>
        /// <returns>Task of IEnumerable of PartcipantUploads</returns>
        public async Task<IEnumerable<ParticipantUpload>> GetLatestSuccessfulUploadsByState()
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return (await connection.QueryAsync(@"
                    SELECT 
                        state, 
                        max(completed_at) as completed_at
                    FROM participant_uploads
					where status='COMPLETE'
                    GROUP BY state
                    ORDER BY completed_at ASC
                ;")).Select(o => new ParticipantUpload
                {
                    UploadIdentifier = o.UploadIdentifier,
                    State = o.state,
                    CompletedAt = o.completed_at
                });
            }
        }

        /// <summary>
        /// Adds a new Participant Upload record to the database.
        /// </summary>
        /// <param name="newUploadDbo">The ParticipantUploadDbo object that will be added to the database.</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> AddUpload(ParticipantUploadDbo newUploadDbo)
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteAsync(@"
                    INSERT INTO participant_uploads 
                    (
                        state, 
                        uploaded_at,
                        status,
                        upload_identifier
                    ) 
                    VALUES
                    (
                        @state, 
                        @uploaded_at,
                        @status,
                        @upload_identifier
                    );",
                    new
                    {
                        state = newUploadDbo.State,
                        uploaded_at = newUploadDbo.UploadedAt,
                        status = newUploadDbo.Status,
                        upload_identifier = newUploadDbo.UploadIdentifier
                    });
            }
        }

        /// <summary>
        /// Updates a Participant Upload record to the database.
        /// </summary>
        /// <param name="uploadDbo">The ParticipantUploadDbo object that will be updated in the database</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> UpdateUpload(ParticipantUploadDbo uploadDbo)
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteAsync(@"
                    UPDATE participant_uploads 
                    SET state=@state, uploaded_at=@uploaded_at, completed_at=@completed_at, status=@status, 
                        participants_uploaded=@participants_uploaded, error_message=@error_message
                    WHERE upload_identifier=@upload_identifier;",
                    new
                    {
                        state = uploadDbo.State,
                        uploaded_at = uploadDbo.UploadedAt,
                        completed_at = uploadDbo.CompletedAt,
                        status = uploadDbo.Status,
                        upload_identifier = uploadDbo.UploadIdentifier,
                        participants_uploaded = uploadDbo.ParticipantsUploaded,
                        error_message = uploadDbo.ErrorMessage
                    });
            }
        }
    }

}
