using Moq;
using Piipan.Shared.Http;
using Piipan.States.Api.Models;
using Xunit;

namespace Piipan.States.Client.Tests
{
    public class StatesClientTests
    {
        [Fact]
        public async Task GetStates_ReturnsApiClientResponse()
        {
            // Arrange
            var expectedResponse = new StatesInfoResponse
            {
                Results = new List<StateInfoResponseData>
                {
                    new StateInfoResponseData
                    {
                        Email = "ea-test@usda.example",
                        Phone = "123-456-7890",
                        Id = "123",
                        Region = "MWO",
                        State = "Echo Alpha",
                        StateAbbreviation = "EA"
                    },
                    new StateInfoResponseData
                    {
                        Email = "eb-test@usda-gov",
                        Phone = "123-456-7890",
                        Id = "456",
                        Region = "MWO",
                        State = "Echo Bravo",
                        StateAbbreviation = "EB"
                    }
                }
            };

            var apiClient = new Mock<IAuthorizedApiClient<StatesClient>>();
            apiClient
                .Setup(m => m.GetAsync<StatesInfoResponse>("states"))
                .ReturnsAsync(expectedResponse);

            var client = new StatesClient(apiClient.Object);

            // Act
            var response = await client.GetStates();

            // Assert
            Assert.Equal(expectedResponse, response);
        }
    }
}
