using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Database;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnectionFactory<ParticipantsDb> _dbConnectionFactory;
        private readonly IParticipantBulkInsertHandler _bulkInsertHandler;
        private readonly ILogger<ParticipantDao> _logger;
        private readonly ICryptographyClient _cryptographyClient;
        public ParticipantDao(
            IDbConnectionFactory<ParticipantsDb> dbConnectionFactory,
            IParticipantBulkInsertHandler bulkInsertHandler,
            ILogger<ParticipantDao> logger,
            ICryptographyClient cryptographyClientt)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _bulkInsertHandler = bulkInsertHandler;
            _logger = logger;
            _cryptographyClient = cryptographyClientt;
            SqlMapper.AddTypeHandler(new DateRangeListHandler());
        }

        public async Task<IEnumerable<ParticipantDbo>> GetParticipants(string state, string ldsHash, Int64 uploadId)
        {
            using (var connection = await _dbConnectionFactory.Build(state))
            {
                return await connection
                    .QueryAsync<ParticipantDbo>(@"
                    SELECT
                        lds_hash LdsHash,
                        participant_id ParticipantId,
                        case_id CaseId,
                        participant_closing_date ParticipantClosingDate,
                        recent_benefit_issuance_dates RecentBenefitIssuanceDates,
                        vulnerable_individual VulnerableIndividual,
                        upload_id UploadId
                    FROM participants
                    WHERE lds_hash=@ldsHash
                        AND upload_id=@uploadId",
                        new
                        {
                            ldsHash = ldsHash,
                            uploadId = uploadId
                        }
                    );
            }
        }

        public async Task<ulong> AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                await connection.OpenAsync();

                try
                {
                    return await _bulkInsertHandler.LoadParticipants(participants, connection, "participants");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }
        }
        public async Task DeleteOldParticipantsExcept(string state,  Int64 uploadId)
        {
            using (var connection = await _dbConnectionFactory.Build(state))
            {
              

                var recordCount = await connection
                    .ExecuteAsync(@"
                    DELETE FROM participants
                    WHERE  upload_id<>@uploadId",
                        new
                        {
                            uploadId = uploadId
                        }
                    );

                if (String.IsNullOrEmpty(state))
                    _logger.LogInformation("Event Type : Outdated participant cleanup; Cleanup Time: {0} ; Records deleted :{1} ", DateTime.Now.ToString(), recordCount.ToString());
                else
                    _logger.LogInformation("Event Type : Outdated participant cleanup; Cleanup Time: {0} ; Records deleted :{1} ; State : {2}", DateTime.Now.ToString(), recordCount.ToString(), state);
            }
        }

    }
}
