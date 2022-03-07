using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Exceptions;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Match.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class MatchResEventDaoTests : DbFixture
    {
        private IDbConnectionFactory<CollaborationDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<CollaborationDb>>();
            factory
                .Setup(m => m.Build(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var conn = Factory.CreateConnection();
                    conn.ConnectionString = ConnectionString;
                    conn.Open();
                    return conn;
                });

            return factory.Object;
        }

        [Fact]
        public async Task AddEvent_FindsCorrectRecord()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearMatchResEvents();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchRecordDao>>();
                var dao = new MatchResEventDao(DbConnFactory(), logger);
                var matchRecordDao = new MatchRecordDao(DbConnFactory(), matchLogger);
                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                var match = new MatchRecordDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                var record = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@example.com",
                    ActorState = "ea",
                    Delta = "{}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                int id = await dao.AddEvent(record);
                record.Id = id;

                // Assert
                Assert.True(HasMatchResEvent(record));
            }
        }

        [Fact]
        public async void GetEvents_FindsAllRecordsInAscOrder()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchRecordDao>>();

                var dao = new MatchResEventDao(DbConnFactory(), logger);
                var matchRecordDao = new MatchRecordDao(DbConnFactory(), matchLogger);

                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                // parent match
                var match = new MatchRecordDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                // related match events
                var record1 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@example.com",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                };
                var record2 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@example.com",
                    ActorState = "ea",
                    Delta = "{\"status\": \"closed\"}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                await dao.AddEvent(record1);
                await dao.AddEvent(record2);

                var result = await dao.GetEvents(matchId);
                result = result.ToList();

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Equal(result.ElementAt(0).Delta, record1.Delta);
                Assert.Equal(result.ElementAt(1).Delta, record2.Delta);
                Assert.True(result.ElementAt(0).InsertedAt < result.ElementAt(1).InsertedAt); // asc order
            }
        }

        [Fact]
        public async void GetEvents_FindsAllRecordsInDescOrder()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchRecordDao>>();

                var dao = new MatchResEventDao(DbConnFactory(), logger);
                var matchRecordDao = new MatchRecordDao(DbConnFactory(), matchLogger);

                var idService = new MatchIdService();
                var matchId = idService.GenerateId();
                // parent match
                var match = new MatchRecordDbo
                {
                    MatchId = matchId,
                    Hash = "foo",
                    HashType = "ldshash",
                    Initiator = "ea",
                    States = new string[] { "ea", "eb" },
                    Data = "{}"
                };
                // related match events
                var record1 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@example.com",
                    ActorState = "ea",
                    Delta = "{\"status\": \"open\"}"
                };
                var record2 = new MatchResEventDbo
                {
                    MatchId = matchId,
                    Actor = "noreply@example.com",
                    ActorState = "ea",
                    Delta = "{\"status\": \"closed\"}"
                };

                // Act
                await matchRecordDao.AddRecord(match);
                await dao.AddEvent(record1);
                await dao.AddEvent(record2);

                var result = await dao.GetEvents(matchId, false);
                result = result.ToList();

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Equal(result.ElementAt(0).Delta, record2.Delta);
                Assert.Equal(result.ElementAt(1).Delta, record1.Delta);
                Assert.True(result.ElementAt(1).InsertedAt < result.ElementAt(0).InsertedAt); // desc order
            }
        }

        [Fact]
        public async void GetEvents_ReturnsEmptyAryWhenNotFound()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var logger = Mock.Of<ILogger<MatchResEventDao>>();
                var matchLogger = Mock.Of<ILogger<MatchRecordDao>>();

                var dao = new MatchResEventDao(DbConnFactory(), logger);
                var matchRecordDao = new MatchRecordDao(DbConnFactory(), matchLogger);

                // Act
                var result = await dao.GetEvents("foo", false);
                result = result.ToList();

                // Assert
                Assert.Empty(result);
            }
        }
    }
}
