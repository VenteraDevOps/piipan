using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;
using PostgreSQLCopyHelper;

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
            using (var connection = await _dbConnectionFactory.Build() as NpgsqlConnection)
            {
                await connection.OpenAsync();

                // Use NodaTime as a convenience to handle converting DateInterval[]
                // to a valid daterange[] input format
                connection.TypeMapper.UseNodaTime();

                try
                {
                    var copyHelper = new PostgreSQLCopyHelper<ParticipantDbo>("participants")
                        .MapText("lds_hash", p => p.LdsHash)
                        .MapText("participant_id", p => p.ParticipantId)
                        .MapText("case_id", p => p.CaseId)
                        .MapInteger("upload_id", p => (int)p.UploadId)
                        .MapDate("participant_closing_date", p => p.ParticipantClosingDate)
                        .Map("recent_benefit_issuance_dates", p =>
                            p.RecentBenefitIssuanceDates.Select(range =>
                                new DateInterval(
                                    LocalDate.FromDateTime(range.Start),
                                    LocalDate.FromDateTime(range.End)
                                )
                            ).ToArray<DateInterval>()
                        )
                        .MapBoolean("protect_location", p => p.ProtectLocation);

                    _logger.LogDebug("Bulk inserting participant upload");
                    var result = await copyHelper.SaveAllAsync(connection, participants);
                    _logger.LogDebug("Completed bulk insert of {0} participant records", result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }
        }
    }
}
