using System;
using System.Collections.Generic;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;
using Moq;
using Xunit;
using Piipan.Metrics.Core.Builders;
using System.Threading.Tasks;

namespace Piipan.Metrics.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    {
        [Fact]
        public async Task GetUploadCount()
        {
            // Arrange
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploadCount(It.IsAny<string>()))
                .Returns(99);
            var metaBuilder = Mock.Of<IMetaBuilder>();
            
            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);
            
            // Act
            var count = await service.GetUploadCount("somestate");

            // Assert
            Assert.Equal(99, count);
        }

        [Fact]
        public async Task GetUploads()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var response = await service.GetUploads("somestate", 1, 1);

            // Assert
            Assert.Single(response.Data);
            Assert.Single(response.Data, (u) => u.State == "somestate");
            Assert.Single(response.Data, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public async Task GetLatestUploadsByState()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUploadsByState())
                .Returns(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var response = await service.GetLatestUploadsByState();

            // Assert
            Assert.Single(response.Data);
            Assert.Single(response.Data, (u) => u.State == "somestate");
            Assert.Single(response.Data, (u) => u.UploadedAt == uploadedAt);
        }

        [Fact]
        public async Task AddUpload()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(1);
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var nRows = await service.AddUpload("somestate", uploadedAt);

            // Assert
            Assert.Equal(1, nRows);
            uploadDao.Verify(m => m.AddUpload("somestate", uploadedAt), Times.Once);
        }
    }
}