using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Piipan.Components.Validation.ValidationConstants;

namespace Piipan.QueryTool.Tests
{
    public class MatchPageTests : BasePageTest
    {
        private const string ValidMatchId = "m123456";

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var pageModel = SetupMatchModel();

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
            var caseid = ValidMatchId;
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
                    MatchId = ValidMatchId
                }
            };
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatch(ValidMatchId))
                .ReturnsAsync(apiReturnValue);
            mockMatchApi
                .Setup(n => n.GetMatch(It.IsNotIn(ValidMatchId)))
                .ReturnsAsync((MatchResApiResponse)null);
            return mockMatchApi;
        }

        [Theory]
        [InlineData("123", $"{ValidationFieldPlaceholder} must be 7 characters")]
        [InlineData("12345678", $"{ValidationFieldPlaceholder} must be 7 characters")]
        [InlineData("m1$2345", $"{ValidationFieldPlaceholder} contains invalid characters")]
        [InlineData("", $"{ValidationFieldPlaceholder} is required")]
        public async Task TestInvalidMatchId_Post(string matchId, string expectedError)
        {
            // arrange
            var pageModel = SetupMatchModel();

            pageModel.Query = new MatchSearchRequest
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

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = ValidMatchId
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

            pageModel.BindModel(new MatchSearchRequest
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
                .Setup(n => n.GetMatch(ValidMatchId))
                .ThrowsAsync(new System.Exception("Test Error"));
            var pageModel = SetupMatchModel(mockMatchApi);

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = ValidMatchId
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost());

            // assert
            Assert.Equal(new List<ServerError> { new("", "There was an error running your search. Please try again.") }, pageModel.RequestErrors);
        }

        [Theory]
        [InlineData(nameof(MatchModel.OnGet), null, null, false)]
        [InlineData(nameof(MatchModel.OnGet), "IA", null, false)]
        [InlineData(nameof(MatchModel.OnGet), null, "Worker", false)]
        [InlineData(nameof(MatchModel.OnGet), "IA", "Worker", true)]
        [InlineData(nameof(MatchModel.OnPost), null, null, false)]
        [InlineData(nameof(MatchModel.OnPost), "IA", null, false)]
        [InlineData(nameof(MatchModel.OnPost), null, "Worker", false)]
        [InlineData(nameof(MatchModel.OnPost), "IA", "Worker", true)]
        public void IsAccessibleWhenRolesExist(string method, string role, string location, bool isAuthorized)
        {
            var mockClaimsProvider = claimsProviderMock(state: location, role: role);

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
            var pageModel = SetupMatchModel(mockClaimsProvider: claimsProvider);

            return base.GetPageHandlerExecutingContext(pageModel, methodName);
        }

        private MatchModel SetupMatchModel(Mock<IMatchResolutionApi> mockMatchApi = null, IClaimsProvider mockClaimsProvider = null)
        {
            // arrange
            mockClaimsProvider ??= claimsProviderMock();
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockMatchApi.Object
            );
            return pageModel;
        }
    }
}