using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class ParticipantDao : IParticipantDao
    {
        private readonly IDbConnectionFactory<ParticipantsDb> _dbConnectionFactory;
        private readonly ILogger<ParticipantDao> _logger;

        public ParticipantDao(
            IDbConnectionFactory<ParticipantsDb> dbConnectionFactory,
            ILogger<ParticipantDao> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
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
                        benefits_end_date BenefitsEndDate,
                        recent_benefit_months RecentBenefitMonths,
                        protect_location ProtectLocation,
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

        public async Task AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            const string sql = @"
                INSERT INTO participants
                (
                    lds_hash,
                    upload_id,
                    case_id,
                    participant_id,
                    benefits_end_date,
                    recent_benefit_months,
                    protect_location
                )
                VALUES
                (
                    @LdsHash,
                    @UploadId,
                    @CaseId,
                    @ParticipantId,
                    @BenefitsEndDate,
                    @RecentBenefitMonths::date[],
                    @ProtectLocation
                )
            ";

            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                // A performance optimization. Dapper will open/close around invidual
                // calls if it is passed a closed connection. 
                await connection.OpenAsync();
                foreach (var participant in participants)
                {
                    _logger.LogDebug(
                        $"Adding participant for upload {participant.UploadId} with LDS Hash: {participant.LdsHash}");

                    await connection.ExecuteAsync(sql, participant);
                }
            }
        }
    }
}
