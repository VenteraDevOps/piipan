using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Models;
using Piipan.Participants.Core.Enums;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Metrics.Core.IntegrationTests
{


    public class MetricsCommonDaoTests : DbFixture
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

        private string RandomState()
        {
            return Guid.NewGuid().ToString().Substring(0, 2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploadCount_ReturnsExpected(long expectedCount)
        {
            // Arrange
            for (var i = 0; i < expectedCount; i++)
            {
                Insert(RandomState(), DateTime.Now, DateTime.Now, "status", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var count = await dao.GetUploadCount(new ParticipantUploadRequestFilter());

            // Assert
            Assert.Equal(expectedCount, count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploadCountByState_ReturnsExpected(long expectedCount)
        {
            // Arrange
            for (var i = 0; i < expectedCount; i++)
            {
                Insert("ea", DateTime.Now, DateTime.Now, "status", "upload_identifier");
                Insert("eb", DateTime.Now, DateTime.Now, "status", "upload_identifier");
                Insert("ec", DateTime.Now, DateTime.Now, "status", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaCount = await dao.GetUploadCount(new Api.ParticipantUploadRequestFilter { State = "ea" });
            var ebCount = await dao.GetUploadCount(new Api.ParticipantUploadRequestFilter { State = "eb" });
            var ecCount = await dao.GetUploadCount(new Api.ParticipantUploadRequestFilter { State = "ec" });

            // Assert
            Assert.Equal(expectedCount, eaCount);
            Assert.Equal(expectedCount, ebCount);
            Assert.Equal(expectedCount, ecCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploads_ReturnsCorrectCount(long count)
        {
            // Arrange
            for (var i = 0; i < count; i++)
            {
                Insert("ea", DateTime.Now, DateTime.Now, "status", "upload_identifier");
                Insert("eb", DateTime.Now, DateTime.Now, "status", "upload_identifier");
                Insert("ec", DateTime.Now, DateTime.Now, "status", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaUploads = await dao.GetUploads(new Api.ParticipantUploadRequestFilter { State = "ea", PerPage = 100 });
            var ebUploads = await dao.GetUploads(new Api.ParticipantUploadRequestFilter { State = "eb", PerPage = 100 });
            var ecUploads = await dao.GetUploads(new Api.ParticipantUploadRequestFilter { State = "ec", PerPage = 100 });

            // Assert
            Assert.Equal(count, eaUploads.Count());
            Assert.Equal(count, ebUploads.Count());
            Assert.Equal(count, ecUploads.Count());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public async Task GetUploads_LimitingWorks(int limit)
        {
            // Arrange
            for (var i = 0; i < 100; i++)
            {
                Insert("ea", DateTime.Now, DateTime.Now, "status", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var eaUploads = await dao.GetUploads(new Api.ParticipantUploadRequestFilter { State = "ea", PerPage = limit });

            // Assert
            Assert.Equal(limit, eaUploads.Count());
        }

        [Theory]
        [InlineData(1, 30)]
        [InlineData(3, 30)]
        [InlineData(4, 10)]
        [InlineData(5, 0)]
        public async Task GetUploads_OffsettingWorks(int pageNumber, int expectedReturned)
        {
            // Arrange
            for (var i = 0; i < 100; i++)
            {
                Insert("ea", DateTime.Now, DateTime.Now, "status", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());
            // Act
            var eaUploads = await dao.GetUploads(new Api.ParticipantUploadRequestFilter { State = "ea", PerPage = 30, Page = pageNumber });

            // Assert
            Assert.Equal(expectedReturned, eaUploads.Count());
        }

        [Fact]
        public async Task GetLatestUploadsByState_ReturnsExpected()
        {
            DateTime uploadTime = new DateTime(2021, 1, 1, 5, 0, 0);

            // Arrange
            DateTime latestEA = new DateTime(2021, 1, 1, 5, 10, 0);
            DateTime latestEB = new DateTime(2021, 2, 2, 5, 10, 0);
            DateTime latestEC = new DateTime(2021, 3, 3, 5, 10, 0);
            for (var i = 0; i < 100; i++)
            {
                latestEA = latestEA + TimeSpan.FromSeconds(i);
                latestEB = latestEB + TimeSpan.FromSeconds(i);
                latestEC = latestEC + TimeSpan.FromSeconds(i);
                Insert("ea", uploadTime, latestEA, UploadStatuses.COMPLETE.ToString(), $"upload_identifier_ea{i}");
                Insert("eb", uploadTime, latestEB, UploadStatuses.COMPLETE.ToString(), $"upload_identifier_eb{i}");
                Insert("ec", uploadTime, latestEC, UploadStatuses.COMPLETE.ToString(), $"upload_identifier_ec{i}");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var latest = await dao.GetLatestSuccessfulUploadsByState();

            // Assert
            Assert.Equal(3, latest.Count());
            Assert.Single(latest, u => u.State == "ea" && u.CompletedAt.Equals(latestEA));
            Assert.Single(latest, u => u.State == "eb" && u.CompletedAt.Equals(latestEB));
            Assert.Single(latest, u => u.State == "ec" && u.CompletedAt.Equals(latestEC));
        }

        [Fact]
        public async Task AddUpload_InsertsRecord()
        {
            // Arrange
            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());
            var uploadedAt = new DateTime(2021, 1, 1, 5, 10, 0);
            var filter = new ParticipantUploadRequestFilter { State = "ea" };

            var uploadCount = await dao.GetUploadCount(filter);
            Assert.Equal(0, uploadCount);

            // Act
            await dao.AddUpload(new ParticipantUploadDbo() { State = "ea", UploadedAt = uploadedAt, Status = UploadStatuses.UPLOADING.ToString() });
            var results = await dao.GetUploads(filter);

            // Assert
            uploadCount = await dao.GetUploadCount(filter);
            Assert.Equal(1, uploadCount);

            Assert.Equal(uploadedAt, results.ToList()[0].UploadedAt);
        }

        [Fact]
        public async Task UpdateUpload_UpdatesRecord()
        {
            // Arrange
            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());
            var uploadedAt = new DateTime(2021, 1, 1, 5, 10, 0);
            var completedAt = new DateTime(2021, 1, 1, 5, 15, 0);
            var upload_id = "abcd123";
            Insert("ea", uploadedAt, null, UploadStatuses.UPLOADING.ToString(), upload_id);
            var filter = new ParticipantUploadRequestFilter { State = "ea" };

            var results = await dao.GetUploads(filter);
            var uploadCount = await dao.GetUploadCount(filter);
            Assert.Equal(1, uploadCount);
            Assert.Equal(uploadedAt, results.ToList()[0].UploadedAt);
            Assert.Null(results.ToList()[0].CompletedAt);

            // Act
            await dao.UpdateUpload(new ParticipantUploadDbo() { State = "ea", UploadedAt = uploadedAt, CompletedAt = completedAt, Status = UploadStatuses.COMPLETE.ToString(), UploadIdentifier = upload_id });

            // Assert
            uploadCount = await dao.GetUploadCount(filter);
            Assert.Equal(1, uploadCount);
            results = await dao.GetUploads(filter);
            Assert.Equal(uploadedAt, results.ToList()[0].UploadedAt);
            Assert.Equal(completedAt, results.ToList()[0].CompletedAt);
        }
        [Fact]
        public async Task ParticipantSearch_InsertsRecord()
        {
            // Arrange
            var dao = new ParticipantSearchDao(DbConnFactory(), new NullLogger<ParticipantSearchDao>());
            // Act
            var numberOfRows = await dao.AddParticipantSearchRecord(new ParticipantSearchDbo()
            {
                State = "ea",
                SearchReason = "Application",
                SearchFrom = "SearchFrom",
                MatchCreation = "MatchCreation",
                MatchCount = 1,
                SearchedAt = DateTime.UtcNow
            });
            Assert.Equal(1, numberOfRows);
        }
        [Fact]
        public async Task ParticipantMatch_InsertsRecord()
        {
            // Arrange
            ClearParticipantMatchesMetrics();
            var dao = new ParticipantMatchDao(DbConnFactory(), new NullLogger<ParticipantMatchDao>());
            // Act
            var numberOfRows = await dao.AddParticipantMatchRecord(new ParticipantMatchDbo()
            {
                MatchId = "foo_1",
                InitState = "ea",
                MatchingState = "ec",
                Status = "open"
            });
            Assert.Equal(1, numberOfRows);
        }
        [Fact]
        public async Task ParticipantMatch_UpdateRecord()
        {
            // Arrange
            ClearParticipantMatchesMetrics();
            var dao = new ParticipantMatchDao(DbConnFactory(), new NullLogger<ParticipantMatchDao>());
            ParticipantMatchDbo participantMatchDbo = new ParticipantMatchDbo()
            {
                MatchId = "foo",
                InitState = "ea",
                MatchingState = "ec",
                Status = "open"
            };
            var numberOfRows = await dao.AddParticipantMatchRecord(participantMatchDbo);

            participantMatchDbo.MatchingStateFinalDisposition = "MSFinalDisposition";
            participantMatchDbo.InitStateFinalDisposition = "ISFinalDisposition";
            var numberOfRowsUPdated = await dao.UpdateParticipantMatchRecord(participantMatchDbo);

            var results = await dao.GetMatchMetrics(participantMatchDbo.MatchId);

            Assert.Equal(participantMatchDbo.MatchingStateFinalDisposition, results.ToList()[0].MatchingStateFinalDisposition);
            Assert.Equal(participantMatchDbo.InitStateFinalDisposition, results.ToList()[0].InitStateFinalDisposition);

            Assert.Equal(1, numberOfRowsUPdated);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(1, 2, 1, 2)]
        [InlineData(50, 100, 10, 10)]
        public async Task GetUploadStatistics_ReturnsExpected(int failed, int complete, int expectedFailedStates, int expectedCompleteStates)
        {
            // Arrange
            for (var i = 0; i < failed; i++)
            {
                Insert($"E{i % 10}", DateTime.Now, DateTime.Now, "failed", "upload_identifier");
            }
            for (var i = 0; i < complete; i++)
            {
                Insert($"E{i % 10}", DateTime.Now, DateTime.Now, "complete", "upload_identifier");
            }

            var dao = new ParticipantUploadDao(DbConnFactory(), new NullLogger<ParticipantUploadDao>());

            // Act
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            var statistics = await dao.GetUploadStatistics(new ParticipantUploadStatisticsRequest
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date,
                HoursOffset = (int)offset.TotalHours
            });

            // Assert
            Assert.Equal(expectedFailedStates, statistics.TotalFailure);
            Assert.Equal(expectedCompleteStates, statistics.TotalComplete);
        }
    }
}
