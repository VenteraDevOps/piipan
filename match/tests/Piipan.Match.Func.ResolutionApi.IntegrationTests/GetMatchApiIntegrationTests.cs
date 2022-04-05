using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Xunit;
using Moq;
using Npgsql;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;
using Piipan.Match.Func.ResolutionApi.Models;
using Newtonsoft.Json;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class GetMatchApiIntegrationTests : DbFixture
    {
        static GetMatchApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IMatchRecordDao, MatchRecordDao>();
            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchResAggregator, MatchResAggregator>();

            services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new BasicPgConnectionFactory<CollaborationDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            var provider = services.BuildServiceProvider();

            var api = new GetMatchApi(
                provider.GetService<IMatchRecordDao>(),
                provider.GetService<IMatchResEventDao>(),
                provider.GetService<IMatchResAggregator>()
            );

            return api;
        }

        static Mock<HttpRequest> MockGetRequest(string matchId = "foo")
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
                { "X-Initiating-State", "ea"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async void GetMatch_Returns404IfNotFound()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();

            var matchId = "foo";
            var api = Construct();
            var mockRequest = MockGetRequest(matchId);
            var mockLogger = Mock.Of<ILogger>();

            // Act
            var response = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as NotFoundObjectResult;

            // Assert
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async void GetMatch_ReturnsCorrectSchemaIfFound()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();

            var matchId = "ABC";
            var api = Construct();
            var mockRequest = MockGetRequest(matchId);
            var mockLogger = Mock.Of<ILogger>();

            // insert into database
            var match = new MatchRecordDbo() {
                CreatedAt = DateTime.UtcNow,
                Data = "{\"State\": \"bb\", \"CaseId\": \"GHI\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"JKL\", \"ParticipantClosingDate\": \"2021-02-28\", \"ProtectLocation\": true, \"RecentBenefitIssuanceDates\": [{\"start\": \"2021-03-01\", \"end\":\"2021-03-31\"}]}",
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                Input = "{\"CaseId\": \"ABC\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"DEF\"}",
                MatchId = matchId,
                States = new string[] { "ea", "bb" }
            };
            Insert(match);

            // Act
            var response = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as JsonResult;
            string resString = JsonConvert.SerializeObject(response.Value);

            // Assert
            Assert.Equal(200, response.StatusCode);
            // Assert Participant Data
            var expected = "{\"data\":{\"dispositions\":[{\"initial_action_at\":null,\"invalid_match\":false,\"final_disposition\":null,\"protect_location\":null,\"state\":\"ea\"},{\"initial_action_at\":null,\"invalid_match\":false,\"final_disposition\":null,\"protect_location\":null,\"state\":\"bb\"}],\"initiator\":\"ea\",\"match_id\":\"ABC\",\"participants\":[{\"case_id\":\"GHI\",\"participant_closing_date\":\"2021-02-28\",\"participant_id\":\"JKL\",\"recent_benefit_issuance_dates\":[{\"start\":\"2021-03-01\",\"end\":\"2021-03-31\"}],\"state\":\"bb\"},{\"case_id\":\"ABC\",\"participant_closing_date\":null,\"participant_id\":\"DEF\",\"recent_benefit_issuance_dates\":[],\"state\":\"ea\"}],\"states\":[\"ea\",\"bb\"],\"status\":\"open\"}}";
            Assert.Equal(expected, resString);
        }
        // When match res events are added, GetMatch response should update accordingly
        [Fact]
        public async void GetMatch_ShowsUpdatedData()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();

            var matchId = "ABC";
            var api = Construct();
            var mockRequest = MockGetRequest(matchId);
            var mockLogger = Mock.Of<ILogger>();

            // insert into database
            var match = new MatchRecordDbo() {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "bb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // Act
            var response = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as JsonResult;

            // Assert first request
            Assert.Equal(200, response.StatusCode);
            var resBody = response.Value as ApiResponse;
            Assert.False(resBody.Data.Dispositions[0].InvalidMatch);

            // Act again
            // creating an "invalid match" match event results in newly pulled Match request having invalid_match = true
            var mre = new MatchResEventDbo() {
                MatchId = matchId,
                ActorState = "ea",
                Actor = "user",
                Delta = "{ \"invalid_match\": true }"
            };
            InsertMatchResEvent(mre);
            var nextResponse = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as JsonResult;

            // Assert next request
            var nextResBody = nextResponse.Value as ApiResponse;
            Assert.Equal(200, nextResponse.StatusCode);
            // now this disposition's invalid flag should be true
            Assert.True(nextResBody.Data.Dispositions[0].InvalidMatch);
        }
    }
}
