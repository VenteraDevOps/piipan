using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Shared.API.Utilities;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using Azure;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests.Parsers
{
    public class BlobClientStreamTests
    {

        private string EventString = "{\"topic\":\"/subscriptions/719bb99b-1a3b-4132-a0f6-1805a75dc30e/resourceGroups/rg-core-dev/providers/Microsoft.Storage/storageAccounts/ttssteauploaddev\",\"subject\":\"/blobServices/default/containers/upload/blobs/example333.csv\",\"eventType\":\"Microsoft.Storage.BlobCreated\",\"id\":\"0b6dcc46-401e-00eb-2f8b-5946db06a8ee\",\"data\":{\"api\":\"PutBlob\",\"requestId\":\"0b6dcc46-401e-00eb-2f8b-5946db000000\",\"eTag\":\"0x8DA27A31DCB5337\",\"contentType\":\"text/plain\",\"contentLength\":6592,\"blobType\":\"BlockBlob\",\"url\":\"https://ttssteauploaddev.blob.core.windows.net/upload/example333.csv\",\"sequencer\":\"00000000000000000000000000002A0D0000000001b0fc63\",\"storageDiagnostics\":{\"batchId\":\"ce49b6a6-f006-00f8-008b-598bff000000\"}},\"dataVersion\":\"\",\"metadataVersion\":\"1\",\"eventTime\":\"2022-04-26T16:37:55.9373378Z\"}";

        [Fact]
        public void ParseEventReturnStream()
        {

            var logger = new Mock<ILogger>();
            // var blobClientStream = new BlobClientStream();

            var blobClientStream = new Mock<BlobClientStream>();
                blobClientStream
                    .Setup(m => m.GetBlob(It.IsAny<string>()))
                    .Returns(new Mock<BlockBlobClient>().Object);

            // Act
            var streamValue = blobClientStream.Object.Parse(EventString, logger.Object);

            //Assert
            Assert.Equal(streamValue.GetType(), typeof(System.IO.MemoryStream));

        }

        [Fact]
        public void BlobClientPropertiesTest()
        {

            // Arrange
            var logger = new Mock<ILogger>();

            var blobClient = new Mock<BlobClient>();
            var responseMock = new Mock<Response>();

            var blobClientStream = new Mock<BlobClientStream>();
                blobClientStream
                    .Setup(m => m.GetBlob(It.IsAny<string>()))
                    .Returns(new Mock<BlockBlobClient>().Object);
                blobClientStream
                    .Setup(m => m.GetBlobProperties(It.IsAny<BlockBlobClient>()))
                    .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));


            // Act
            var blobProperties = blobClientStream.Object.BlobClientProperties(EventString, logger.Object);

            //Assert
            Assert.Equal(blobProperties.GetType(), typeof(BlobProperties));

        }

        [Fact]
        public void GetBlobNameTest()
        {

            // Arrange
            var queuedEvent = Azure.Messaging.EventGrid.EventGridEvent.Parse(BinaryData.FromString(EventString));
            var createdBlobEvent = queuedEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();
            var blobClientStream = new BlobClientStream();

            // Act
            var blobName = blobClientStream.GetBlobName(createdBlobEvent);

            //Assert
            Assert.Equal("example333.csv", blobName);

        }

        [Fact]
        public void GetBlobPropertiesTest()
        {

            // Arrange
            var blobClientStream = new BlobClientStream();
            var blob = Mock.Of<BlockBlobClient>();

            var blobClient = new Mock<BlockBlobClient>();
            var responseMock = new Mock<Response>();
            blobClient
                .Setup(m => m.GetProperties(null, CancellationToken.None))
                .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));

            // Act
            var response = blobClientStream.GetBlobProperties(blobClient.Object);

            //Assert
            Assert.Equal(typeof(BlobProperties), response.Value.GetType());

        }

      

    }
}