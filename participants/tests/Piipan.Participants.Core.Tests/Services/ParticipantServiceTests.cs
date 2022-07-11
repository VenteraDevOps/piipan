using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Enums;
using Piipan.Participants.Core.Models;
using Piipan.Participants.Core.Services;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Participants.Core.Tests.Services
{
    public class ParticipantServiceTests
    {
        private IRedactionService _redactionService;
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public ParticipantServiceTests()
        {
            _redactionService = Mock.Of<IRedactionService>();
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        private IEnumerable<ParticipantDbo> RandomParticipants(int n)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    CaseId = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    ParticipantId = cryptographyClient.EncryptToBase64String(Guid.NewGuid().ToString()),
                    ParticipantClosingDate = DateTime.UtcNow.Date,
                    RecentBenefitIssuanceDates = new List<DateRange>(),
                    VulnerableIndividual = (new Random()).Next(2) == 0,
                    UploadId = (new Random()).Next()
                });
            }

            return result;
        }

        private IEnumerable<ParticipantDbo> RandomParticipantsWithError(int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (i == 1)
                {
                    throw new Exception("Some parsing exception");
                }
                yield return new ParticipantDbo
                {
                    LdsHash = Guid.NewGuid().ToString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    ParticipantClosingDate = DateTime.UtcNow.Date,
                    RecentBenefitIssuanceDates = new List<DateRange>(),
                    VulnerableIndividual = (new Random()).Next(2) == 0,
                    UploadId = (new Random()).Next()
                };
            }
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

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            // Act
            var result = await service.GetParticipants(randomState, randomLdsHash);

            // Assert
            // results should have the State set
            var expected = participants.Select(p => new ParticipantDto(p) { 
                State = randomState,
                ParticipantId = cryptographyClient.DecryptFromBase64String(p.ParticipantId),
                CaseId = cryptographyClient.DecryptFromBase64String(p.CaseId),
            });
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

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

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

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

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
                .Setup(m => m.AddUpload("test-etag"))
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var stateService = Mock.Of<IStateService>();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            // Act
            await service.AddParticipants(participants, "test-etag", "ea", null);

            // Assert

            // we should add a new upload for this batch
            uploadDao.Verify(m => m.AddUpload("test-etag"), Times.Once);

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadId)
                    ))
                );

        }

        /// <summary>
        /// When Add Participants has an error, the error is logged with the value of the redaction service.
        /// </summary>
        [Fact]
        public async Task AddParticipants_ThrowsExceptionWhenFailed()
        {
            // Arrange
            var logger = new Mock<ILogger<ParticipantService>>();
            var participants = RandomParticipants(10);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            const string exceptionMessage = "Unhandled error!";
            var thrownException = new Exception(exceptionMessage);
            participantDao.Setup(n => n.AddParticipants(It.IsAny<IEnumerable<ParticipantDbo>>()))
                                .ThrowsAsync(thrownException);
            Exception foundException = null;

            var uploadDao = new Mock<IUploadDao>();
            UploadDbo uploadRecord = new UploadDbo
            {
                Id = uploadId,
                CreatedAt = DateTime.UtcNow,
                Publisher = "me"
            };
            uploadDao
                .Setup(m => m.AddUpload("test-etag"))
                .ReturnsAsync(uploadRecord);

            var stateService = Mock.Of<IStateService>();
            var redactionService = new Mock<IRedactionService>();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                redactionService.Object,
                logger.Object,
                cryptographyClient,
                participantPublishUploadMetric.Object
                );

            // Act
            await service.AddParticipants(participants, "test-etag", "ea", (ex) => foundException = ex);

            // Assert
            Assert.Equal(thrownException, foundException);

            participantPublishUploadMetric.Verify(m => m.PublishUploadMetric(
                        It.Is<ParticipantUpload>(s => s.Status == UploadStatuses.FAILED.ToString() && s.ErrorMessage == exceptionMessage)));
            uploadDao.Verify(x => x.UpdateUploadStatus(It.Is<IUpload>(u => u == uploadRecord), UploadStatuses.FAILED.ToString()));

        }

        /// <summary>
        /// When Add Participants has an error, the error is logged with the value of the redaction service.
        /// </summary>
        [Fact]
        public void LogParticipantsUploadError_LogsRedactedError()
        {
            // Arrange
            var logger = new Mock<ILogger<ParticipantService>>();
            var participants = RandomParticipants(10);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload("test-etag"))
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var stateService = Mock.Of<IStateService>();
            var redactionService = new RedactionService();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                redactionService,
                logger.Object,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            var uploadDetails = new ParticipantUploadErrorDetails("EA", DateTime.UtcNow, DateTime.UtcNow, new Exception("Exception with first participant: " + participants.First().LdsHash), "test.csv");

            // Act
            service.LogParticipantsUploadError(uploadDetails, participants);

            // Assert
            logger.Verify(n => n.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString() == $"Error uploading participants: {uploadDetails.ToString().Replace(participants.First().LdsHash, "REDACTED")}"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                  Times.Once());
        }

        /// <summary>
        /// When Add Participants has an error, the error is logged with the value of the redaction service.
        /// In the event that there's a parsing error, we should still attempt to redact the entries prior to the parsing error.
        /// </summary>
        [Fact]
        public void LogParticipantsUploadError_LogsRedactedErrorEvenAfterException()
        {
            // Arrange
            var logger = new Mock<ILogger<ParticipantService>>();
            var participants = RandomParticipantsWithError(10);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();

            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload("test-etag"))
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var stateService = Mock.Of<IStateService>();
            var redactionService = new RedactionService();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                redactionService,
                logger.Object,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            var uploadDetails = new ParticipantUploadErrorDetails("EA", DateTime.UtcNow, DateTime.UtcNow, new Exception("Exception with first participant: " + participants.First().LdsHash), "test.csv");

            // Act
            service.LogParticipantsUploadError(uploadDetails, participants);

            // Assert
            logger.Verify(n => n.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString() == $"Error uploading participants: {uploadDetails.ToString().Replace(participants.First().LdsHash, "REDACTED")}"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                  Times.Once());

            // Assert that we wrote an error log stating which row we failed to redact
            logger.Verify(n => n.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object v, Type _) => v.ToString() == $"Error parsing participant at index 1"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                  Times.Once());
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
                .ReturnsAsync(new List<string> { "ea", "eb", "ec" });

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao,
                uploadDao,
                stateService.Object,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            // Act
            var result = await service.GetStates();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Collection(result,
                s => Assert.Equal("ea", s),
                s => Assert.Equal("eb", s),
                s => Assert.Equal("ec", s));

        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void DeleteOldParticipants_AddedCoupleOfUploads(int nParticipants)
        {
            // Arrange
            var logger = Mock.Of<ILogger<ParticipantService>>();
            var participants = RandomParticipants(nParticipants);
            var uploadId = (new Random()).Next();
            var participantDao = new Mock<IParticipantDao>();
            var uploadDao = new Mock<IUploadDao>();
            uploadDao
                .Setup(m => m.AddUpload("test-etag"))
                .ReturnsAsync(new UploadDbo
                {
                    Id = uploadId,
                    CreatedAt = DateTime.UtcNow,
                    Publisher = "me"
                });

            var stateService = Mock.Of<IStateService>();

            var participantPublishUploadMetric = new Mock<IParticipantPublishUploadMetric>();
                participantPublishUploadMetric.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

            var service = new ParticipantService(
                participantDao.Object,
                uploadDao.Object,
                stateService,
                _redactionService,
                logger,
                cryptographyClient,
                participantPublishUploadMetric.Object);

            // Act
            await service.AddParticipants(participants, "test-etag", "ea", null);

            // Now Add another Upload 
            var uploadIdNew = (new Random()).Next();
            var uploadDaoNew = new Mock<IUploadDao>();
            uploadDaoNew
               .Setup(m => m.AddUpload("test-etag1"))
               .ReturnsAsync(new UploadDbo
               {
                   Id = uploadIdNew,
                   CreatedAt = DateTime.UtcNow,
                   Publisher = "me",
               });

            var serviceNew = new ParticipantService(
               participantDao.Object,
               uploadDaoNew.Object,
               stateService,
               _redactionService,
               logger,
                cryptographyClient,
               participantPublishUploadMetric.Object);
            await serviceNew.AddParticipants(participants, "test-etag1", "ea", null);

            // Assert

            // we should add a new upload for this batch
            uploadDao.Verify(m => m.AddUpload("test-etag"), Times.Once);
            uploadDaoNew.Verify(m => m.AddUpload("test-etag1"), Times.Once);

            var uploadDaoDelete = new Mock<IUploadDao>();
            uploadDaoDelete
               .Setup(m => m.GetLatestUpload(It.IsAny<string>()))
               .ReturnsAsync(new UploadDbo
               {
                   Id = uploadIdNew,
                   CreatedAt = DateTime.UtcNow,
                   Publisher = "me",
               });

            var serviceDelete = new ParticipantService(
             participantDao.Object,
             uploadDaoDelete.Object,
             stateService,
             _redactionService,
             logger,
             cryptographyClient,
             participantPublishUploadMetric.Object);

            await serviceDelete.DeleteOldParticpants();

            // each participant added via the DAO should have the created upload ID
            participantDao
                .Verify(m => m
                    .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                        participants.All(p => p.UploadId == uploadIdNew)
                    ))
                );
            participantDao
               .Verify(m => m
                   .AddParticipants(It.Is<IEnumerable<ParticipantDbo>>(participants =>
                       participants.All(p => p.UploadId != uploadId)
                   ))
               );
        }

    }
}
