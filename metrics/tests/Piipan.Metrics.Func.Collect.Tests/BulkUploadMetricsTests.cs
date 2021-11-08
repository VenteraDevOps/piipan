using Piipan.Metrics.Func.Collect;
using Xunit;
using Moq;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class BulkUploadMetricsTests
    {
        private EventGridEvent MockEvent(string url, DateTime eventTime)
        {
            var gridEvent = Mock.Of<EventGridEvent>();
            gridEvent.Data = new { url = url };
            gridEvent.EventTime = eventTime;
            return gridEvent;
        }

        [Fact]
        public async Task Run_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            await function.Run(MockEvent("https://somethingeaupload", now), logger.Object);

            // Assert
            uploadApi.Verify(m => m.AddUpload("ea", now), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Theory]
        [InlineData("badurl", "State not found")] // malformed url, can't parse the state
        [InlineData("https://eupload", "State not found")] // state is only one character
        public async Task Run_BadUrl(string url, string expectedLogMessage)
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(1);

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            await Assert.ThrowsAsync<FormatException>(() => function.Run(MockEvent(url, now), logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedLogMessage),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Run_UploadApiThrows()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadWriterApi>();
            uploadApi
                .Setup(m => m.AddUpload(It.IsAny<string>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new BulkUploadMetrics(uploadApi.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => function.Run(MockEvent("https://somethingeaupload", now), logger.Object));

            // Assert
            uploadApi.Verify(m => m.AddUpload("ea", now), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "upload api broke"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}