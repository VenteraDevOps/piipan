using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Shared.Database;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;
using Piipan.States.Func.Api;
using Piipan.States.Func.Api.IntegrationTests;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("StateApiTests")]
    public class StateApiIntegrationTests : DbFixture
    {
        static StateApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IStateInfoDao, StateInfoDao>();

            services.AddTransient<IDbConnectionFactory<StateInfoDb>>(s =>
            {
                return new BasicPgConnectionFactory<StateInfoDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            var provider = services.BuildServiceProvider();

            var api = new StateApi(
                provider.GetService<IStateInfoDao>()
            );

            return api;
        }

        static Mock<HttpRequest> MockGetRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async void GetStates_ReturnsEmptyListIfNotFound()
        {
            // Arrange
            // clear databases
            ClearStates();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();

            // Act
            var response = (await api.GetStates(mockRequest.Object, mockLogger) as JsonResult).Value as StatesInfoResponse;

            // Assert
            Assert.Empty(response.Results);
        }

        [Fact]
        public async void GetStates_ReturnsCorrectSchemaIfFound()
        {
            // Arrange
            // clear databases
            ClearStates();

            var api = Construct();
            var mockRequest = MockGetRequest();
            var mockLogger = Mock.Of<ILogger>();
            // insert into database
            var match = new StateInfoDbo()
            {
                Id = "1",
                Email = "ea-test@usda.example",
                State = "Echo Alpha",
                StateAbbreviation = "EA",
                Phone = "123-123-1234",
                Region = "TEST"
            };

            var match2 = new StateInfoDbo()
            {
                Id = "2",
                Email = "eb-test@usda.example",
                State = "Echo Bravo",
                StateAbbreviation = "EB",
                Phone = "123-123-1234",
                Region = "TEST"
            };
            Insert(match);
            Insert(match2);

            // Act
            var response = await api.GetStates(mockRequest.Object, mockLogger) as JsonResult;
            var responseRecords = (response.Value as StatesInfoResponse).Results;
            string resString = JsonConvert.SerializeObject(response.Value);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(2, responseRecords.Count());
            // Assert Participant Data
            var expected = "{\"data\":[{\"id\":\"1\",\"state\":\"Echo Alpha\",\"state_abbreviation\":\"EA\",\"email\":\"ea-test@usda.example\",\"phone\":\"123-123-1234\",\"region\":\"TEST\"},{\"id\":\"2\",\"state\":\"Echo Bravo\",\"state_abbreviation\":\"EB\",\"email\":\"eb-test@usda.example\",\"phone\":\"123-123-1234\",\"region\":\"TEST\"}]}";
            Assert.Equal(expected, resString);
        }
    }
}
