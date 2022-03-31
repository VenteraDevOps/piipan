using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Validators;
using Piipan.Shared.Database;
using FluentValidation;
using Moq;
using Npgsql;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class AddEventApiIntegrationTests : DbFixture
    {
        static AddEventApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IMatchRecordDao, MatchRecordDao>();
            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchResAggregator, MatchResAggregator>();
            services.AddTransient<IValidator<AddEventRequest>, AddEventRequestValidator>();
            services.AddTransient<IStreamParser<AddEventRequest>, AddEventRequestParser>();

            services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new BasicPgConnectionFactory<CollaborationDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            var provider = services.BuildServiceProvider();

            var api = new AddEventApi(
                provider.GetService<IMatchRecordDao>(),
                provider.GetService<IMatchResEventDao>(),
                provider.GetService<IMatchResAggregator>(),
                provider.GetService<IStreamParser<AddEventRequest>>()
            );

            return api;
        }

        static Mock<HttpRequest> MockRequest(string jsonBody = "{}")
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
                { "X-Initiating-State", "ea"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfNotRelatedState()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();
            // insert a match into the database
            var matchId = "ABCDEF";
            var match = new MatchRecordDbo() {
                MatchId = matchId,
                Initiator = "ec",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ec", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}"
            };
            Insert(match);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            var response = await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object) as UnauthorizedResult;

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfClosed()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();
            // insert a match into the database
            var matchId = "ABCDEF";
            var match = new MatchRecordDbo() {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}"
            };
            Insert(match);
            // insert an event into the database
            var matchResEvent = new MatchResEventDbo() {
                MatchId = matchId,
                Actor = "system",
                Delta = "{ \"status\": \"closed\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_SuccessInsertsEvent()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();
            // insert a match into the database
            var matchId = "ABCDEF";
            var match = new MatchRecordDbo() {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}"
            };
            Insert(match);

            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));
            var events = await GetEvents(matchId);
            var lastEvent = events.Last();

            // Assert
            Assert.Single(events);
            Assert.Equal(matchId, lastEvent.MatchId);
            Assert.Equal("ea", lastEvent.ActorState);
            Assert.Equal("{\"invalid_match\": true}", lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_SuccessInsertsClosedEventIfClosed()
        {
            // Arrange
            // clear databases
            ClearMatchRecords();
            ClearMatchResEvents();
            // insert a match into db
            var matchId = "ABCDEF";
            var match = new MatchRecordDbo() {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}"
            };
            Insert(match);
            // insert final disposition event into db
            var matchResEvent = new MatchResEventDbo() {
                MatchId = matchId,
                Actor = "user",
                ActorState = "eb",
                Delta = "{ \"final_disposition\": \"foo\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest = MockRequest("{ \"data\": { \"final_disposition\": \"bar\" } }");
            var mockLogger = new Mock<ILogger>();
            var api = Construct();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, matchId, mockLogger.Object));
            var events = await GetEvents(matchId);
            var lastEvent = events.Last();

            // Assert
            Assert.Equal(3, events.Count());
            Assert.Equal(matchId, lastEvent.MatchId);
            Assert.Equal(api.SystemActor, lastEvent.Actor);
            Assert.Equal(api.ClosedDelta, lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
