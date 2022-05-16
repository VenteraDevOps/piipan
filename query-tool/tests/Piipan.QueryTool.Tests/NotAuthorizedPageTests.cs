using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class NotAuthorizedPageTests : BasePageTest
    {
        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock();
            var pageModel = new NotAuthorizedModel(mockClaimsProvider);

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void TestMessageOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock();
            var pageModel = new NotAuthorizedModel(mockClaimsProvider);

            // act
            pageModel.OnGet();
            // assert
            Assert.Equal("You do not have sufficient NAC roles or a NAC Location associated with your account", pageModel.Message);
        }

        /// <summary>
        /// The not authorized page should be accessible no matter your role
        /// </summary>
        [Theory]
        [InlineData(nameof(NotAuthorizedModel.OnGet), null, null, true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), "IA", null, true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), null, "Worker", true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), "IA", "Worker", true)]
        public void IsAccessibleWhenRolesExist(string method, string role, string location, bool isAuthorized)
        {
            var mockClaimsProvider = claimsProviderMock(state: location, nacRole: role);

            var pageHandlerExecutingContext = GetPageHandlerExecutingContext(mockClaimsProvider, method);

            if (!isAuthorized)
            {
                Assert.Equal(403, pageHandlerExecutingContext.HttpContext.Response.StatusCode);
                Assert.IsType<RedirectToPageResult>(pageHandlerExecutingContext.Result);
            }
            else
            {
                Assert.Equal(200, pageHandlerExecutingContext.HttpContext.Response.StatusCode);
            }
        }

        private PageHandlerExecutingContext GetPageHandlerExecutingContext(IClaimsProvider claimsProvider, string methodName)
        {
            var pageModel = new NotAuthorizedModel(claimsProvider);

            return base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}