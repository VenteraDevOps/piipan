using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
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
    }
}