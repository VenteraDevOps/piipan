using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Metrics.Api;
using Piipan.Shared.TestFixtures;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    /// <summary>
    /// Test fixture for match records integration testing.
    /// Creates a fresh matches tables, dropping it when testing is complete.
    /// </summary>
    public class DbFixture : MatchesDbFixture
    {

        public void Insert(MatchRecordDbo record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute(@"
                    INSERT INTO matches
                    (
                        created_at,
                        match_id,
                        initiator,
                        states,
                        hash,
                        hash_type,
                        input,
                        data
                    )
                    VALUES
                    (
                        now() at time zone 'utc',
                        @MatchId,
                        @Initiator,
                        @States,
                        @Hash,
                        @HashType::hash_type,
                        @Input::jsonb,
                        @Data::jsonb
                    )", record);

                conn.Close();
            }
        }

        public void InsertMatchResEvent(MatchResEventDbo record)
        {
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                conn.Execute(@"
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
                    )", record);

                conn.Close();
            }
        }

        public bool HasRecord(MatchRecordDbo record)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var row = conn.QuerySingle<MatchRecordDbo>(@"
                    SELECT match_id,
                        created_at,
                        initiator,
                        hash,
                        hash_type::text,
                        states,
                        input,
                        data
                    FROM matches
                    WHERE match_id=@MatchId", record);

                result = row.Equals(record);

                conn.Close();
            }

            return result;
        }

        public bool HasMatchResEvent(MatchResEventDbo record)
        {
            var result = false;
            var factory = NpgsqlFactory.Instance;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var row = conn.QuerySingle<MatchResEventDbo>(@"
                    SELECT
                        id,
                        inserted_at,
                        match_id,
                        actor,
                        actor_state,
                        delta
                    FROM match_res_events
                    WHERE id=@Id
                    ", record);

                result = row.Id.Equals(record.Id);

                conn.Close();
            }

            return result;
        }

        public async Task<IEnumerable<IMatchResEvent>> GetEvents(string matchId)
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
                ORDER BY inserted_at asc
                ;";
            var parameters = new
            {
                MatchId = matchId,
            };
            var factory = NpgsqlFactory.Instance;
            IEnumerable<MatchResEventDbo> result;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = await conn.QueryAsync<MatchResEventDbo>(sql, parameters);
                conn.Close();
            }
            return result;
        }
        public async Task<IEnumerable<ParticipantMatchMetrics>> GetMatchMetrics(string matchId)
        {
            const string sql = @"
               SELECT
                        match_id,
                        match_created_at,
                        init_state,
                        init_state_invalid_match,
                        init_state_invalid_match_reason,
                        init_state_initial_action_taken,
                        init_state_initial_action_at,
                        init_state_final_disposition,
                        init_state_final_disposition_date,
                        init_state_vulnerable_individual,
                        matching_state,
                        matching_state_invalid_match,
                        matching_state_invalid_match_reason,
                        matching_state_initial_action_taken,
                        matching_state_initial_action_at,
                        matching_state_final_disposition,
                        matching_state_final_disposition_date,
                        matching_state_vulnerable_individual,
                        match_status
                 FROM
                    participant_matches
                 WHERE
                    match_id = @MatchId
                ;";
            var parameters = new
            {
                MatchId = matchId,
            };
            var factory = NpgsqlFactory.Instance;
            IEnumerable<ParticipantMatchMetrics> result;

            using (var conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                result = await conn.QueryAsync<ParticipantMatchMetrics>(sql, parameters);
                conn.Close();
            }
            return result;
        }
    }
}
