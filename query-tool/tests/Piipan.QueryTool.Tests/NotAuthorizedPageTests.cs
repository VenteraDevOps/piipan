using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class NotAuthorizedPageTests : BasePageTest
    {
        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void TestMessageOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // act
            pageModel.OnGet();
            // assert
            Assert.Equal("You do not have a sufficient role or location to access this page", pageModel.Message);
        }

        /// <summary>
        /// The not authorized page should be accessible no matter your role
        /// </summary>
        [Theory]
        [InlineData(nameof(NotAuthorizedModel.OnGet), null, null, true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), "IA", null, true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), null, "Worker", true)]
        [InlineData(nameof(NotAuthorizedModel.OnGet), "IA", "Worker", true)]
        public async Task IsAccessibleWhenRolesExist(string method, string role, string location, bool isAuthorized)
        {
            var mockServiceProvider = serviceProviderMock(location: location, role: role);

            var pageHandlerExecutingContext = await GetPageHandlerExecutingContext(mockServiceProvider, method);

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

        private async Task<PageHandlerExecutingContext> GetPageHandlerExecutingContext(IServiceProvider serviceProvider, string methodName)
        {
            var pageModel = new NotAuthorizedModel(serviceProvider);

            return await base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}