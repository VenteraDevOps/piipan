using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Pages;
using Piipan.QueryTool.Tests.Builders;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class MatchPageTests
    {
        public static IClaimsProvider claimsProviderMock(string email)
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

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async Task TestShortMatchId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m12345")).PageName;

            // assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public async Task TestLongMatchId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m123456789")).PageName;

            // assert
            Assert.Equal("Error", result);
        }


        [Fact]
        public async Task TestInvalidCharactersOnMatchId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m1$23^45")).PageName;

            // assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public async Task TestValidMatchId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();

            var mockMatchApi = SetupMatchResolutionApi();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = "m123456";
            var result = await pageModel.OnGet(caseid);

            // assert the match was set to the value returned by the match resolution API
            Assert.IsType<PageResult>(result);
            Assert.Equal(caseid, pageModel.Match.Data.MatchId);
            Assert.Equal("ea", pageModel.Match.Data.Initiator);
            Assert.Equal(MatchRecordStatus.Open, pageModel.Match.Data.Status);
            Assert.Empty(pageModel.Match.Data.Dispositions);
            Assert.Empty(pageModel.Match.Data.Participants);
            Assert.Equal(new string[] { "ea", "eb" }, pageModel.Match.Data.States);
        }

        [Fact]
        public async Task TestValidMatchIdThatDoesNotExist()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = SetupMatchResolutionApi();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = await pageModel.OnGet("m123457");

            // assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Error", (result as RedirectToPageResult).PageName);
        }

        private Mock<IMatchResolutionApi> SetupMatchResolutionApi()
        {
            var apiReturnValue = MatchResApiResponseBuilder.Build();
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatch("m123456"))
                .ReturnsAsync(apiReturnValue);
            mockMatchApi
                .Setup(n => n.GetMatch(It.IsNotIn("m123456")))
                .ReturnsAsync((MatchResApiResponse)null);
            return mockMatchApi;
        }
    }
}