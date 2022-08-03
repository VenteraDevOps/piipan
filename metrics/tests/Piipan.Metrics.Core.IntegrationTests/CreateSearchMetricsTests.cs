using Piipan.Metrics.Func.Collect;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;


namespace Piipan.Metrics.Core.IntegrationTests
{
    public class CreateSearchMetricsTests
    {

        private EventGridEvent MockEvent(DateTime eventTime, string State)
        {

            var gridEvent = new Mock<EventGridEvent>("", "", "", new BinaryData(new
            {
                State = "ea",
                search_reason = It.IsAny<string>(),
                search_from = It.IsAny<string>(),
                match_creation = It.IsAny<string>(),
                match_count = It.IsAny<int>(),
                searched_at = eventTime
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

            var searchMetricsApi = new Mock<IParticipantSearchWriterApi>();
            searchMetricsApi
                .Setup(m => m.AddSearchMetrics(
                    It.IsAny<ParticipantSearch>()))
                .ReturnsAsync(1);

            var function = new CreateSearchMetrics(searchMetricsApi.Object);

            // Act
            await function.Run(MockEvent(now, "ea"), logger.Object);

            // Assert
            searchMetricsApi.Verify(m => m.AddSearchMetrics(It.IsAny<ParticipantSearch>()), Times.Once);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Number of rows inserted=1"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }



        [Fact]
        public async Task Run_DB_BadParticipantUpload()
        {
            var now = DateTime.Now;
            var logger = new Mock<ILogger>();

            var searchMetricsApi = new Mock<IParticipantSearchWriterApi>();
            searchMetricsApi
                .Setup(m => m.AddSearchMetrics(
                    It.IsAny<ParticipantSearch>()))
                .ReturnsAsync(1);

            var function = new CreateSearchMetrics(searchMetricsApi.Object);


            // Act //Assert
            await Assert.ThrowsAsync<System.ArgumentException>(() => function.Run(MockBadEvent(now, null), logger.Object));

        }

    }
}
