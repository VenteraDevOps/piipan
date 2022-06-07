using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;

namespace Piipan.QueryTool.Tests
{
    public class BasePageTest
    {
        public static IServiceProvider serviceProviderMock(string email = "noreply@tts.test",
            string location = "IA", string role = "Worker", string[] states = null)
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

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(claimsProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(locationProviderMock.Object);
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

        protected async Task<PageHandlerExecutingContext> GetPageHandlerExecutingContext<T>(T pageModel, string methodName) where T : BasePageModel
        {
            pageModel.PageContext.HttpContext = contextMock();

            var pageContext = new PageContext(new ActionContext(
                pageModel.PageContext.HttpContext,
                new RouteData(),
                new PageActionDescriptor(),
                new ModelStateDictionary()));
            var model = new Mock<PageModel>();

            var pageHandlerExecutingContext = new PageHandlerExecutingContext(
               pageContext,
               Array.Empty<IFilterMetadata>(),
               new HandlerMethodDescriptor() { MethodInfo = typeof(T).GetMethod(methodName) },
               new Dictionary<string, object>(),
               model.Object);

            var pageHandlerExecutedContext = new PageHandlerExecutedContext(
               pageContext,
               Array.Empty<IFilterMetadata>(),
               new HandlerMethodDescriptor() { MethodInfo = typeof(T).GetMethod(methodName) },
               model.Object);

            PageHandlerExecutionDelegate pageHandlerExecutionDelegate = () => Task.FromResult(pageHandlerExecutedContext);

            await pageModel.OnPageHandlerExecutionAsync(pageHandlerExecutingContext, pageHandlerExecutionDelegate);

            return pageHandlerExecutingContext;

        }
    }
}
