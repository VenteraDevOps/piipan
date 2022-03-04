using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Xunit;
using Moq;

namespace Piipan.Match.Core.Tests.Builders
{
    public class MatchResAggregatorTests
    {
        public string _matchId = new MatchIdService().GenerateId();

        public MatchRecordDbo _match { get; set; }

        public List<MatchResEventDbo> _match_res_events { get; set; }

        public MatchResAggregatorTests()
        {
            _match = new MatchRecordDbo()
            {
                MatchId = _matchId,
                States = new string[] { "ea", "eb" }
            };

            _match_res_events = new List<MatchResEventDbo>()
            {
                new MatchResEventDbo()
                {
                    Id = 123,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 01, 01),
                    Actor = "ea@example.com",
                    ActorState = "ea",
                    Delta = "{ initial_action_at: '2022-01-01T00:00:00.0000000' }"
                },
                new MatchResEventDbo()
                {
                    Id = 124,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 01, 02),
                    Actor = "ea@example.com",
                    ActorState = "ea",
                    Delta = "{ 'final_disposition': 'final disposition for ea' }"
                },
                new MatchResEventDbo()
                {
                    Id = 456,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 01),
                    Actor = "eb@example.com",
                    ActorState = "eb",
                    Delta = "{ 'invalid_match': false, initial_action_at: '2022-02-01T00:00:00.0000000' }"
                },
                new MatchResEventDbo()
                {
                    Id = 125,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 03),
                    Actor = "eb@example.com",
                    ActorState = "eb",
                    Delta = "{ 'invalid_match': true, 'final_disposition': 'final disposition for eb' }"
                },
                new MatchResEventDbo()
                {
                    Id = 789,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 04),
                    Actor = "system",
                    ActorState = null,
                    Delta = "{ 'status': 'closed' }"
                }
            };
        }

        public MatchResAggregator Builder()
        {
            return new MatchResAggregator();
        }

        // returns correct match status
        [Fact]
        public void Build_ReturnsCorrectStatus()
        {
            // Act
            var result = Builder().Build(_match, _match_res_events);

            // Assert
            Assert.Equal("closed", result.Status);
        }
        // returns correct order
        [Fact]
        public void Build_ReturnsCorrectOrder()
        {
            // Act
            var reversed = new List<MatchResEventDbo>(_match_res_events);
            reversed = Enumerable.Reverse(reversed).ToList();
            var result = Builder().Build(_match, reversed);

            // Assert
            Assert.True(result.Dispositions[1].InvalidMatch);
        }
        // returns correct state aggregate data
        // final disposition, etc for each property
        [Fact]
        public void Build_ReturnsCorrectStateData()
        {
            // Act
            var result = Builder().Build(_match, _match_res_events);
            var eaObj = result.Dispositions[0];
            var ebObj = result.Dispositions[1];

            // Assert
            // Length
            Assert.Equal(2, result.Dispositions.Count());

            // state name
            Assert.Equal("ea", eaObj.State);
            Assert.Equal("eb", ebObj.State);

            // initial action at
            Assert.Equal(new DateTime(2022, 01, 01), eaObj.InitialActionAt);
            Assert.Equal(new DateTime(2022, 02, 01), ebObj.InitialActionAt);

            // invalid match
            Assert.False(eaObj.InvalidMatch);
            Assert.True(ebObj.InvalidMatch);

            // final disposition
            Assert.Equal("final disposition for ea", eaObj.FinalDisposition);
            Assert.Equal("final disposition for eb", ebObj.FinalDisposition);

            // protect location
            Assert.Null(eaObj.ProtectLocation);
            Assert.Null(ebObj.ProtectLocation);
        }
    }
}
