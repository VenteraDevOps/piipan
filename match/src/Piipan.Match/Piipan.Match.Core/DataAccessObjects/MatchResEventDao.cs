using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Match.Core.DataAccessObjects
{
    /// <summary>
    /// Data Access Object for match resolution events
    /// </summary>
    public class MatchResEventDao : IMatchResEventDao
    {
        private readonly IDbConnectionFactory<CollaborationDb> _dbConnectionFactory;
        private readonly ILogger<MatchResEventDao> _logger;

        /// <summary>
        /// Initializes a new instance of MatchResEventDao
        /// </summary>
        public MatchResEventDao(
            IDbConnectionFactory<CollaborationDb> dbConnectionFactory,
            ILogger<MatchResEventDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new match resolution event to the database.
        /// </summary>
        /// <param name="record">The MatchResEventDbo object that will be added to the database.</param>
        /// <returns>ID for inserted record</returns>
        public async Task<int> AddRecord(MatchResEventDbo record)
        {
            const string sql = @"
                INSERT INTO match_res_events
                (
                    inserted_at,
                    match_id,
                    actor,
                    actor_state,
                    delta
                )
                VALUES
                (
                    now() at time zone 'utc',
                    @MatchId,
                    @Actor,
                    @ActorState,
                    @Delta::jsonb
                )
                RETURNING id
            ;";

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteAsync(sql, record);
            }
        }

        /// <summary>
        /// Finds all match resolution events related to a given match ID,
        /// and orders results by oldest event first
        /// </summary>
        /// <param name="matchId">The given match ID</param>
        /// <param name="timestamp">Will select all events prior to this timestamp. Pass the current timestamp to get all related records.</param>
        /// <returns>IEnumerable of IMatchResEvents</returns>
        public async Task<IEnumerable<IMatchResEvent>> GetRecords(
            string matchId,
            DateTime timestamp
        )
        {
            if (timestamp.Kind != DateTimeKind.Utc)
            {
                timestamp = timestamp.ToUniversalTime();
            }
            const string sql = @"
                SELECT
                    id,
                    match_id,
                    inserted_at,
                    actor,
                    actor_state,
                    delta::jsonb
                FROM match_res_events
                WHERE
                    match_id=@MatchId AND
                    inserted_at < @Timestamp
                ORDER BY inserted_at asc
                ;";
            var parameters = new { MatchId = matchId, Timestamp = timestamp };

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.QueryAsync<MatchResEventDbo>(sql, parameters);
            }
        }
    }
}
