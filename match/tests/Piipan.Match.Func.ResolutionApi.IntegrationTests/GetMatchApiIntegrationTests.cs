using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Shared.Database;
using Piipan.States.Core.DataAccessObjects;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class GetMatchApiIntegrationTests : DbFixture
    {
        static GetMatchApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ia");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IMatchRecordDao, MatchRecordDao>();
            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchResAggregator, MatchResAggregator>();
            services.AddTransient<IStateInfoDao, StateInfoDao>();
            services.AddTransient<IMemoryCache, MemoryCache>();

            services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new BasicPgConnectionFactory<CollaborationDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });
            services.AddTransient<IDbConnectionFactory<StateInfoDb>>(s =>
            {
                return new BasicPgConnectionFactory<StateInfoDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString)
                );
            });

            var provider = services.BuildServiceProvider();

            var api = new GetMatchApi(
                provider.GetService<IMatchRecordDao>(),
                provider.GetService<IMatchResEventDao>(),
                provider.GetService<IMatchResAggregator>(),
                provider.GetService<IStateInfoDao>(),
                provider.GetService<IMemoryCache>()
            );

            return api;
        }

        static Mock<HttpRequest> MockGetRequest(string matchId = "foo", string requestLocation = "IA")
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
                { "X-Request-Location", requestLocation}
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
            var matchCreateDate = DateTime.UtcNow;
            // insert into database
            var match = new MatchRecordDbo()
            {
                CreatedAt = matchCreateDate,
                Data = "{\"State\": \"bb\", \"CaseId\": \"GHI\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"JKL\", \"ParticipantClosingDate\": \"2021-02-28\", \"VulnerableIndividual\": true, \"RecentBenefitIssuanceDates\": [{\"start\": \"2021-03-01\", \"end\":\"2021-03-31\"}]}",
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ia",
                Input = "{\"CaseId\": \"ABC\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"DEF\"}",
                MatchId = matchId,
                States = new string[] { "ia", "bb" }
            };
            Insert(match);

            // Act
            var response = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as JsonResult;
            var createdDate = (response.Value as MatchResApiResponse).Data.CreatedAt;
            string resString = JsonConvert.SerializeObject(response.Value);

            // Assert
            Assert.Equal(200, response.StatusCode);
            // Assert Participant Data
            var expected = "{\"data\":{\"dispositions\":[{\"initial_action_taken\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition_date\":null,\"state\":\"ia\"},{\"initial_action_taken\":null,\"invalid_match_reason\":null,\"other_reasoning_for_invalid_match\":null,\"final_disposition_date\":null,\"state\":\"bb\"}],\"initiator\":\"ia\",\"match_id\":\"ABC\",\"created_at\":" + JsonConvert.SerializeObject(createdDate) + ",\"participants\":[{\"case_id\":\"GHI\",\"participant_closing_date\":\"2021-02-28\",\"participant_id\":\"JKL\",\"recent_benefit_issuance_dates\":[{\"start\":\"2021-03-01\",\"end\":\"2021-03-31\"}],\"state\":\"bb\"},{\"case_id\":\"ABC\",\"participant_closing_date\":null,\"participant_id\":\"DEF\",\"recent_benefit_issuance_dates\":[],\"state\":\"ia\"}],\"states\":[\"ia\",\"bb\"],\"status\":\"open\"}}";
            Assert.Equal(expected, resString);
            // Assert the created date that is returned is nearly identical to the actual current time
            Assert.True((createdDate - matchCreateDate).Value.TotalMinutes < 1);
        }

        [Fact]
        public async void GetMatch_ReturnsNotFoundWhenNotAuthorized()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();

            var matchId = "ABC";
            var api = Construct();
            var mockRequest = MockGetRequest(matchId);
            var mockLogger = Mock.Of<ILogger>();
            var matchCreateDate = DateTime.UtcNow;
            // insert into database
            var match = new MatchRecordDbo()
            {
                CreatedAt = matchCreateDate,
                Data = "{\"State\": \"bb\", \"CaseId\": \"GHI\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"JKL\", \"ParticipantClosingDate\": \"2021-02-28\", \"VulnerableIndividual\": true, \"RecentBenefitIssuanceDates\": [{\"start\": \"2021-03-01\", \"end\":\"2021-03-31\"}]}",
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "eb",
                Input = "{\"CaseId\": \"ABC\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"DEF\"}",
                MatchId = matchId,
                States = new string[] { "eb", "bb" }
            };
            Insert(match);

            // Act
            // Act
            var response = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as NotFoundObjectResult;

            // Assert
            Assert.Equal(404, response.StatusCode);
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
            var match = new MatchRecordDbo()
            {
                MatchId = matchId,
                Initiator = "ia",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ia", "bb" },
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
            var resBody = response.Value as MatchResApiResponse;
            Assert.Null(resBody.Data.Dispositions[0].InvalidMatch);

            // Act again
            // creating an "invalid match" match event results in newly pulled Match request having invalid_match = true
            var mre = new MatchResEventDbo()
            {
                MatchId = matchId,
                ActorState = "ia",
                Actor = "user",
                Delta = "{ \"invalid_match\": true }"
            };
            InsertMatchResEvent(mre);
            var nextResponse = await api.GetMatch(mockRequest.Object, matchId, mockLogger) as JsonResult;

            // Assert next request
            var nextResBody = nextResponse.Value as MatchResApiResponse;
            Assert.Equal(200, nextResponse.StatusCode);
            // now this disposition's invalid flag should be true
            Assert.True(nextResBody.Data.Dispositions[0].InvalidMatch);
        }
    }
}
