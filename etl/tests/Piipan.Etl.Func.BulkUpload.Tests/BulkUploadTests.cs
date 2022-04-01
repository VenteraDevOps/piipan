using System;
using System.Collections.Generic;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Services;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Utilities;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class BulkUploadTests
    {
        private const string FAKE_CONTAINER_URL = "https://myTest.blob.core.windows.net/mycontainer/example.csv";

        private Stream ToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        private BlobClient BlobClientMock()
        {
            Stream data = ToStream("data");

            var blobClient = new Mock<BlobClient>();
            blobClient.Setup(x => x.OpenRead(0, null, null, default)).Returns(data);

            return blobClient.Object;
        }

        private EventGridEvent EventMock()
        {
            var e = Mock.Of<EventGridEvent>();
            // Can't override Data in Setup, just use a real one
            e.Data = new StorageBlobCreatedEventData() { Url= FAKE_CONTAINER_URL };
            return e;
        }

        private void VerifyLogError(Mock<ILogger> logger, String expected)
        {
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expected),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async void Run_NullInputStream()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var blobRetrievalService = new Mock<ICustomerEncryptedBlobRetrievalService>();
            var participantStreamParser = Mock.Of<IParticipantStreamParser>();
            var logger = new Mock<ILogger>();

            var blobClient = new Mock<BlobClient>();

            blobRetrievalService
                .Setup(x => x.RetrieveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(blobClient.Object);

            var function = new BulkUpload(participantApi, participantStreamParser, blobRetrievalService.Object);

            // Act
            await function.Run(EventMock(), logger.Object);

            // Assert
            VerifyLogError(logger, "No input stream was provided");
        }

        [Fact]
        public async void Run_ParserThrows()
        {
            // Arrange
            var blobRetrievalService = new Mock<ICustomerEncryptedBlobRetrievalService>();
            var participantApi = Mock.Of<IParticipantApi>();
            var participantStreamParser = new Mock<IParticipantStreamParser>();
            participantStreamParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .Throws(new Exception("the parser broke"));

            var blobClient = BlobClientMock();
            
            blobRetrievalService
                .Setup(x => x.RetrieveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(blobClient);

            var logger = new Mock<ILogger>();
            var function = new BulkUpload(participantApi, participantStreamParser.Object, blobRetrievalService.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run(EventMock(), logger.Object));
            VerifyLogError(logger, "the parser broke");
        }

        [Fact]
        public async void Run_ApiThrows()
        {
            // Arrange
            var blobRetrievalService = new Mock<ICustomerEncryptedBlobRetrievalService>();

            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.AddParticipants(It.IsAny<IEnumerable<IParticipant>>()))
                .Throws(new Exception("the api broke"));

            var participantStreamParser = Mock.Of<IParticipantStreamParser>();

            var blobClient = BlobClientMock();

            blobRetrievalService
                .Setup(x => x.RetrieveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(blobClient);

            var logger = new Mock<ILogger>();
            var function = new BulkUpload(participantApi.Object, participantStreamParser, blobRetrievalService.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run(EventMock(), logger.Object));
            VerifyLogError(logger, "the api broke");
        }

        [Fact]
        public async void Run_ParsedInputPassedToApi()
        {
            // Arrange
            var blobRetrievalService = new Mock<ICustomerEncryptedBlobRetrievalService>();

            var participants = new List<Participant>
            {
                new Participant
                {
                    LdsHash = Guid.NewGuid().ToString(),
                    State = Guid.NewGuid().ToString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    ParticipantClosingDate = DateTime.UtcNow,
                    RecentBenefitIssuanceDates = new List<DateRange>(),
                    ProtectLocation = (new Random()).Next(2) == 1
                }
            };

            var participantStreamParser = new Mock<IParticipantStreamParser>();
            participantStreamParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .Returns(participants);

            var blobClient = BlobClientMock();

            blobRetrievalService
                .Setup(x => x.RetrieveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(blobClient);

            var participantApi = new Mock<IParticipantApi>();
            var logger = new Mock<ILogger>();
            var function = new BulkUpload(participantApi.Object, participantStreamParser.Object, blobRetrievalService.Object);

            // Act
            await function.Run(EventMock(), logger.Object);

            // Assert
            participantApi.Verify(m => m.AddParticipants(participants), Times.Once);
        }
    }
}
