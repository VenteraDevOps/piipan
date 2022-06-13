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
    }
}