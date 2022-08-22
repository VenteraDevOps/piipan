using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Metrics.Api;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class AddEventApiTests
    {

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
        private Mock<IParticipantPublishMatchMetric> ParticipantPublishMatchMetricMock()
        {
            var mock = new Mock<IParticipantPublishMatchMetric>();
            mock.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                  .Returns(Task.CompletedTask);

            return mock;
        }
        private Mock<INotificationService> NotificationServiceMock()
        {
            var emailTemplateInput = new EmailTemplateInput()
            {
                Topic = It.IsAny<string>(),
                TemplateData = new
                {
                    MatchId = It.IsAny<string>(),
                    InitState = It.IsAny<string>(),
                    MatchingState = It.IsAny<string>(),
                    MatchingUrl = It.IsAny<string>(),

                },
                EmailTo = It.IsAny<string>()
            };
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.CreateMessageFromTemplate(
                emailTemplateInput)).Returns(Task.FromResult(true));
            return mock;
        }
        private Mock<IStateInfoDao> StateInfoDaoMock()
        {
            var matchRecordDao = new Mock<IStateInfoDao>();
            matchRecordDao
                .Setup(r => r.GetStates())
                    .ReturnsAsync(new List<StateInfoDbo>()
                    {
                    new StateInfoDbo() { Id = "1", State = "Echo Alpha", StateAbbreviation = "ea" },
                    new StateInfoDbo() { Id = "2", State = "Echo Bravo", StateAbbreviation = "eb" },
                    });
            return matchRecordDao;
        }
        [Fact]
        public async void AddEvent_LogsRequest()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object
            );
            var mockRequest = MockRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" },
                    { "From", "foobar"},
                    { "X-Initiating-State", "ea"}
                }));
            var logger = new Mock<ILogger>();

            // Act
            await api.AddEvent(mockRequest.Object, "foo", logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using APIM subscription sub-name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async void AddEvent_Returns404IfNotFound()
        {

            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("not found error"));
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            NotFoundObjectResult response = (NotFoundObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsUnauthorizedIfMatchClosed()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo() { States = new string[] { "ea", "bc" } });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "closed"
                });
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfUnrelatedStateActor()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo() { States = new string[] { "bc", "de" } });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new Mock<IStreamParser<AddEventRequest>>();
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfDuplicateAction()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo() { States = new string[] { "ea", "eb" } });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true }", // same action as request body
                    ActorState = "ea"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }"); // same action as delta
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfRequestBodyInvalid()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": \"foo\" } }");
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "bar", logger.Object));

            // Assert
            Assert.Equal(400, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Never());
        }

        [Fact]
        public async void AddEvent_ReturnsErrorIfRequestNotParsed()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("foobar");
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "bar", logger.Object));

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_InsertsEventOnSuccess()
        {
            // Arrange
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo()
                {
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    MatchId = "foo",
                    States = new string[] { "ea", "eb" }
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                .Returns(Task.CompletedTask);

            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"vulnerable_individual\": true } }");
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(It.IsAny<MatchResEventDbo>()), Times.Once());
            matchResEventDao.Verify(mock => mock.AddEvent(It.Is<MatchResEventDbo>(m => m.ActorState == "ea")), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo")), Times.Once());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Exactly(2));
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo()
                {
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    CreatedAt = DateTime.Parse("2022-07-01")
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            var eventsAfterUpdate = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\"}",
                    ActorState = "eb"
                },
                 new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "ea"
                }
            };
            matchResEventDao
                .SetupSequence(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events)
                .ReturnsAsync(eventsAfterUpdate);

            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .SetupSequence(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo"
                })
                .Returns(new MatchResRecord()
                {
                    Status = "closed",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                  .Returns(Task.CompletedTask);

            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"2022-07-20T00:00:01\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo" && m.Status == "closed")), Times.Once());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Exactly(2));
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess_WithOneInvalid()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-07-01"),
                    MatchId = "foo"
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .SetupSequence(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open"
                })
                .Returns(new MatchResRecord()
                {
                    Status = "closed",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                 .Returns(Task.CompletedTask);
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            Assert.Equal(200, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo" && m.Status == "closed")), Times.Once());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Exactly(2));
        }

        [Fact]
        public async void AddEvent_ChecksIfShouldBeClosedOnSuccess_WithBothInvalid()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-07-01")
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"invalid_match\": true }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            publishMatchMetrics.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                .Returns(Task.CompletedTask);

            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo")), Times.Once());
            var topic = "UPDATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Exactly(2));
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void AddEvent_CheckIfFinalDispositionDateError()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo()
                {
                    States = new string[] { "ea", "eb" },
                    CreatedAt = DateTime.Parse("2022-08-01") // Created At date > Final Disposition Date
                });
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var events = new List<IMatchResEvent>() {
                new MatchResEventDbo() {
                    Delta = "{ \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\",  \"final_disposition\": \"foo\", \"final_disposition_date\": \"2022-07-20T00:00:02\" }",
                    ActorState = "eb"
                }
            };
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);
            matchResEventDao
             .Setup(r => r.AddEvent(
                 It.IsAny<MatchResEventDbo>()
             ))
             .ReturnsAsync(1);
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open"
                });
            var requestParser = new AddEventRequestParser(
                new AddEventRequestValidator(),
                Mock.Of<ILogger<AddEventRequestParser>>()
            );
            var publishMatchMetrics = new Mock<IParticipantPublishMatchMetric>();
            var stateRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"initial_action_taken\": \"Notice Sent\", \"initial_action_at\": \"2022-07-20T00:00:02\", \"final_disposition\": \"bar\", \"final_disposition_date\": \"2022-07-20T00:00:01\" } }"); // coming form state ea
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(
                It.Is<MatchResEventDbo>(m => m.Actor == "system" && m.Delta == api.ClosedDelta)
            ), Times.Never());
            Assert.Equal(400, response.StatusCode);
        }
    }
}