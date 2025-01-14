﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Language.Flow;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using Piipan.Shared.Roles;
using Piipan.States.Api;
using Piipan.States.Api.Models;

namespace Piipan.QueryTool.Tests
{
    public class BasePageTest
    {
        public static IServiceProvider serviceProviderMock(string email = "noreply@tts.test",
            string location = "EA", string role = "Worker", string[] states = null,
            Action<ISetup<IStatesApi, Task<StatesInfoResponse>>> statesInfoResponseOverride = null)
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

            var roleProviderMock = new Mock<IRolesProvider>();
            roleProviderMock.Setup(c => c.GetMatchEditRoles()).Returns(new string[] { "Worker" });
            roleProviderMock.Setup(c => c.GetMatchViewRoles()).Returns(new string[] { "Worker", "Oversight" });

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

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(claimsProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(locationProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(roleProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IStatesApi))).Returns(statesApiMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IMemoryCache))).Returns(new MemoryCache(new MemoryCacheOptions()));

            var inMemorySettings = new Dictionary<string, string> {
                {"HelpDeskEmail", "test@usda.example"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(configuration);

            return serviceProviderMock.Object;
        }

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

        /// <summary>
        /// Sets up the server fixture, view engines, razor page activator, etc.
        /// This will be used to render CSHTML pages and test them.
        /// </summary>
        /// <returns></returns>
        protected ViewRenderer SetupRenderingApi()
        {
            var server = new PageTestServerFixture();
            var serviceProvider = server.GetRequiredService<IServiceProvider>();
            var viewEngine = server.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = server.GetRequiredService<ITempDataProvider>();
            var razorPageActivator = server.GetRequiredService<IRazorPageActivator>();

            var viewRenderer = new ViewRenderer(viewEngine, tempDataProvider, serviceProvider, razorPageActivator);
            return viewRenderer;
        }
    }
}
