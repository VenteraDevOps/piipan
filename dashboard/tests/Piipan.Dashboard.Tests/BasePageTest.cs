using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Language.Flow;
using Piipan.Dashboard.Pages;
using Piipan.Shared.Claims;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.Dashboard.Tests
{
    public class BasePageTest
    {

        protected static HttpContext contextMock(Mock<HttpRequest> request = null)
        {
            request ??= new Mock<HttpRequest>();
            var defaultHttpContext = new DefaultHttpContext();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            request
                .Setup(m => m.Headers)
                .Returns(new HeaderDictionary());

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);
            context.Setup(m => m.Response).Returns(defaultHttpContext.Response);

            return context.Object;
        }
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

        protected async Task OnPageHandlerExecutionAsync<T>(T pageModel, string methodName) where T : BasePageModel
        {
            pageModel.PageContext.HttpContext = contextMock();

            var pageContext = new PageContext(new ActionContext(
                pageModel.PageContext.HttpContext,
                new RouteData(),
                new PageActionDescriptor(),
                new ModelStateDictionary()));
            var model = new Mock<PageModel>();

            var pageHandlerExecutingContext = new PageHandlerExecutedContext(
               pageContext,
               Array.Empty<IFilterMetadata>(),
               new HandlerMethodDescriptor() { MethodInfo = typeof(T).GetMethod(methodName) },
               model.Object);

            await pageModel.OnPageHandlerExecutionAsync(null, () => Task.FromResult(pageHandlerExecutingContext));
        }
    }
}
