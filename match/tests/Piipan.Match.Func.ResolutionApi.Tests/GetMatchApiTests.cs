using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Func.ResolutionApi;
using Piipan.Match.Func.ResolutionApi.Models;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class GetMatchApiTests
    {

        static GetMatchApi Construct()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );
            return api;
        }

        static Mock<HttpRequest> MockGetRequest(string matchId = "foo")
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
                { "X-Initiating-State", "ea"}
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async Task GetMatch_LogsRequest()
        {
            // Arrange
            var api = Construct();
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();

            // Act
            await api.GetMatch(mockRequest.Object, "foo", logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using APIM subscription sub-name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async Task GetMatch_Returns404IfNotFound()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();
             // Mocks
            var matchRecord = new MatchRecordDbo();
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("not found error"));

            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();

            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );

            // Act
            var response = await api.GetMatch(mockRequest.Object, "foo", logger.Object);

            // Assert
            var result = response as NotFoundObjectResult;
            Assert.Equal(404, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("404", errorResponse.Errors[0].Status);
            Assert.Equal("not found", errorResponse.Errors[0].Detail);
            Assert.Contains("NotFoundException", errorResponse.Errors[0].Title);
        }

        [Fact]
        public async Task GetMatch_ReturnsIfFound()
        {
            // Arrange
            var mockRequest = MockGetRequest();
            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Ocp-Apim-Subscription-Name", "sub-name" }
                }));
            var logger = new Mock<ILogger>();

            // Mock Dao response
            var matchRecordDao = new Mock<IMatchRecordDao>();
            matchRecordDao
                .Setup(r => r.GetRecordByMatchId(It.IsAny<string>()))
                .ReturnsAsync(new MatchRecordDbo());
                var mock = new Mock<IMatchResEventDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            matchResEventDao
                .Setup(r => r.GetEvents(
                    It.IsAny<string>(),
                    It.IsAny<bool>()
                ))
                .ReturnsAsync(new List<IMatchResEvent>());
            var matchResAggregator = new Mock<IMatchResAggregator>();
            matchResAggregator
                .Setup(r => r.Build(It.IsAny<IMatchRecord>(), It.IsAny<IEnumerable<IMatchResEvent>>()))
                .Returns(new MatchResRecord(){
                    Status = "open"
                });

            var api = new GetMatchApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );

            // Act
            var response = await api.GetMatch(mockRequest.Object, "foo", logger.Object) as JsonResult;

            //Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);

            var resBody = response.Value as ApiResponse;
            Assert.NotNull(resBody);
            Assert.Equal("open", resBody.Data.Status);
        }
    }
}
