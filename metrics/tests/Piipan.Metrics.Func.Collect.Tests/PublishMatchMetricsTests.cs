using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Metrics.Func.Collect.Tests
{
    public class PublishMatchMetricsTests
    {
        private EventGridEvent MockEvent(DateTime eventTime, string State)
        {

            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {

                match_id = "foo",
                init_state = It.IsAny<string>(),
                matching_state = It.IsAny<string>(),
                status = It.IsAny<string>()
            }));
            gridEvent.Object.EventTime = eventTime;
            return gridEvent.Object;

        }

        private EventGridEvent MockBadEvent(DateTime eventTime, string State)
        {

            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {
                example = "123"
            }));
            gridEvent.Object.EventTime = eventTime;
            return gridEvent.Object;

        }


        [Fact]
        public async Task Run_DB_Success()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var matchMetricsApi = new Mock<IParticipantMatchWriterApi>();
            matchMetricsApi
                .Setup(m => m.PublishMatchMetrics(
                    It.IsAny<ParticipantMatchMetrics>()))
                .ReturnsAsync(1);

            var function = new PublishMatchMetrics(matchMetricsApi.Object);

            // Act
            await function.Run(MockEvent(now, "ea"), logger.Object);

            // Assert
            matchMetricsApi.Verify(m => m.PublishMatchMetrics(It.IsAny<ParticipantMatchMetrics>()), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }



        [Fact]
        public async Task Run_DB_BadParticipantMatchMetrics()
        {
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var matchMetricsApi = new Mock<IParticipantMatchWriterApi>();
            matchMetricsApi
                .Setup(m => m.PublishMatchMetrics(
                    It.IsAny<ParticipantMatchMetrics>()))
                .ReturnsAsync(1);

            var function = new PublishMatchMetrics(matchMetricsApi.Object);


            // Act //Assert
            await Assert.ThrowsAsync<System.ArgumentException>(() => function.Run(MockBadEvent(now, null), logger.Object));

        }

    }
}