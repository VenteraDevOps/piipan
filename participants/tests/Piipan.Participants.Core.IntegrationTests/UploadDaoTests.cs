using System;
using Dapper;
using Moq;
using Npgsql;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
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

                string uploadId = "test_etag";
                InsertUpload(uploadId);

                var expected = GetLastUploadId();

                var dao = new UploadDao(DbConnFactory());

                // Act
                var result = await dao.GetLatestUpload();

                // Assert
                Assert.Equal(expected, result.Id);
            }
        }

        [Fact]
        public async void GetUploadById()
        {
            string uploadId1 = "test_etag";
            string uploadId2 = "test_etag_2";

            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();

                InsertUpload(uploadId1);
                InsertUpload(uploadId2);

                var dao = new UploadDao(DbConnFactory());

                // Act
                var upload1 = await dao.GetUploadById(uploadId1);
                var upload2 = await dao.GetUploadById(uploadId2);

                // Assert
                Assert.NotEqual(upload1, upload2);
                Assert.True(upload1.CreatedAt < upload2.CreatedAt);
                Assert.Equal(uploadId1, upload1.UploadIdentifier);
                Assert.Equal(uploadId2, upload2.UploadIdentifier);
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
            var result = await dao.AddUpload("test-etag");

            // Assert
            Assert.Equal(GetLastUploadId(), result.Id);
        }

        [Fact]
        public async void UpdateUpload()
        {
            string uploadId1 = "test_etag";

            // Arrange
            var dao = new UploadDao(DbConnFactory());

            InsertUpload(uploadId1);

            var upload = await dao.GetUploadById(uploadId1);
            Assert.Equal(UploadStatuses.COMPLETE.ToString(), upload.Status);


            // Act
            upload.Status = UploadStatuses.FAILED.ToString();
            await dao.UpdateUpload(upload);

            // Assert
            var updatedUpload = await dao.GetUploadById(uploadId1);
            Assert.Equal(UploadStatuses.FAILED.ToString(), upload.Status);
        }
    }
}
