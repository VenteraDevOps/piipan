using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Metrics.Api;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Cryptography;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Services
{
    public class MatchEventServiceTests
    {
        private const string QueryToolUrl = "https://tts.test";
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public MatchEventServiceTests()
        {
            Environment.SetEnvironmentVariable("QueryToolUrl", QueryToolUrl);
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        private Mock<IActiveMatchRecordBuilder> BuilderMock(MatchRecordDbo record)
        {
            var recordBuilder = new Mock<IActiveMatchRecordBuilder>();
            recordBuilder
                .Setup(r => r.SetMatch(It.IsAny<RequestPerson>(), It.IsAny<IParticipant>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.SetStates(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(recordBuilder.Object);
            recordBuilder
                .Setup(r => r.GetRecord())
                .Returns(record);

            return recordBuilder;
        }

        private Mock<IMatchRecordApi> ApiMock(string matchId = "foo")
        {
            var api = new Mock<IMatchRecordApi>();
            api.Setup(r => r.AddRecord(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(matchId);

            return api;
        }

        private Mock<IMatchResEventDao> MatchResEventDaoMock(
            IEnumerable<IMatchResEvent> events
        )
        {
            var mock = new Mock<IMatchResEventDao>();
            mock.Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(events);

            return mock;
        }
        private Mock<IParticipantPublishSearchMetric> ParticipantPublishSearchMetricMock()
        {
            var mock = new Mock<IParticipantPublishSearchMetric>();
            mock.Setup(m => m.PublishSearchdMetric(It.IsAny<ParticipantSearchMetrics>()))
                  .Returns(Task.CompletedTask);

            return mock;
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

        private Mock<IMatchResAggregator> MatchResAggregatorMock(
                    MatchResRecord result
                )
        {
            var mock = new Mock<IMatchResAggregator>();
            mock.Setup(r => r.Build(
                It.IsAny<IMatchRecord>(),
                It.IsAny<IEnumerable<IMatchResEvent>>()
            ))
            .Returns(result);

            return mock;
        }

        [Fact]
        public async void Resolve_AddsSingleRecord()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = String.Empty,
                SearchReason = null,
                MatchCreation = "New Created Match",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Once);

            publishSearchMetrics.Verify(r => r.PublishSearchdMetric(
              It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                   r.Data.First().MatchCreation == search.MatchCreation &&
                                                   r.Data.First().SearchReason == search.SearchReason)),
                                                   Times.Once);

            publishMatchMetrics.Verify(r => r.PublishMatchMetric(
             It.Is<ParticipantMatchMetrics>(r => r.MatchId == "foo" &&
                                                  r.InitState == record.Initiator
                                                 )),
                                                  Times.Once);

            var topic = "CREATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Exactly(2));
        }

        [Fact]
        public async void Resolve_AddsManyRecords()
        {
            // Arrange
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() {
                    new ParticipantMatch { LdsHash = "foo", State= "eb" },
                    new ParticipantMatch { LdsHash = "foo", State = "ec" }
                }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord());
            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");

            // Assert
            recordApi.Verify(r => r.AddRecord(
                It.Is<IMatchRecord>(r =>
                    r.Hash == record.Hash &&
                    r.HashType == record.HashType &&
                    r.Initiator == record.Initiator &&
                    r.States.SequenceEqual(record.States))),
                Times.Exactly(result.Matches.Count()));
        }

        [Fact]
        public async void Resolve_InsertsMatchId()
        {
            // Arrange
            var mockMatchId = "BDC2345";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(mockMatchId);

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();


            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{mockMatchId}", firstMatch.MatchUrl);
            Assert.Equal(mockMatchId, resolvedResponse.Data.Results.First().Matches.First().MatchId);
        }

        [Fact]
        public async void Resolve_InsertsMostRecentMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var closedMatchId = "CDB5432";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = openMatchId,
                        CreatedAt = new DateTime(2020,01,02)
                    },
                    new MatchRecordDbo {
                        MatchId = closedMatchId,
                        CreatedAt = new DateTime(2020,01,01)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord());
            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{openMatchId}", firstMatch.MatchUrl);
            Assert.Equal(openMatchId, firstMatch.MatchId);
        }

        [Fact]
        public async void Resolve_InsertsOpenMatchId()
        {
            // Arrange
            var openMatchId = "BDC2345";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock();
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = openMatchId
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord());

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{openMatchId}", firstMatch.MatchUrl);
            Assert.Equal(openMatchId, firstMatch.MatchId);
        }

        [Fact]
        public async void Resolve_InsertsNewMatchIdIfMostRecentRecordIsClosed()
        {
            // Arrange
            var newId = "newId";
            var record = new MatchRecordDbo
            {
                Hash = "foo",
                HashType = "ldshash",
                Initiator = "ea",
                States = new string[] { "ea", "eb" }
            };
            var search = new ParticipantSearch
            {
                State = "ea",
                SearchFrom = String.Empty,
                SearchReason = null,
                MatchCreation = "Already Existing Match",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            };
            ParticipantSearchMetrics searchMetrics = new ParticipantSearchMetrics();
            searchMetrics.Data.Add(search);
            var recordBuilder = BuilderMock(record);
            var recordApi = ApiMock(newId);
            recordApi.Setup(r => r.GetRecords(It.IsAny<IMatchRecord>()))
                .ReturnsAsync(new List<MatchRecordDbo> {
                    new MatchRecordDbo {
                        MatchId = "closedId",
                        CreatedAt = new DateTime(2020,01,02)
                    }
                });

            var request = new OrchMatchRequest();
            var person = new RequestPerson { LdsHash = "foo" };
            request.Data.Add(person);

            var response = new OrchMatchResponse();
            var match = new ParticipantMatch { LdsHash = "foo", State = "eb" };
            var result = new OrchMatchResult
            {
                Index = 0,
                Matches = new List<ParticipantMatch>() { match }
            };
            response.Data.Results.Add(result);

            var mreDao = MatchResEventDaoMock(new List<IMatchResEvent>());
            var aggDao = MatchResAggregatorMock(new MatchResRecord()
            {
                Status = MatchRecordStatus.Closed
            });

            var publishSearchMetrics = ParticipantPublishSearchMetricMock();
            var publishMatchMetrics = ParticipantPublishMatchMetricMock();

            var matchRecordDao = StateInfoDaoMock();
            var notificationService = NotificationServiceMock();

            var service = new MatchEventService(
                recordBuilder.Object,
                recordApi.Object,
                mreDao.Object,
                aggDao.Object,
                cryptographyClient,
                publishSearchMetrics.Object,
                publishMatchMetrics.Object,
                matchRecordDao.Object,
                notificationService.Object

            );

            // Act
            var resolvedResponse = await service.ResolveMatches(request, response, "ea");
            var firstMatch = resolvedResponse.Data.Results.First().Matches.First();

            // Assert
            Assert.Equal($"{QueryToolUrl}/match/{newId}", firstMatch.MatchUrl);
            Assert.Equal(newId, firstMatch.MatchId);

            publishSearchMetrics.Verify(r => r.PublishSearchdMetric(
             It.Is<ParticipantSearchMetrics>(r => r.Data.First().MatchCount == search.MatchCount &&
                                                  r.Data.First().MatchCreation == search.MatchCreation &&
                                                  r.Data.First().SearchReason == search.SearchReason)),
             Times.Once);
            var topic = "CREATE_MATCH_RES";
            notificationService.Verify(r => r.CreateMessageFromTemplate(It.Is<EmailTemplateInput>(p => p.Topic == topic)), Times.Never);
        }
    }
}