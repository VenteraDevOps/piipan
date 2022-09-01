using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Dashboard.Pages;
using Piipan.Shared.Claims;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class IndexPageTests : BasePageTest
    {
        [Fact]
        public void BeforeOnGet_EmailIsCorrect()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), serviceProviderMock());
            pageModel.PageContext.HttpContext = contextMock();

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public void AfterOnGet_EmailIsCorrect()
        {
            // arrange
            var pageModel = new IndexModel(new NullLogger<IndexModel>(), serviceProviderMock());
            pageModel.PageContext.HttpContext = contextMock();

            // act
            pageModel.OnGet();

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        private IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        public static HttpContext contextMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context.Object;
        }
    }
}
