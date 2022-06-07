using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Pages;
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
            var mockServiceProvider = serviceProviderMock();
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockMatchApi,
                mockServiceProvider
            );

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async Task Test_Get_Accessible()
        {
            // arrange
            var pageModel = SetupMatchModel("National", new string[] { "*" });
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

        [Fact]
        public async Task Test_Get_Unauthorized_Location()
        {
            // arrange
            var pageModel = SetupMatchModel("IA");
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = await pageModel.OnGet();

            // assert
            Assert.IsType<RedirectToPageResult>(result);
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

        private ListModel SetupMatchModel(string location, string[] states = null, Mock<IMatchResolutionApi> mockMatchApi = null)
        {
            // arrange
            var mockServiceProvider = serviceProviderMock(location: location, states: states);
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockMatchApi.Object,
                mockServiceProvider
            );
            return pageModel;
        }

        [Theory]
        [InlineData(nameof(ListModel.OnGet), null, null, false)]
        [InlineData(nameof(ListModel.OnGet), "IA", null, false)]
        [InlineData(nameof(ListModel.OnGet), null, "Worker", false)]
        [InlineData(nameof(ListModel.OnGet), "IA", "Worker", true)]
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

        private async Task<PageHandlerExecutingContext> GetPageHandlerExecutingContext(IServiceProvider mockServiceProvider, string methodName)
        {
            var mockMatchApi = Mock.Of<IMatchResolutionApi>();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockMatchApi,
                mockServiceProvider
            );

            return await base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}