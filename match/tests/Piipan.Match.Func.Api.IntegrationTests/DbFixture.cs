using System;
using System.Threading;
using Dapper;
using Npgsql;
using Piipan.Match.Core.Models;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Match.Func.Api.IntegrationTests
{
    /// <summary>
    /// Test fixture for per-state match API database integration testing.
    /// Creates a fresh set of participants and uploads tables, dropping them
    /// when testing is complete.
    /// </summary>
    public class DbFixture : TestDbFixture
    {
        public int CountMatchRecords()
        {
            int count;

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM matches");

                conn.Close();
            }

            return count;
        }

        public MatchRecordDbo GetMatchRecord(string matchId)
        {
            MatchRecordDbo record;

            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = CollabConnectionString;
                conn.Open();

                record = conn.QuerySingle<MatchRecordDbo>(
                    @"SELECT
                        match_id,
                        initiator,
                        states,
                        hash,
                        hash_type::text,
                        input::jsonb,
                        data::jsonb
                    FROM matches
                    WHERE match_id=@matchId;",
                    new { matchId = matchId });

                conn.Close();
            }

            return record;
        }

        public void Insert(ParticipantDbo record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                Int64 lastval = conn.ExecuteScalar<Int64>("SELECT MAX(id) FROM uploads");
                DynamicParameters parameters = new DynamicParameters(record);
                parameters.Add("UploadId", lastval);

                conn.Execute(@"
                    INSERT INTO participants(lds_hash, upload_id, case_id, participant_id, participant_closing_date, recent_benefit_months, protect_location)
                    VALUES (@LdsHash, @UploadId, @CaseId, @ParticipantId, @ParticipantClosingDate, @RecentBenefitMonths::date[], @ProtectLocation)",
                    parameters);

                conn.Close();
            }
        }
    }
}
