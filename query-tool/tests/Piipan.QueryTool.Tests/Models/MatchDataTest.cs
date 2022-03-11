using System;
using Moq;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class MatchDataTest
    {

        [Fact]
        public void TestStoreDataOnMatch()
        {
            // Given
            var Match = new MatchData()
            {
                MatchId = "m123456",
                Status = "Open"
            };
            // When

            // Then
            Assert.Equal("m123456", Match.MatchId);
            Assert.Equal("Open", Match.Status);
    

        }


        [Fact]
        public void TestAssignMatchData()
        {
            // Given
            var Match = new MatchData()
            {
                MatchId = "m123456",
                Status = "Close"

            };
            // When
            MatchData Match2 = Match;
            // Then
            Assert.Equal("m123456", Match2.MatchId);
            Assert.Equal("Close", Match.Status);
        }

        [Fact]
        public void TestMatchDataToJson()
        {
            // Given
            var Match = new MatchData()
            {
                MatchId = "m123456",
                Status = "Close"

            };

            // When
        
            // Then
            Assert.Equal("{\n  \"match_id\": \"m123456\",\n  \"match_status\": \"Close\"\n}", Match.ToJson());
        }
    }
}


