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
        public async Task<int> AddEvent(MatchResEventDbo record)
        {
            const string sql = @"
                INSERT INTO match_res_events
                (
                    match_id,
                    actor,
                    actor_state,
                    delta
                )
                VALUES
                (
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
        /// Finds all match resolution events related to a match record
        /// </summary>
        /// <param name="matchId">The given match ID</param>
        /// <param name="sortByAsc">Boolean indicating ascending sort order, defaults to true. Argument of false is descending order</param>
        /// <returns>Task of IEnumerable of IMatchResEvents</returns>
        public async Task<IEnumerable<IMatchResEvent>> GetEvents(
            string matchId,
            bool sortByAsc = true
        )
        {
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
                    match_id=@MatchId
                ORDER BY
                CASE
                    WHEN @SortByAsc = true THEN inserted_at
                END ASC,
                CASE
                    WHEN @SortByAsc = false THEN inserted_at
                END DESC
                ;";
            var parameters = new {
                MatchId = matchId,
                SortByAsc = sortByAsc
            };

            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.QueryAsync<MatchResEventDbo>(sql, parameters);
            }
        }
    }
}
