using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Utilities;
using Xunit;
using Azure.Storage.Blobs;

namespace Piipan.Etl.Func.BlobEventTriggerProcessing.Tests
{
    public class BlobEventTriggerProcessingTest
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

        private EventGridEvent EventMock()
        {
            var e = Mock.Of<EventGridEvent>();
            // Can't override Data in Setup, just use a real one
            e.Data = new Object();
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
        public async void Run_NullInputBlob()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var participantStreamParser = Mock.Of<IParticipantStreamParser>();
            var logger = new Mock<ILogger>();
            var function = new BlobEventTriggerProcessing(participantApi, participantStreamParser);

            // Act
            var name = function.Run(EventMock(), null, logger.Object);

            // Assert
            VerifyLogError(logger, "No input stream was provided");
            
        }

        [Fact]
        public async void Run_NullInputInvalidBlob()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var participantStreamParser = Mock.Of<IParticipantStreamParser>();
            var logger = new Mock<ILogger>();
            var function = new BlobEventTriggerProcessing(participantApi, participantStreamParser);
            var blob = Mock.Of<BlobClient>( m => m.Name == null);

            // Act
            var name = function.Run(EventMock(), blob, logger.Object);

            // Assert
            VerifyLogError(logger, "No input stream was provided");
            
        }

        [Fact]
        public async void Run_NullInputValidBlob()
        {
            // Arrange
            var participantApi = Mock.Of<IParticipantApi>();
            var participantStreamParser = Mock.Of<IParticipantStreamParser>();
            var logger = new Mock<ILogger>();
            var function = new BlobEventTriggerProcessing(participantApi, participantStreamParser);
            var blob = Mock.Of<BlobClient>( m => m.Name == "BlobName");
   
            // Act
            var name = function.Run(EventMock(), blob, logger.Object);

            // Assert
            Assert.Equal(name, "BlobName");
            
        }


    }
}
