using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Func.ResolutionApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.Match.Func.ResolutionApi.Tests
{
    public class GetMatchesApiTests
    {

        static GetMatchesApi Construct()
        {
            var matchRecordDao = new Mock<IMatchRecordDao>();
            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();
            var api = new GetMatchesApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );
            return api;
        }

        static Mock<HttpRequest> MockGetRequest()
        {
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "From", "foobar"},
            }) as IHeaderDictionary;
            mockRequest.Setup(x => x.Headers).Returns(headers);

            return mockRequest;
        }

        [Fact]
        public async Task GetMatches_LogsRequest()
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
            await api.GetMatches(mockRequest.Object, logger.Object);

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
        public async Task GetMatches_Returns500IfExceptionOccurs()
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
                .Setup(r => r.GetMatches())
                .ThrowsAsync(new Exception("Some database error"));

            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();

            var api = new GetMatchesApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );

            // Act
            var response = await api.GetMatches(mockRequest.Object, logger.Object);

            // Assert
            var result = response as JsonResult;
            Assert.Equal(500, result.StatusCode);

            var errorResponse = result.Value as ApiErrorResponse;
            Assert.Equal(1, (int)errorResponse.Errors.Count);
            Assert.Equal("500", errorResponse.Errors[0].Status);
            Assert.Equal("Some database error", errorResponse.Errors[0].Detail);
            Assert.Contains("Exception", errorResponse.Errors[0].Title);
        }

        [Fact]
        public async Task GetMatches_ReturnsEmptyListIfNotFound()
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
                .Setup(r => r.GetMatches())
                .ReturnsAsync(new List<IMatchRecord>());

            var matchResEventDao = new Mock<IMatchResEventDao>();
            var matchResAggregator = new Mock<IMatchResAggregator>();

            var api = new GetMatchesApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );

            // Act
            var response = await api.GetMatches(mockRequest.Object, logger.Object) as JsonResult;

            //Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);

            var resBody = response.Value as MatchResListApiResponse;
            Assert.NotNull(resBody);
            Assert.Empty(resBody.Data);
        }

        [Fact]
        public async Task GetMatches_ReturnsIfFound()
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
                .Setup(r => r.GetMatches())
                .ReturnsAsync(new List<MatchRecordDbo>()
                {
                    new MatchRecordDbo() { MatchId = "m123456" },
                    new MatchRecordDbo() { MatchId = "m654321" },
                });
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
                .Returns(new MatchResRecord()
                {
                    Status = "open"
                });

            var api = new GetMatchesApi(
                matchRecordDao.Object,
                matchResEventDao.Object,
                matchResAggregator.Object
            );

            // Act
            var response = await api.GetMatches(mockRequest.Object, logger.Object) as JsonResult;

            //Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);

            var resBody = response.Value as MatchResListApiResponse;
            Assert.NotNull(resBody);
            Assert.Equal(2, resBody.Data.Count());
        }
    }
}
