using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public void TestNotAuthorizedStatusCode()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // act
            var pageResult = pageModel.OnGet();
            // assert
            Assert.IsType<PageResult>(pageResult);
            Assert.Equal(403, (pageResult as PageResult).StatusCode);
        }
    }
}