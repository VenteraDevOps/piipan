using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Pages;
using Piipan.QueryTool.Tests.Extensions;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using System.Collections.Generic;
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
        public async Task TestShortMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m12345")).PageName;

            // assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public async Task TestLongMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m123456789")).PageName;

            // assert
            Assert.Equal("Error", result);
        }


        [Fact]
        public async Task TestInvalidCharactersOnMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(await pageModel.OnGet("m1$23^45")).PageName;

            // assert
            Assert.Equal("Error", result);
        }

        [Fact]
        public async Task TestValidMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
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
        public async Task TestValidMatchIdThatDoesNotExist_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = await pageModel.OnGet("m123457");

            // assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Error", (result as RedirectToPageResult).PageName);
        }

        private Mock<IMatchResolutionApi> SetupMatchResolutionApi()
        {
            var apiReturnValue = new MatchResApiResponse
            {
                Data = new MatchResRecord
                {
                    States = new string[] { "ea", "eb" },
                    Initiator = "ea",
                    MatchId = "m123456"
                }
            };
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatch("m123456"))
                .ReturnsAsync(apiReturnValue);
            mockMatchApi
                .Setup(n => n.GetMatch(It.IsNotIn("m123456")))
                .ReturnsAsync((MatchResApiResponse)null);
            return mockMatchApi;
        }

        [Theory]
        [InlineData("123", "@@@ must be 7 characters")]
        [InlineData("12345678", "@@@ must be 7 characters")]
        [InlineData("m1$2345", "@@@ contains invalid characters")]
        [InlineData("", "@@@ is required")]
        public async Task TestInvalidMatchId_Post(string matchId, string expectedError)
        {
            // arrange
            var pageModel = SetupMatchModel();

            pageModel.Query = new Client.Models.MatchSearchRequest
            {
                MatchId = matchId
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost());

            // assert
            Assert.Equal(new List<ServerError> { new("Query.MatchId", expectedError) },
                pageModel.RequestErrors);
        }

        [Fact]
        public async Task TestFoundMatchId_Post()
        {
            // arrange
            var pageModel = SetupMatchModel();

            pageModel.Query = new Client.Models.MatchSearchRequest
            {
                MatchId = "m123456"
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost());

            // assert
            Assert.Empty(pageModel.RequestErrors);
            Assert.Single(pageModel.AvailableMatches);
        }

        [Fact]
        public async Task TestNotFoundMatchId_Post()
        {
            var pageModel = SetupMatchModel();

            pageModel.BindModel(new Client.Models.MatchSearchRequest
            {
                MatchId = "m333333"
            }, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost());

            // assert
            Assert.Empty(pageModel.RequestErrors);
            Assert.Empty(pageModel.AvailableMatches);
        }

        [Fact]
        public async Task TestSearchError_Post()
        {
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatch("m123456"))
                .ThrowsAsync(new System.Exception("Test Error"));
            var pageModel = SetupMatchModel(mockMatchApi);

            pageModel.Query = new Client.Models.MatchSearchRequest
            {
                MatchId = "m123456"
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost());

            // assert
            Assert.Equal(new List<ServerError> { new("", "There was an error running your search. Please try again.") }, pageModel.RequestErrors);
        }

        private MatchModel SetupMatchModel(Mock<IMatchResolutionApi> mockMatchApi = null)
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            return pageModel;
        }
    }
}