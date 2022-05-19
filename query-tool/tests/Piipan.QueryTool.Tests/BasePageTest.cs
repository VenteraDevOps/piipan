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
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Piipan.QueryTool.Tests
{
    public class BasePageTest
    {
        public static IClaimsProvider claimsProviderMock(string email = "noreply@tts.test",
            string state = "IA", string role = "Worker")
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            claimsProviderMock
                .Setup(c => c.GetState(It.IsAny<ClaimsPrincipal>()))
                .Returns(state);
            claimsProviderMock
                .Setup(c => c.GetRole(It.IsAny<ClaimsPrincipal>()))
                .Returns(role);
            return claimsProviderMock.Object;
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

        protected PageHandlerExecutingContext GetPageHandlerExecutingContext<T>(T pageModel, string methodName) where T : BasePageModel
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

            pageModel.OnPageHandlerExecuting(pageHandlerExecutingContext);

            return pageHandlerExecutingContext;

        }
    }
}
