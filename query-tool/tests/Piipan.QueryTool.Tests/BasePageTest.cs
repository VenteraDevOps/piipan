using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.QueryTool.Tests
{
    public class BasePageTest
    {
        public static IServiceProvider serviceProviderMock(string email = "noreply@tts.test",
            string location = "EA", string role = "Worker", string[] states = null)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            claimsProviderMock
                .Setup(c => c.GetLocation(It.IsAny<ClaimsPrincipal>()))
                .Returns(location);
            claimsProviderMock
                .Setup(c => c.GetRole(It.IsAny<ClaimsPrincipal>()))
                .Returns(role);

            var locationProviderMock = new Mock<ILocationsProvider>();
            locationProviderMock.Setup(c => c.GetStates(It.IsAny<string>())).Returns(states ?? new string[] { location });

            var statesApiMock = new Mock<IStatesApi>();

            // declare it as object so MemoryCache setup works.
            object stateInfoResponse = new StatesInfoResponse
            {
                Results = new System.Collections.Generic.List<StateInfoResponseData>
                {
                    new StateInfoResponseData
                    {
                        Email = "ea-test@usda.gov",
                        Phone = "123-123-1234",
                        State = "Echo Alpha",
                        StateAbbreviation = "EA"
                    }
                }
            };

            statesApiMock.Setup(c => c.GetStates()).ReturnsAsync(stateInfoResponse as StatesInfoResponse);

            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache.Setup(n => n.TryGetValue("StateInfo", out stateInfoResponse))
                .Returns(true);

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(claimsProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(locationProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IStatesApi))).Returns(statesApiMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IMemoryCache))).Returns(mockMemoryCache.Object);
            return serviceProviderMock.Object;
        }

        protected static HttpContext contextMock()
        {
            var request = new Mock<HttpRequest>();
            var defaultHttpContext = new DefaultHttpContext();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));


            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);
            context.Setup(m => m.Response).Returns(defaultHttpContext.Response);

            return context.Object;
        }
    }
}
