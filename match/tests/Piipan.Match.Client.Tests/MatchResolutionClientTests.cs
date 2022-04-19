using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.Http;
using Xunit;

namespace Piipan.Match.Client.Tests
{
    public class MatchResolutionClientTests
    {
        [Fact]
        public async Task GetMatch_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new MatchResApiResponse
            {
                Data = new MatchResRecord
                {
                    Dispositions = new Disposition[] { Mock.Of<Disposition>() },
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    MatchId = "m123456",
                    Participants = new Participant[] { Mock.Of<Participant>() },
                    Status = MatchRecordStatus.Open
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<MatchResolutionClient>>();
            apiClient
                .Setup(m => m.TryGetAsync<MatchResApiResponse>("matches/m123456"))
                .ReturnsAsync((expectedResponse, 200));

            var client = new MatchResolutionClient(apiClient.Object);

            // Act
            var response = await client.GetMatch("m123456");

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
