using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Dashboard.Pages;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class ParticipantUploadsModelTests : BasePageTest
    {
        private GetParticipantUploadsResponse DefaultGetUploadsResponse => new GetParticipantUploadsResponse
        {
            Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload { State = "ea", UploadedAt = DateTime.Now }
                },
            Meta = new Meta()
        };

        private ParticipantUploadStatistics DefaultStatisticsResponse =>
            new ParticipantUploadStatistics
            {
                TotalComplete = 2,
                TotalFailure = 1
            };

        [Fact]
        public void BeforeOnGetAsync_TitleIsCorrect()
        {
            var pageModel = new ParticipantUploadsModel(
                Mock.Of<IParticipantUploadReaderApi>(),
                new NullLogger<ParticipantUploadsModel>(),
                serviceProviderMock()
            );
            pageModel.PageContext.HttpContext = contextMock().Object;

            Assert.Equal("Most recent upload from each state", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public void BeforeOnGetAsync_DefaultsAreCorrect()
        {
            ParticipantUploadRequestFilter filter = new ParticipantUploadRequestFilter();
            Assert.Equal(53, filter.PerPage);
            Assert.Equal(1, filter.Page);
        }

        [Fact]
        public void BeforeOnGetAsync_InitializesParticipantUploadResults()
        {
            var pageModel = new ParticipantUploadsModel(
                Mock.Of<IParticipantUploadReaderApi>(),
                new NullLogger<ParticipantUploadsModel>(),
                serviceProviderMock()
            );
            Assert.IsType<List<ParticipantUpload>>(pageModel.ParticipantUploadResults);
        }

        // sets participant uploads after Get request
        [Fact]
        public async void AfterOnGetAsync_SetsParticipantUploadResults()
        {
            // Arrange
            var response = DefaultGetUploadsResponse;

            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ReturnsAsync(response);
            participantApi
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()))
                .ReturnsAsync(DefaultStatisticsResponse);

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                new NullLogger<ParticipantUploadsModel>(),
                serviceProviderMock()
            );

            var httpContext = contextMock().Object;
            pageModel.PageContext.HttpContext = httpContext;

            // Act
            await pageModel.OnGetAsync();

            // assert
            Assert.Equal(response.Data.First(), pageModel.ParticipantUploadResults[0]);
            Assert.Equal(1, pageModel.UploadStatistics.TotalFailure);
            Assert.Equal(2, pageModel.UploadStatistics.TotalComplete);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task AfterOnGetAsync_ApiThrows(bool getUploadsFailed, bool getStatisticsFailed)
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();
            participantApi
                .Setup(m => m.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()))
                .ThrowsAsync(new Exception("api broke"));

            var getUploadsSetup = participantApi
                .Setup(m => m.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()));
            if (getUploadsFailed)
            {
                getUploadsSetup.ThrowsAsync(new Exception("api broke"));
            }
            else
            {
                getUploadsSetup.ReturnsAsync(DefaultGetUploadsResponse);
            }

            var getUploadsStatisticsSetup = participantApi
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()));
            if (getStatisticsFailed)
            {
                getUploadsStatisticsSetup.ThrowsAsync(new Exception("api broke"));
            }
            else
            {
                getUploadsStatisticsSetup.ReturnsAsync(DefaultStatisticsResponse);
            }


            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                serviceProviderMock()
            );

            // Act
            await pageModel.OnGetAsync();

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task AfterOnGetAsync_ApiThrowsHttpRequestExecption(bool getUploadsFailed, bool getStatisticsFailed)
        {
            // Arrange
            var participantApi = new Mock<IParticipantUploadReaderApi>();

            var getUploadsSetup = participantApi
                .Setup(m => m.GetUploads(It.IsAny<ParticipantUploadRequestFilter>()));
            if (getUploadsFailed)
            {
                getUploadsSetup.ThrowsAsync(new HttpRequestException("api broke"));
            }
            else
            {
                getUploadsSetup.ReturnsAsync(DefaultGetUploadsResponse);
            }

            var getUploadsStatisticsSetup = participantApi
                .Setup(m => m.GetUploadStatistics(It.IsAny<ParticipantUploadStatisticsRequest>()));
            if (getStatisticsFailed)
            {
                getUploadsStatisticsSetup.ThrowsAsync(new HttpRequestException("api broke"));
            }
            else
            {
                getUploadsStatisticsSetup.ReturnsAsync(DefaultStatisticsResponse);
            }

            var logger = new Mock<ILogger<ParticipantUploadsModel>>();

            var pageModel = new ParticipantUploadsModel(
                participantApi.Object,
                logger.Object,
                serviceProviderMock()
            );

            // Act
            await pageModel.OnGetAsync();

            // Assert
            Assert.Contains("You may be able to try again", pageModel.RequestError);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("api broke")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }


        public static Mock<HttpRequest> requestMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            return request;
        }

        public static Mock<HttpContext> contextMock()
        {
            var request = requestMock();

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context;
        }
    }
}
