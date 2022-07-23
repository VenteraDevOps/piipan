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
    public class ParticipantSearchDao : IParticipantSearchDao
    {
        private readonly IDbConnectionFactory<MetricsDb> _dbConnectionFactory;
        private readonly ILogger<ParticipantSearchDao> _logger;

        public ParticipantSearchDao(
            IDbConnectionFactory<MetricsDb> dbConnectionFactory,
            ILogger<ParticipantSearchDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

       
        /// <summary>
        /// Adds a new Participant Upload record to the database.
        /// </summary>
        /// <param name="newSearchDbo">The ParticipantUploadDbo object that will be added to the database.</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> AddParticipantSearchRecord(ParticipantSearchDbo newSearchDbo)
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection.ExecuteAsync(@"
                    INSERT INTO participant_search
                    (
                        state, 
                        search_reason,
                        search_from,
                        match_creation,
                        match_count,
                        searched_at

                   ) 
                    VALUES
                    (
                        @state, 
                        @search_reason,
                        @search_from,
                        @match_creation,
                        @match_count,
                        @searched_at
                    );",
                    new
                    {
                        state = newSearchDbo.State,
                        search_reason = newSearchDbo.SearchReason,
                        search_from = newSearchDbo.SearchFrom,
                        match_creation = newSearchDbo.MatchCreation,
                        match_count = newSearchDbo.MatchCount,
                        searched_at = newSearchDbo.SearchedAt
                    });
            }
        }
     }
}
