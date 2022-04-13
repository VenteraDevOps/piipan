using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
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

        public async Task AddParticipants(IEnumerable<ParticipantDbo> participants)
        {
            const string sql = @"
                INSERT INTO participants
                (
                    lds_hash,
                    upload_id,
                    case_id,
                    participant_id,
                    participant_closing_date,
                    recent_benefit_issuance_dates,
                    vulnerable_individual
                )
                VALUES
                (
                    @LdsHash,
                    @UploadId,
                    @CaseId,
                    @ParticipantId,
                    @ParticipantClosingDate,
                    @RecentBenefitIssuanceDates::daterange[],
                    @VulnerableIndividual
                )
            ";

            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                // A performance optimization. Dapper will open/close around invidual
                // calls if it is passed a closed connection. 
                await connection.OpenAsync();

                try
                {
                    foreach (var participant in participants)
                    {
                        _logger.LogDebug(
                            $"Adding participant for upload {participant.UploadId} with LDS Hash: {participant.LdsHash}");
                        await connection.ExecuteAsync(sql, participant);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }
        }
    }
}
