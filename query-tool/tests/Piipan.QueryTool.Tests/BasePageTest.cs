namespace Piipan.QueryTool.Tests
{
    public class BasePageTest
    {
        public static IServiceProvider serviceProviderMock(string email = "noreply@tts.test",
            string location = "IA", string role = "Worker", string[] states = null,
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

            var statesApiMock = new Mock<IStatesApi>();

            // declare it as object so MemoryCache setup works.
            StatesInfoResponse defaultStateInfoResponse = new StatesInfoResponse
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
            serviceProviderMock.Setup(c => c.GetService(typeof(IStatesApi))).Returns(statesApiMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IMemoryCache))).Returns(new MemoryCache(new MemoryCacheOptions()));
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
