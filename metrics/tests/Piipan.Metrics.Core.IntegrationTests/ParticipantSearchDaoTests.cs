using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Piipan.Shared.Database;
using Piipan.Metrics.Core.DataAccessObjects;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Piipan.Metrics.Core.Models;
using Piipan.Participants.Core.Enums;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.IntegrationTests
{
    
    public class ParticipantSearchDaoTests : DbFixture
    {
        private IDbConnectionFactory<MetricsDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<MetricsDb>>();
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
        public async Task ParticipantSearch_InsertsRecord()
        {
            // Arrange
            ClearParticipantSearch();
            var dao = new ParticipantSearchDao(DbConnFactory(), new NullLogger<ParticipantSearchDao>());
             // Act
             var numberOfRows =  await dao.AddParticipantSearchRecord(new ParticipantSearchDbo() {
                State = "ea",
                SearchReason = "Application",
                SearchFrom = "SearchFrom",
                MatchCreation = "MatchCreation",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            });
            Assert.Equal(1, numberOfRows);
        }
       
    }
}
