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
    }
}