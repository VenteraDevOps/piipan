using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Notifications.Func.Api;
using Xunit;

namespace Piipan.Notifications.Core.IntegrationTests
{
    public class NotificationApiTest
    {
        private EventGridEvent MockEvent()
        {
            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {
                ToList = It.IsAny<string>().Split(',').ToList(),
                ToCCList = It.IsAny<string>()?.Split(',').ToList(),
                ToBCCList = It.IsAny<string>()?.Split(',').ToList(),
                Body = It.IsAny<string>(),
                Subject = It.IsAny<string>(),
                From = It.IsAny<string>()
            }));
            return gridEvent.Object;
        }

        private EventGridEvent MockBadEvent(DateTime eventTime, string State)
        {
            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {
            }));

            return gridEvent.Object;
        }

        [Fact]
        public async Task Run_Log_Success()
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
        [Fact]
        public async Task Run_Log_BadRequest()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var function = new NotificationApi();

            // Act
            await function.Run("", logger.Object);

            // Assert

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "No input was provided"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}