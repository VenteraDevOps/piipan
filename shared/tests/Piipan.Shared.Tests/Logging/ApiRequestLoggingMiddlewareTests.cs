using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Shared.Logging.Tests
{
    public class ApiRequestLoggingMiddlewareTests
    {
        [Fact]
        public async void InvokeAsync_Succeeds()
        {
            // Arrange
            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));
            var middleware = new ApiRequestLoggingMiddleware(requestDelegate);

            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { "Ocp-Apim-Subscription-Name", "sub-name" },
                { "From", "foobar"},
                { "X-Initiating-State", "ea"}
            }) as IHeaderDictionary;
            var httpRequest = new Mock<HttpRequest>();
            httpRequest
                .Setup(m => m.Headers)
                .Returns(headers);

            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(m => m.Request)
                .Returns(httpRequest.Object);

            var logger = new Mock<ILogger<RequestLoggingMiddleware>>();

            // Act
            await middleware.InvokeAsync(httpContext.Object, logger.Object);

            // Assert
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using APIM subscription sub-name")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }
    }
}
