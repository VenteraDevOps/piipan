using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Metrics.Api;
using Piipan.Metrics.Func.Api.Extensions;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace Piipan.Metrics.Func.Api.Tests
{
    public class GetParticipantUploadsTests
    {
        [Fact]
        public async Task Run_Success()
        {
            // Arrange
            var uploadedAt = DateTime.Now;

            var context = new DefaultHttpContext();
            var request = context.Request;
            request.QueryString = new QueryString("");

            Assert.Equal(0, request.Query.ParseInt("perPage", 0));

            var logger = new Mock<ILogger>();

            var expectedResponse = new GetParticipantUploadsResponse
            {
                Data = new List<ParticipantUpload>
                {
                    new ParticipantUpload 
                    {
                        State = "ea",
                        UploadedAt = uploadedAt
                    }
                },
                Meta = new Meta
                {
                    Page = 1,
                    PerPage = 1,
                    Total = 1,
                    NextPage = null,
                    PrevPage = null
                }
            };

            var uploadApi = new Mock<IParticipantUploadReaderApi>();
            uploadApi
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            var function = new GetParticipantUploads(uploadApi.Object);

            // Act
            var result = (await function.Run(request, logger.Object)) as JsonResult;
            var response = result.Value as GetParticipantUploadsResponse;
            
            // Assert
            Assert.NotNull(response);
            Assert.Equal(response.Meta, expectedResponse.Meta);
            Assert.Equal(response.Data, expectedResponse.Data);
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing request from user")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task Run_UploadApiThrows()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var request = context.Request;

            var logger = new Mock<ILogger>();

            var uploadApi = new Mock<IParticipantUploadReaderApi>();
            uploadApi
                .Setup(m => m.GetUploads(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("upload api broke"));

            var function = new GetParticipantUploads(uploadApi.Object);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(() => function.Run(request, logger.Object));
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