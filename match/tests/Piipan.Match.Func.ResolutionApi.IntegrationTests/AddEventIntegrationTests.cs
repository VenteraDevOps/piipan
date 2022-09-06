using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Notification.Core.Extensions;
using Piipan.Notifications.Core.Builders;
using Piipan.Notifications.Core.Services;
using Piipan.Shared.Database;
using Piipan.States.Core.DataAccessObjects;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.IntegrationTests
{
    [Collection("MatchResolutionApiTests")]
    public class AddEventApiIntegrationTests : DbFixture
    {
        static AddEventApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");
            Environment.SetEnvironmentVariable("EventGridMetricMatchEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridMetricMatchKeyString", "example");
            Environment.SetEnvironmentVariable("EventGridNotificationEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridNotificationKeyString", "example");
            Environment.SetEnvironmentVariable("QueryToolUrl", "http://someendpoint.gov");
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IMatchRecordDao, MatchRecordDao>();
            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchResAggregator, MatchResAggregator>();
            services.AddTransient<IValidator<AddEventRequest>, AddEventRequestValidator>();
            services.AddTransient<IStreamParser<AddEventRequest>, AddEventRequestParser>();
            services.AddTransient<IParticipantPublishMatchMetric, ParticipantPublishMatchMetric>();
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
            services.AddTransient<IStateInfoDao, StateInfoDao>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<INotificationPublish, NotificationPublish>();
            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);

            services.AddMvc()
                   .AddApplicationPart(typeof(Piipan.Notification.Common.ViewRenderService).GetTypeInfo().Assembly)
                   .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                   .AddDataAnnotationsLocalization();

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Templates/{0}" + RazorViewEngine.ViewExtension);
            });

            services.RegisterNotificationServices();
            var provider = services.BuildServiceProvider();

            var api = new AddEventApi(
                provider.GetService<IMatchRecordDao>(),
                provider.GetService<IMatchResEventDao>(),
                provider.GetService<IMatchResAggregator>(),
                provider.GetService<IStreamParser<AddEventRequest>>(),
                provider.GetService<IParticipantPublishMatchMetric>(),
                provider.GetService<IStateInfoDao>(),
                provider.GetService<INotificationService>(),
                provider.GetService<INotificationRecordBuilder>()

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
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchRecordDbo()
            {
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
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchRecordDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert an event into the database
            var matchResEvent = new MatchResEventDbo()
            {
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
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();

            // insert a match into the database
            var matchId = "ABCDEFG";
            var match = new MatchRecordDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                CreatedAt = DateTime.UtcNow,
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
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
            Assert.Equal("{\"state\": null, \"invalid_match\": true, \"initial_action_taken\": null, \"invalid_match_reason\": null, \"final_disposition_date\": null, \"other_reasoning_for_invalid_match\": null}", lastEvent.Delta);
            Assert.Equal(200, response.StatusCode);

        }

        [Fact]
        public async void AddEvent_SuccessInsertsClosedEventIfClosed()
        {
            // Arrange
            // clear databases Changing the order for any referential integrity issues.
            ClearMatchResEvents();
            ClearMatchRecords();
            // insert a match into db
            var matchId = "ABCDEFG";
            var match = new MatchRecordDbo()
            {
                MatchId = matchId,
                Initiator = "ea",
                States = new string[] { "ea", "eb" },
                Hash = "foo",
                HashType = "ldshash",
                Data = "{}",
                Input = "{}"
            };
            Insert(match);
            // insert final disposition event into db
            var matchResEvent = new MatchResEventDbo()
            {
                MatchId = matchId,
                Actor = "user",
                ActorState = "eb",
                Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }"
            };
            InsertMatchResEvent(matchResEvent);

            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"" + System.DateTime.UtcNow.AddDays(2).ToString("s") + "\" } }");
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