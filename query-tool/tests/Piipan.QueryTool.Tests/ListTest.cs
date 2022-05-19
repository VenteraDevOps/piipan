﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class ListPageTests : BasePageTest
    {
        private string[] matchIds = new string[] { "m123456", "m654321" };

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockClaimsProvider,
                mockMatchApi
            );

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async Task Test_Get()
        {
            // arrange
            var pageModel = SetupMatchModel();
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = await pageModel.OnGet();

            // assert the match was set to the value returned by the match resolution API
            Assert.IsType<PageResult>(result);
            var list = pageModel.AvailableMatches.Data.ToList();
            Assert.Equal(2, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.Equal(matchIds[i], list[i].MatchId);
                Assert.Equal("ea", list[i].Initiator);
                Assert.Equal(MatchRecordStatus.Open, list[i].Status);
                Assert.Empty(list[i].Dispositions);
                Assert.Empty(list[i].Participants);
                Assert.Equal(new string[] { "ea", "eb" }, list[i].States);
            }
        }

        private Mock<IMatchResolutionApi> SetupMatchResolutionApi()
        {
            var matchResRecords = matchIds.Select(n => new MatchResRecord
            {
                States = new string[] { "ea", "eb" },
                Initiator = "ea",
                MatchId = n
            });
            var apiReturnValue = new MatchResListApiResponse
            {
                Data = matchResRecords
            };
            var mockMatchApi = new Mock<IMatchResolutionApi>();
            mockMatchApi
                .Setup(n => n.GetMatches())
                .ReturnsAsync(apiReturnValue);
            return mockMatchApi;
        }

        private ListModel SetupMatchModel(Mock<IMatchResolutionApi> mockMatchApi = null)
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock();
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockClaimsProvider,
                mockMatchApi.Object
            );
            return pageModel;
        }

        [Theory]
        [InlineData(nameof(ListModel.OnGet), null, null, false)]
        [InlineData(nameof(ListModel.OnGet), "IA", null, false)]
        [InlineData(nameof(ListModel.OnGet), null, "Worker", false)]
        [InlineData(nameof(ListModel.OnGet), "IA", "Worker", true)]
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
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                claimsProvider,
                mockMatchApi
            );

            return base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}