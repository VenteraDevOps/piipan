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
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Builders;
using Piipan.Notifications.Core.Services;
using Piipan.Notifications.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class AddEventApiTests
    {
        private static Mock<HttpRequest> MockRequest(string jsonBody = "{}")
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

        private NotificationRecord NotificationRecord = new NotificationRecord()
        {
            MatchResEvent = new DispositionModel()
            {
                MatchId = "foo",
                InitState = "ea",
                MatchingState = "eb",
                InitStateVulnerableIndividual = true
            },
            EmailToRecord = new EmailToModel()
            {
                EmailTo = "Ea@Nac.gov"
            },
            EmailToRecordMS = new EmailToModel()
            {
                EmailTo = "Eb@Nac.gov"
            }
        };

        private Mock<IParticipantPublishMatchMetric> ParticipantPublishMatchMetricMock()
        {
            var mock = new Mock<IParticipantPublishMatchMetric>();
            mock.Setup(m => m.PublishMatchMetric(It.IsAny<ParticipantMatchMetrics>()))
                  .Returns(Task.CompletedTask);

            return mock;
        }

        private Mock<INotificationRecordBuilder> BuilderNotificationMock(NotificationRecord record)
        {
            var recordBuilder = new Mock<INotificationRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatchModel(It.IsAny<MatchModel>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetEmailToModel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
               .Setup(r => r.SetDispositionModel(It.IsAny<DispositionModel>()))
               .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(record);

            return recordBuilder;
        }

        private Mock<INotificationService> NotificationServiceMock()
        {
            var notificationRecord = new NotificationRecord()
            {
                MatchResEvent = new DispositionModel
                {
                    MatchId = It.IsAny<string>(),
                    InitState = It.IsAny<string>(),
                    MatchingState = It.IsAny<string>()
                },
                EmailToRecord = new EmailToModel()
                {
                    EmailTo = It.IsAny<string>()
                }
            };
            var mock = new Mock<INotificationService>();
            mock.Setup(m => m.PublishNotificationOnMatchResEventsUpdate(
                notificationRecord)).Returns(Task.FromResult(true));
            return mock;
        }

        private Mock<IStateInfoDao> StateInfoDaoMock()
        {
            var stateInfoDao = new Mock<IStateInfoDao>();
            stateInfoDao
                .Setup(r => r.GetStates())
                    .ReturnsAsync(new List<StateInfoDbo>()
                    {
                    new StateInfoDbo() { Id = "1", State = "Echo Alpha", StateAbbreviation = "ea" , Email = "Ea@Nac.gov" },
                    new StateInfoDbo() { Id = "2", State = "Echo Bravo", StateAbbreviation = "eb" , Email = "Eb@Nac.gov" },
                    });
            return stateInfoDao;
        }

        private DispositionModel BeforeDispositionUpdate = new DispositionModel()
        {
            MatchId = "foo",
            InitState = "Echo Alpha",
            MatchingState = "Echo Beta",
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open",
            InitStateInvalidMatch = false,
            InitStateInvalidMatchReason = "No Reason",
            InitStateInitialActionAt = new DateTime(2022, 9, 1),
            InitStateInitialActionTaken = "No Action Taken",
            InitStateFinalDisposition = null,
            InitStateFinalDispositionDate = null,
            InitStateVulnerableIndividual = false,
            MatchingStateInvalidMatch = false,
            MatchingStateInvalidMatchReason = "No Reason",
            MatchingStateInitialActionAt = new DateTime(2022, 9, 1),
            MatchingStateInitialActionTaken = "No Action Taken",
            MatchingStateFinalDisposition = null,
            MatchingStateFinalDispositionDate = null,
            MatchingStateVulnerableIndividual = false
        };

        private DispositionModel AfterDispositionUpdate = new DispositionModel()
        {
            MatchId = "foo",
            InitState = "Echo Alpha",
            MatchingState = "Echo Beta",
            CreatedAt = new DateTime(2022, 9, 1),
            Status = "Open",
            InitStateInvalidMatch = false,
            InitStateInvalidMatchReason = "No Reason",
            InitStateInitialActionAt = new DateTime(2022, 9, 1),
            InitStateInitialActionTaken = "No Action Taken",
            InitStateFinalDisposition = null,
            InitStateFinalDispositionDate = null,
            InitStateVulnerableIndividual = false,
            MatchingStateInvalidMatch = false,
            MatchingStateInvalidMatchReason = "No Reason",
            MatchingStateInitialActionAt = new DateTime(2022, 9, 1),
            MatchingStateInitialActionTaken = "No Action Taken",
            MatchingStateFinalDisposition = null,
            MatchingStateFinalDispositionDate = null,
            MatchingStateVulnerableIndividual = false
        };

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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            UnauthorizedResult response = (UnauthorizedResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(401, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo")), Times.Never());
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": true } }"); // same action as delta
            var logger = new Mock<ILogger>();

            // Act
            UnprocessableEntityObjectResult response = (UnprocessableEntityObjectResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            Assert.Equal(422, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());
            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo")), Times.Never());
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"invalid_match\": \"foo\" } }");
            var logger = new Mock<ILogger>();

            // Act
            BadRequestObjectResult response = (BadRequestObjectResult)(await api.AddEvent(mockRequest.Object, "bar", logger.Object));

            // Assert
            Assert.Equal(400, response.StatusCode);
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == It.IsAny<string>())), Times.Never());

            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo")), Times.Never());
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
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
                .SetupSequence(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = false } }
                })
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = true } }
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest("{ \"data\": { \"vulnerable_individual\": true } }");
            var logger = new Mock<ILogger>();

            // Act
            OkResult response = (OkResult)(await api.AddEvent(mockRequest.Object, "foo", logger.Object));

            // Assert
            matchResEventDao.Verify(mock => mock.AddEvent(It.IsAny<MatchResEventDbo>()), Times.Once());
            matchResEventDao.Verify(mock => mock.AddEvent(It.Is<MatchResEventDbo>(m => m.ActorState == "ea")), Times.Once());
            publishMatchMetrics.Verify(mock => mock.PublishMatchMetric(It.Is<ParticipantMatchMetrics>(m => m.MatchId == "foo")), Times.Once());
            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo" && p.EmailToRecord.EmailTo == "Ea@Nac.gov")), Times.Once);
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
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = false } }
                })
                .Returns(new MatchResRecord()
                {
                    Status = "closed",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = true } }
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);

            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
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

            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo" && p.EmailToRecord.EmailTo == "Ea@Nac.gov")), Times.Once);

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
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = false } }
                })
                .Returns(new MatchResRecord()
                {
                    Status = "closed",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = true } }
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
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
            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo" && p.EmailToRecord.EmailTo == "Ea@Nac.gov")), Times.Once);
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
                .SetupSequence(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = false } }
                })
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = true } }
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
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
            notificationService.Verify(r => r.PublishNotificationOnMatchResEventsUpdate(It.Is<NotificationRecord>(p => p.MatchResEvent.MatchId == "foo" && p.EmailToRecord.EmailTo == "Ea@Nac.gov")), Times.Once);

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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
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

        [Fact]
        public async void ValidateDispositionUpdatesModeTest()
        {
            var beforeUpdate = new DispositionModel()
            {
                MatchId = "foo",
                InitState = "Echo Alpha",
                MatchingState = "Echo Beta",
                CreatedAt = new DateTime(2022, 9, 1),
                Status = "Open",
                InitStateInvalidMatch = false,
                InitStateInvalidMatchReason = "No Reason",
                InitStateInitialActionAt = new DateTime(2022, 9, 1),
                InitStateInitialActionTaken = "No Action Taken",
                InitStateFinalDisposition = null,
                InitStateFinalDispositionDate = null,
                InitStateVulnerableIndividual = false,
                MatchingStateInvalidMatch = false,
                MatchingStateInvalidMatchReason = "No Reason",
                MatchingStateInitialActionAt = new DateTime(2022, 9, 1),
                MatchingStateInitialActionTaken = "No Action Taken",
                MatchingStateFinalDisposition = null,
                MatchingStateFinalDispositionDate = null,
                MatchingStateVulnerableIndividual = false
            };

            var afterUpdate = new DispositionModel()
            {
                MatchId = "foo",
                InitState = "Echo Alpha",
                MatchingState = "Echo Beta",
                CreatedAt = new DateTime(2022, 9, 1),
                Status = "Open",
                InitStateInvalidMatch = true,
                InitStateInvalidMatchReason = "No Reason",
                InitStateInitialActionAt = new DateTime(2022, 9, 1),
                InitStateInitialActionTaken = "No Action Taken",
                InitStateFinalDisposition = null,
                InitStateFinalDispositionDate = null,
                InitStateVulnerableIndividual = true,
                MatchingStateInvalidMatch = false,
                MatchingStateInvalidMatchReason = "No Reason",
                MatchingStateInitialActionAt = new DateTime(2022, 9, 1),
                MatchingStateInitialActionTaken = "No Action Taken",
                MatchingStateFinalDisposition = "disposition",
                MatchingStateFinalDispositionDate = null,
                MatchingStateVulnerableIndividual = false
            };
            // Act
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
                .SetupSequence(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = false } }
                })
                .Returns(new MatchResRecord()
                {
                    Status = "open",
                    States = new string[] { "ea", "eb" },
                    MatchId = "foo",
                    Dispositions = new Disposition[] { new Disposition() { VulnerableIndividual = true } }
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                  matchRecordDao.Object,
                  matchResEventDao.Object,
                  matchResAggregator.Object,
                  requestParser,
                  publishMatchMetrics.Object,
                  stateRecordDao.Object,
                  notificationService.Object,
                  recordNotificationBuilder.Object
            );

            DispositionUpdatesModel dispositionUpdates = await api.GetDispositionUpdates(beforeUpdate, afterUpdate);

            Assert.True(dispositionUpdates.InitStateVulnerableIndividual_Changed);
            Assert.True(dispositionUpdates.InitStateInvalidMatch_Changed);
            Assert.False(dispositionUpdates.MatchingStateVulnerableIndividual_Changed);
            Assert.False(dispositionUpdates.MatchingStateInvalidMatch_Changed);
            Assert.False(dispositionUpdates.InitStateFinalDisposition_Changed);
            Assert.True(dispositionUpdates.MatchingStateFinalDisposition_Changed);
        }

        [Fact]
        public async void ValidateDispositionUpdatesModel_InvalidMatch_InitiatingState_Test()
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            AfterDispositionUpdate.InitStateInvalidMatch = true;
            DispositionUpdatesModel dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            var ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.True(ret);
            Assert.True(dispositionUpdates.InitStateInvalidMatch_Changed);

            // Act
            AfterDispositionUpdate.InitStateInvalidMatch = false;
            dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.False(ret);
            Assert.False(dispositionUpdates.InitStateInvalidMatch_Changed);
        }

        [Fact]
        public async void ValidateDispositionUpdatesModel_InvalidMatch_MatchingState_Test()
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            AfterDispositionUpdate.MatchingStateInvalidMatch = true;
            DispositionUpdatesModel dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            var ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.True(ret);
            Assert.True(dispositionUpdates.MatchingStateInvalidMatch_Changed);

            // Act
            AfterDispositionUpdate.MatchingStateInvalidMatch = false;
            dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.False(ret);
            Assert.False(dispositionUpdates.MatchingStateInvalidMatch_Changed);
        }

        [Fact]
        public async void ValidateDispositionUpdatesModel_VulnerableIndividual_InitiatingState_Test()
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            AfterDispositionUpdate.InitStateVulnerableIndividual = true;
            DispositionUpdatesModel dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            var ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.True(ret);
            Assert.True(dispositionUpdates.InitStateVulnerableIndividual_Changed);

            // Act
            AfterDispositionUpdate.InitStateVulnerableIndividual = false;
            dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.False(ret);
            Assert.False(dispositionUpdates.InitStateVulnerableIndividual_Changed);
        }

        [Fact]
        public async void ValidateDispositionUpdatesModel_VulnerableIndividual_MatchingState_Test()
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            AfterDispositionUpdate.MatchingStateVulnerableIndividual = true;
            DispositionUpdatesModel dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            var ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.True(ret);
            Assert.True(dispositionUpdates.MatchingStateVulnerableIndividual_Changed);

            // Act
            AfterDispositionUpdate.MatchingStateVulnerableIndividual = false;
            dispositionUpdates = await api.GetDispositionUpdates(BeforeDispositionUpdate, AfterDispositionUpdate);
            ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdates);

            // Assert
            Assert.False(ret);
            Assert.False(dispositionUpdates.MatchingStateVulnerableIndividual_Changed);
        }

        [Fact]
        public async void GetDispositionUpdates_Error_Test()
        {
            // Arrange
            var dispose = new EmailModel();
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            // Act
            AfterDispositionUpdate.MatchingStateVulnerableIndividual = true;

            // Sending two different objects to compare,  It will fail
            await Assert.ThrowsAsync<System.Reflection.TargetException>(() => api.GetDispositionUpdates(BeforeDispositionUpdate, dispose));
        }

        [Fact]
        public async void Check_Notification_Publish_Test()
        {
            // Arrange
            var dispose = new EmailModel();
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

            var recordNotificationBuilder = BuilderNotificationMock(NotificationRecord);
            var api = new AddEventApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object,
                requestParser.Object,
                publishMatchMetrics.Object,
                stateRecordDao.Object,
                notificationService.Object,
                recordNotificationBuilder.Object
            );
            var mockRequest = MockRequest();
            var logger = new Mock<ILogger>();

            DispositionUpdatesModel dispositionUpdatesModel = new DispositionUpdatesModel()
            {
                MatchId_Changed = true,
                CreatedAt_Changed = false,
                InitState_Changed = false,
                InitStateInvalidMatch_Changed = true,
                InitStateInvalidMatchReason_Changed = false,
                InitStateInitialActionAt_Changed = true,
                InitStateInitialActionTaken_Changed = false,
                InitStateFinalDisposition_Changed = false,
                InitStateFinalDispositionDate_Changed = false,
                InitStateVulnerableIndividual_Changed = false,
                MatchingState_Changed = true,
                MatchingStateInvalidMatch_Changed = true,
                MatchingStateInvalidMatchReason_Changed = false,
                MatchingStateInitialActionAt_Changed = false,
                MatchingStateInitialActionTaken_Changed = false,
                MatchingStateFinalDisposition_Changed = false,
                MatchingStateFinalDispositionDate_Changed = false,
                MatchingStateVulnerableIndividual_Changed = true,
                Status_Changed = true,
            };

            // Act
            var ret = await api.IsValidForNotification(AfterDispositionUpdate, dispositionUpdatesModel);

            // Sending two different objects to compare,  It will fail

            Assert.True(ret);
        }
    }
}