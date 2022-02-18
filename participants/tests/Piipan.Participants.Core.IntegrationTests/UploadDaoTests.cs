using System;
using Dapper;
using Moq;
using Npgsql;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class UploadDaoTests : DbFixture
    {
        private IDbConnectionFactory<ParticipantsDb> DbConnFactory()
        {
            var factory = new Mock<IDbConnectionFactory<ParticipantsDb>>();
            factory
                .Setup(m => m.Build(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var conn = Factory.CreateConnection();
                    conn.ConnectionString = ConnectionString;
                    return conn;
                });

            return factory.Object;
        }

        [Fact]
        public async void GetLatestUpload()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                InsertUpload();

                var expected = GetLastUploadId();

                var dao = new UploadDao(DbConnFactory());

                // Act
                var result = await dao.GetLatestUpload();

                // Assert
                Assert.Equal(expected, result.Id);
            }
        }

        [Fact]
        public async void GetLatestUpload_ThrowsIfNone()
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                ClearUploads();

                var dao = new UploadDao(DbConnFactory());

                // Act / Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => dao.GetLatestUpload());
            }
        }

        [Fact]
        public async void AddUpload()
        {
            // Arrange
            var dao = new UploadDao(DbConnFactory());

            // Act
            var result = await dao.AddUpload();

            // Assert
            Assert.Equal(GetLastUploadId(), result.Id);
        }
    }
}
