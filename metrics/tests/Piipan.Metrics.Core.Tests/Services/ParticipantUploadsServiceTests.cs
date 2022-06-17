using System;
using System.Collections.Generic;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;
using Moq;
using Xunit;
using Piipan.Metrics.Core.Builders;
using System.Threading.Tasks;
using Piipan.Metrics.Core.Models;

namespace Piipan.Metrics.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    {
        [Fact]
        public async Task GetUploads()
        {
            // Arrange
            var uploadedAt = DateTime.Now;
            var uploadDao = new Mock<IParticipantUploadDao>();
            uploadDao
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<ParticipantUpload>()
                {
                    new ParticipantUpload
                    {
                        State = "somestate",
                        UploadedAt = uploadedAt,
                    }
                });
            var metaBuilder = new Mock<IMetaBuilder>();
            metaBuilder.Setup(m => m.SetPage(It.IsAny<int>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetPerPage(It.IsAny<int>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetState(It.IsAny<string>())).Returns(metaBuilder.Object);
            metaBuilder.Setup(m => m.SetTotal(It.IsAny<long>())).Returns(metaBuilder.Object);

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder.Object);

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
                .Setup(m => m.GetLatestSuccessfulUploadsByState())
                .ReturnsAsync(new List<ParticipantUpload>()
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
                .Setup(m => m.AddUpload(It.IsAny<ParticipantUploadDbo>()))
                .ReturnsAsync(1);
            var metaBuilder = Mock.Of<IMetaBuilder>();

            var service = new ParticipantUploadService(uploadDao.Object, metaBuilder);

            // Act
            var nRows = await service.AddUploadMetrics(new ParticipantUpload());

            // Assert
            Assert.Equal(1, nRows);
            uploadDao.Verify(m => m.AddUpload(It.IsAny<ParticipantUploadDbo>()), Times.Once);
        }
    }
}