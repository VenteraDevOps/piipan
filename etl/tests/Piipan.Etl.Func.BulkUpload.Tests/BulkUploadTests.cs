using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class BulkUploadTests
    {
        private Stream ToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
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
            var blobProperties = Mock.Of<BlobClient>();
            var participantStreamParser = Mock.Of<IParticipantStreamParser>();
            var blobStream = Mock.Of<IBlobClientStream>();
            var logger = new Mock<ILogger>();
            var function = new BulkUpload(participantApi, participantStreamParser, blobStream);

            // Act
            await function.Run(null, logger.Object);

            // Assert
            VerifyLogError(logger, "No input stream was provided");
        }

        [Fact]
        public async void Run_EmptyQueue()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var participantStreamParser = new Mock<IParticipantStreamParser>();
            var blobStream = Mock.Of<IBlobClientStream>();
            var logger = new Mock<ILogger>();

            var function = new BulkUpload(participantApi, participantStreamParser.Object, blobStream);

            // Act 
            await function.Run("", logger.Object);
            
            // Assert
            VerifyLogError(logger, "No input stream was provided");
        }

        [Fact]
        public async void Run_ParserThrows()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var blobProperties = Mock.Of<BlobClient>();
            var participantStreamParser = new Mock<IParticipantStreamParser>();
            participantStreamParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .Throws(new Exception("the parser broke"));

            var logger = new Mock<ILogger>();

            var responseMock = new Mock<Response>();

            var blockBlobClient = new Mock<BlockBlobClient>();
            blockBlobClient
                .Setup(m => m.GetProperties(null, CancellationToken.None))
                .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));

            var blobClientStream = new Mock<IBlobClientStream>();
                blobClientStream
                    .Setup(m => m.Parse(It.IsAny<string>(), logger.Object))
                    .Returns(blockBlobClient.Object);

            var function = new BulkUpload(participantApi, participantStreamParser.Object, blobClientStream.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run("Event Grid Event String", logger.Object));
            VerifyLogError(logger, "the parser broke");
        }


        [Fact]
        public async void Run_ApiThrows()
        {
            // Arrange
            var blobClient = new Mock<BlobClient>();
        
            var participantApi = new Mock<IParticipantApi>();
            participantApi
                .Setup(m => m.AddParticipants(It.IsAny<IEnumerable<IParticipant>>(), It.IsAny<string>()))
                .Throws(new Exception("the api broke"));

            var participantStreamParser = Mock.Of<IParticipantStreamParser>();

            var logger = new Mock<ILogger>();

             var responseMock = new Mock<Response>();

            var blockBlobClient = new Mock<BlockBlobClient>();
            blockBlobClient
                .Setup(m => m.GetProperties(null, CancellationToken.None))
                .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));

            var blobClientStream = new Mock<IBlobClientStream>();
                blobClientStream
                    .Setup(m => m.Parse(It.IsAny<string>(), logger.Object))
                    .Returns(blockBlobClient.Object);

            var function = new BulkUpload(participantApi.Object, participantStreamParser, blobClientStream.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run("Event Grid Event String", logger.Object));

            VerifyLogError(logger, "the api broke");
        }

        [Fact]
        public async void Run_ParsedInputPassedToApi()
        {

            // Arrange
            
            var responseMock = new Mock<Response>();

            var blockBlobClient = new Mock<BlockBlobClient>();
            blockBlobClient
                .Setup(m => m.GetProperties(null, CancellationToken.None))
                .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));
            
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
                    VulnerableIndividual = (new Random()).Next(2) == 1
                }
            };

            var participantStreamParser = new Mock<IParticipantStreamParser>();
            participantStreamParser
                .Setup(m => m.Parse(It.IsAny<Stream>()))
                .Returns(participants);

            var participantApi = new Mock<IParticipantApi>();
            var logger = new Mock<ILogger>();

            var blobClientStream = new Mock<IBlobClientStream>();
                blobClientStream
                    .Setup(m => m.Parse(It.IsAny<string>(), logger.Object))
                    .Returns(blockBlobClient.Object);

            var function = new BulkUpload(participantApi.Object, participantStreamParser.Object, blobClientStream.Object);

            // Act
            await function.Run("Event Grid Event String", logger.Object);

            // Assert
            participantApi.Verify(m => m.AddParticipants(participants, It.IsAny<string>()), Times.Once);
        }
    }
}
