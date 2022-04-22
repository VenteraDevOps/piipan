using System;
using System.Linq;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Piipan.Participants.Core.Models;
using Piipan.Shared.TestFixtures;

namespace Piipan.Participants.Core.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : ParticipantsDbFixture
    {

        public void Insert(ParticipantDbo participant)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(participant);
                parameters.Add("UploadId", lastval);

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_issuance_dates, vulnerable_individual)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitIssuanceDates::daterange[], @VulnerableIndividual)",
                    parameters);

                conn.Close();
            }
        }

        public void InsertUpload()
        {
            var factory = NpgsqlFactory.Instance;
            
            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                conn.Execute("INSERT INTO uploads(created_at, publisher,upload_identifier) VALUES(now() at time zone 'utc', current_user ,'test-etag')");
                conn.Close();
            }
        }

        public bool HasParticipant(ParticipantDbo participant)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var record = conn.Query<ParticipantDbo>(@"
                    SELECT lds_hash LdsHash,
                        participant_id ParticipantId,
                        case_id CaseId,
                        participant_closing_date ParticipantClosingDate,
                        recent_benefit_issuance_dates RecentBenefitIssuanceDates,
                        vulnerable_individual VulnerableIndividual,
                        upload_id UploadId
                    FROM participants
                    WHERE lds_hash=@LdsHash", participant).FirstOrDefault();

                if (record == null)
                {
                    result = false;
                }
                else
                {
                    result = record.Equals(participant);
                }

                conn.Close();
            }
            return result;
        }
    }
}
