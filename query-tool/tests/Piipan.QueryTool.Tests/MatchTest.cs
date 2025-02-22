using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Pages;
using Piipan.QueryTool.Tests.Extensions;
using Piipan.Shared.Http;
using Xunit;
using static Piipan.Components.Validation.ValidationConstants;

namespace Piipan.QueryTool.Tests
{
    public class MatchPageTests : BasePageTest
    {
        private const string ValidMatchId = "m123456";
        private const string PageName = "/Pages/Match.cshtml";
        private const string MatchDetailTitle = "NAC Match Detail";
        private const string MatchSearchTitle = "NAC Match Search";
        private const string MatchDetailWrapperComponentName = "Piipan.QueryTool.Client.Components.MatchDetail.MatchDetailWrapper";
        private const string MatchSearchFormComponentName = "Piipan.QueryTool.Client.Components.MatchForm";

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
        public async Task TestAfterOnGet()
        {
            // arrange
            var pageModel = SetupMatchModel();
            var renderer = SetupRenderingApi();

            // act
            await pageModel.OnGet(null);
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal(MatchSearchTitle, page.ViewContext.ViewData["Title"]);
            Assert.Contains(MatchSearchFormComponentName, output);
            Assert.Null(page.ViewContext.ViewData["SelectedPage"]);
        }

        [Fact]
        public async Task TestShortMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnGet("m12345");

            // assert
            Assert.False(pageModel.AppData.IsAuthorized);
        }

        [Fact]
        public async Task TestNoMatchRole_MatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel(role: "Other");
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnGet("m123456");

            // assert
            Assert.False(pageModel.AppData.IsAuthorized);
        }

        [Fact]
        public async Task TestLongMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnGet("m123456789");

            // assert
            Assert.False(pageModel.AppData.IsAuthorized);
        }


        [Fact]
        public async Task TestInvalidCharactersOnMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnGet("m1$23^45");

            // assert
            Assert.False(pageModel.AppData.IsAuthorized);
        }

        [Fact]
        public async Task TestUnauthorized_Post()
        {
            // arrange
            var pageModel = SetupMatchModel(role: "Other");
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            await pageModel.OnPost(caseid);

            // assert
            Assert.False(pageModel.AppData.IsAuthorized);
        }

        [Fact]
        public async Task TestValidMatch_Post()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.True(pageModel.MatchDetailData.SaveSuccess);
            Assert.Empty(pageModel.RequestErrors);

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
        public async Task TestValidMatch_PostFails_WhenInvalidRole()
        {
            // arrange
            var pageModel = SetupMatchModel(role: "Oversight"); // give view only access
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.False(pageModel.MatchDetailData.SaveSuccess);
            Assert.NotEmpty(pageModel.RequestErrors);

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
        public async Task TestInvalidMatch_Post()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                InitialActionAt = DateTime.Now,
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.False(pageModel.MatchDetailData.SaveSuccess);
            Assert.NotEmpty(pageModel.RequestErrors);

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
        public async Task TestValidMatch_PostException()
        {
            // arrange
            var matchResApiMock = SetupMatchResolutionApi();
            matchResApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>())).Throws(new Exception());
            var pageModel = SetupMatchModel(matchResApiMock);
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.False(pageModel.MatchDetailData.SaveSuccess);
            Assert.NotEmpty(pageModel.RequestErrors);

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
        public async Task TestValidMatch_PostEmptyResponseError()
        {
            // arrange
            var matchResApiMock = SetupMatchResolutionApi();
            matchResApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>())).ReturnsAsync((null, "{}"));
            var pageModel = SetupMatchModel(matchResApiMock);
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.False(pageModel.MatchDetailData.SaveSuccess);
            Assert.NotEmpty(pageModel.RequestErrors);

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
        public async Task TestValidMatch_PostValidationError()
        {
            // arrange
            var matchResApiMock = SetupMatchResolutionApi();
            var mockError = new ApiErrorResponse() { Errors = new List<ApiHttpError> { new ApiHttpError { Detail = "Some field validation went wrong" } } };
            matchResApiMock.Setup(n => n.AddMatchResEvent(It.IsAny<string>(), It.IsAny<AddEventRequest>(), It.IsAny<string>())).ReturnsAsync((null, JsonConvert.SerializeObject(mockError)));
            var pageModel = SetupMatchModel(matchResApiMock);
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = ValidMatchId;

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = null
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));
            pageModel.DispositionData = new DispositionModel
            {
                VulnerableIndividual = true
            };
            pageModel.BindModel(pageModel.DispositionData, nameof(MatchModel.DispositionData));

            var result = await pageModel.OnPost(caseid);

            Assert.False(pageModel.MatchDetailData.SaveSuccess);
            Assert.NotEmpty(pageModel.RequestErrors);

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
        public async Task TestValidMatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();
            var renderer = SetupRenderingApi();


            // act
            var caseid = ValidMatchId;
            var result = await pageModel.OnGet(caseid);
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert the match was set to the value returned by the match resolution API
            Assert.IsType<PageResult>(result);
            Assert.Equal(caseid, pageModel.Match.Data.MatchId);
            Assert.Equal("ea", pageModel.Match.Data.Initiator);
            Assert.Equal(MatchRecordStatus.Open, pageModel.Match.Data.Status);
            Assert.Empty(pageModel.Match.Data.Dispositions);
            Assert.Empty(pageModel.Match.Data.Participants);
            Assert.Equal(new string[] { "ea", "eb" }, pageModel.Match.Data.States);
            Assert.Equal(new string[] { "Worker" }, pageModel.RequiredRolesToEdit);
            Assert.Equal(MatchDetailReferralPage.Other, pageModel.MatchDetailData.ReferralPage);
            Assert.Equal(MatchDetailTitle, page.ViewContext.ViewData["Title"]);
            Assert.Contains(MatchDetailWrapperComponentName, output);
            Assert.Null(page.ViewContext.ViewData["SelectedPage"]);
        }

        [Fact]
        public async Task MatchSearchReferrer_MatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            var mockRequest = new Mock<HttpRequest>();
            pageModel.PageContext.HttpContext = contextMock(mockRequest);
            var headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
            headers.Add("Referer", "https://tts.test/match"); // we're coming from the match search screen
            mockRequest.Setup(m => m.Headers).Returns(new HeaderDictionary(headers));
            var renderer = SetupRenderingApi();

            // act
            var caseid = ValidMatchId;
            var result = await pageModel.OnGet(caseid);
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert the match was set to the value returned by the match resolution API
            Assert.Equal(MatchDetailReferralPage.MatchSearch, pageModel.MatchDetailData.ReferralPage);
            Assert.Equal(MatchDetailTitle, page.ViewContext.ViewData["Title"]);
            Assert.Contains(MatchDetailWrapperComponentName, output);
            Assert.Equal("match", page.ViewContext.ViewData["SelectedPage"]);
        }

        [Fact]
        public async Task QuerySearchReferrer_MatchId_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            var mockRequest = new Mock<HttpRequest>();
            pageModel.PageContext.HttpContext = contextMock(mockRequest);
            var headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
            headers.Add("Referer", "https://tts.test/"); // we're coming from the main participant search screen
            mockRequest.Setup(m => m.Headers).Returns(new HeaderDictionary(headers));
            var renderer = SetupRenderingApi();

            // act
            var caseid = ValidMatchId;
            var result = await pageModel.OnGet(caseid);
            var (page, output) = await renderer.RenderPage(PageName, pageModel);

            // assert the match was set to the value returned by the match resolution API
            Assert.Equal(MatchDetailReferralPage.Query, pageModel.MatchDetailData.ReferralPage);
            Assert.Equal(MatchDetailTitle, page.ViewContext.ViewData["Title"]);
            Assert.Contains(MatchDetailWrapperComponentName, output);
            Assert.Equal("", page.ViewContext.ViewData["SelectedPage"]);
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
            Assert.False(pageModel.AppData.IsAuthorized);
        }

        [Fact]
        public void TestInitializeUserState()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.Match = new MatchResApiResponse();

            pageModel.PageContext.HttpContext = contextMock();

            Assert.True(string.IsNullOrEmpty(pageModel.UserState));

            // act
            pageModel.InitializeUserState();

            // assert
            Assert.Equal("Echo Alpha", pageModel.UserState);
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
                .Setup(n => n.GetMatch(ValidMatchId, "EA"))
                .ReturnsAsync(apiReturnValue);
            mockMatchApi
                .Setup(n => n.GetMatch(It.IsNotIn(ValidMatchId), "EA"))
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
            Assert.IsType<PageResult>(await pageModel.OnPost(null));

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
            Assert.IsType<PageResult>(await pageModel.OnPost(null));

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
            Assert.IsType<PageResult>(await pageModel.OnPost(null));

            // assert
            Assert.Empty(pageModel.RequestErrors);
            Assert.Empty(pageModel.AvailableMatches);
        }

        [Fact]
        public async Task TestSearchError_Post()
        {
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatch(ValidMatchId, "EA"))
                .ThrowsAsync(new System.Exception("Test Error"));
            var pageModel = SetupMatchModel(mockMatchApi);

            pageModel.Query = new MatchSearchRequest
            {
                MatchId = ValidMatchId
            };
            pageModel.BindModel(pageModel.Query, nameof(MatchModel.Query));

            pageModel.PageContext.HttpContext = contextMock();

            // act
            Assert.IsType<PageResult>(await pageModel.OnPost(null));

            // assert
            Assert.Equal(new List<ServerError> { new("", "There was an error running your search. Please try again.") }, pageModel.RequestErrors);
        }

        private MatchModel SetupMatchModel(Mock<IMatchResolutionApi> mockMatchApi = null, IServiceProvider mockServiceProvider = null, string role = "Worker")
        {
            // arrange
            mockServiceProvider ??= serviceProviderMock(role: role);
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockMatchApi.Object,
                mockServiceProvider
            );
            var results = new List<States.Api.Models.StateInfoResponseData>();
            results.Add(new States.Api.Models.StateInfoResponseData { StateAbbreviation = "EA", State = "Echo Alpha" });
            results.Add(new States.Api.Models.StateInfoResponseData { StateAbbreviation = "EB", State = "Echo Bravo" });

            pageModel.StateInfo = new States.Api.Models.StatesInfoResponse();
            pageModel.StateInfo.Results = results;
            return pageModel;
        }
    }
}