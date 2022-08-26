using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notifications.Func.Api;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class NotificationTests
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

            var logger = new Mock<ILogger>();
            var function = new NotificationApi();

            // Act
            await function.Run(null, logger.Object);

            // Assert
            VerifyLogError(logger, "No input was provided");
        }

        [Fact]
        public async void Run_EmptyQueue()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var function = new NotificationApi();
            // Act
            await function.Run("", logger.Object);

            // Assert
            VerifyLogError(logger, "No input was provided");
        }

        [Fact]
        public async void Run_ParsedInputPassedToApi()
        {
            // Arrange

            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var function = new NotificationApi();

            // Act
            await function.Run("Event Grid Event String", logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Email Queue trigger function processed"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}