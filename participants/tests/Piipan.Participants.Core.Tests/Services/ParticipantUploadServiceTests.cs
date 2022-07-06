using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantUploadServiceTests
    {
        ILogger<ParticipantUploadService> _logger;

        public ParticipantUploadServiceTests()
        {
            _logger = Mock.Of<ILogger<ParticipantUploadService>>();
        }

        [Fact]
        public async Task GetLatestUploadById()
        {
            // Arrange
            const string uploadId = "upload1";
            var createdAt = DateTime.Now;
            var uploadDao = new Mock<IUploadDao>();

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                UploadIdentifier = uploadId
            };

            uploadDao.Setup(m => m.GetUploadById(It.IsAny<string>())).ReturnsAsync(upload);

            var service = new ParticipantUploadService(uploadDao.Object, _logger);

            // Act
            var result = await service.GetUploadById(uploadId);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
        }

        [Fact]
        public async Task GetLatestUpload()
        {
            // Arrange
            const string state = "EA";
            var createdAt = DateTime.Now;
            var uploadDao = new Mock<IUploadDao>();
            
            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
            };

            uploadDao.Setup(m => m.GetLatestUpload(It.IsAny<string>())).ReturnsAsync(upload);

            var service = new ParticipantUploadService(uploadDao.Object, _logger);

            // Act
            var result = await service.GetLatestUpload(state);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
        }

        [Fact]
        public async Task AddUpload()
        {
            // Arrange
            var createdAt = DateTime.Now;
            const string uploadIdentifier = "upload_1";

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                UploadIdentifier = uploadIdentifier
            };

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload(It.IsAny<string>()))
                .ReturnsAsync(upload);

            var service = new ParticipantUploadService(uploadDao.Object, _logger);

            // Act
            
            var result = await service.AddUpload(uploadIdentifier);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(uploadDto, result);
            uploadDao.Verify(m => m.AddUpload(It.Is<string>(x=>x==uploadIdentifier)), Times.Once);
        }

        [Fact]
        public async Task UpdateUpload()
        {
            // Arrange
            var createdAt = DateTime.Now;
            const string uploadIdentifier = "upload_1";

            IUpload upload = new UploadDbo
            {
                CreatedAt = createdAt,
                UploadIdentifier = uploadIdentifier
            };

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.UpdateUpload(It.IsAny<IUpload>()))
                .ReturnsAsync(1);

            var service = new ParticipantUploadService(uploadDao.Object, _logger);

            // Act

            var result = await service.UpdateUpload(upload);

            var uploadDto = new UploadDto(upload);

            // Assert
            Assert.Equal(1, result);
            uploadDao.Verify(m => m.UpdateUpload(It.Is<IUpload>(x => x.UploadIdentifier == uploadIdentifier)), Times.Once);
        }
    }
}
