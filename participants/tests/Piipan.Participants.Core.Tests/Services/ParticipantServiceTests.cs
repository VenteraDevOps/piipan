using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Piipan.Shared.Utilities;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantServiceTests
    {
        private IEnumerable<ParticipantDbo> RandomParticipants(int n)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = Guid.NewGuid().ToString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    ParticipantClosingDate = DateTime.UtcNow.Date,
                    RecentBenefitIssuanceDates = new List<DateRange>(),
                    VulnerableIndividual = (new Random()).Next(2) == 0,
                    UploadId = (new Random()).Next()
                });
            }

            return result;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetParticipants_AllMatchesReturned(int nMatches)
        {
            // Arrange
            var randomLdsHash = Guid.NewGuid().ToString();
            var randomState = Guid.NewGuid().ToString().Substring(0, 2);

            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nMatches);
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(randomState, randomLdsHash, It.IsAny<Int64>()))
                .ReturnsAsync(participants);
            
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ReturnsAsync(new UploadDbo
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object, 
                uploadDao.Object, 
                stateService,
                logger);

            // Act
            var result = await service.GetParticipants(randomState, randomLdsHash);

            // Assert
            // results should have the State set
            var expected = participants.Select(p => new ParticipantDto(p) { State = randomState });
            Assert.Equal(expected, result);
        }

        [Fact]
        public async void GetParticipants_UsesLatestUploadId()
        {
            // Arrange
            var randomLdsHash = Guid.NewGuid().ToString();
            var randomState = Guid.NewGuid().ToString().Substring(0, 2);

            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(randomState, randomLdsHash, It.IsAny<Int64>()))
                .ReturnsAsync(new List<ParticipantDbo>());
            
            var uploadId = (new Random()).Next();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "someone"
                });

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object, 
                uploadDao.Object, 
                stateService,
                logger);

            // Act
            var result = await service.GetParticipants(randomState, randomLdsHash);

            // Assert
            uploadDao.Verify(m => m.GetLatestUpload(It.IsAny<string>()), Times.Once);
            participantDao.Verify(m => m.GetParticipants(randomState, randomLdsHash, uploadId), Times.Once);
        }

        [Fact]
        public async Task GetParticipants_ReturnsEmptyWhenNoUploads()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = new Mock<IParticipantDao>();
            participantDao
                .Setup(m => m.GetParticipants(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Int64>()))
                .ReturnsAsync(new List<ParticipantDbo>());
            
            var uploadId = (new Random()).Next();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object, 
                uploadDao.Object, 
                stateService,
                logger);

            // Act
            var participants = await service.GetParticipants("ea", "hash");

            // Assert
            Assert.Empty(participants);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants_AllAddedWithUploadId(int nParticipants)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nParticipants);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload())
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var stateService = Mock.Of<IStateService>();

            var service = new ParticipantService(
                participantDao.Object, 
                uploadDao.Object, 
                stateService,
                logger);

            // Act
            await service.AddParticipants(participants);

            // Assert
            
            // we should add a new upload for this batch
            uploadDao.Verify(m => m.AddUpload(), Times.Once);

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadId)
                    ))
                );

        }

        [Fact]
        public async void GetStates_ReturnsDaoResult()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participantDao = Mock.Of<IParticipantDao>();
            var uploadDao = Mock.Of<IUploadDao>();
            
            var stateService = new Mock<IStateService>();
            stateService
                .Setup(m => m.GetStates())
                .ReturnsAsync(new List<string>{ "ea", "eb", "ec" });

            var service = new ParticipantService(
                participantDao, 
                uploadDao, 
                stateService.Object,
                logger);

            // Act
            var result = await service.GetStates();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Collection(result,
                s => Assert.Equal("ea", s),
                s => Assert.Equal("eb", s),
                s => Assert.Equal("ec", s));

        }
    }
}
