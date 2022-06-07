using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class ErrorPageTests : BasePageTest
    {
        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ErrorModel(new NullLogger<ErrorModel>(), mockServiceProvider);

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void TestMessageOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new ErrorModel(new NullLogger<ErrorModel>(), mockServiceProvider);

            // act
            string message = "test message";
            pageModel.OnGet(message);
            // assert
            Assert.Equal(message, pageModel.Message);
        }

        /// <summary>
        /// The error page should be accessible no matter your role
        /// </summary>
        [Theory]
        [InlineData(nameof(ErrorModel.OnGet), null, null, true)]
        [InlineData(nameof(ErrorModel.OnGet), "IA", null, true)]
        [InlineData(nameof(ErrorModel.OnGet), null, "Worker", true)]
        [InlineData(nameof(ErrorModel.OnGet), "IA", "Worker", true)]
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
            var pageModel = new ErrorModel(new NullLogger<ErrorModel>(), serviceProvider);

            return await base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}