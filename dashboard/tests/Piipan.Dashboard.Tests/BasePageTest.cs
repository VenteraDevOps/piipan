using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Language.Flow;
using Piipan.Shared.Claims;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.Dashboard.Tests
{
    public class BasePageTest
    {
        public IServiceProvider serviceProviderMock(Action<ISetup<IStatesApi, Task<StatesInfoResponse>>> statesInfoResponseOverride = null)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(m => m.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns("noreply@tts.test");

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(claimsProviderMock.Object);

            var statesApiMock = new Mock<IStatesApi>();
            // declare it as object so MemoryCache setup works.
            StatesInfoResponse defaultStateInfoResponse = new StatesInfoResponse
            {
                Results = new System.Collections.Generic.List<StateInfoResponseData>
                {
                    new StateInfoResponseData
                    {
                        Email = "ea-test@usda.example",
                        Phone = "123-123-1234",
                        State = "Echo Alpha",
                        StateAbbreviation = "EA"
                    }
                }
            };

            var statesSetup = statesApiMock.Setup(c => c.GetStates());
            if (statesInfoResponseOverride != null)
            {
                statesInfoResponseOverride.Invoke(statesSetup);
            }
            else
            {
                statesSetup.ReturnsAsync(defaultStateInfoResponse);
            }
            serviceProviderMock.Setup(c => c.GetService(typeof(IStatesApi))).Returns(statesApiMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IMemoryCache))).Returns(new MemoryCache(new MemoryCacheOptions()));
            return serviceProviderMock.Object;
        }
    }
}
