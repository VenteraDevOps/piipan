using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Pages;
using Piipan.Shared.Claims;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class ListPageTests
    {
        private string[] matchIds = new string[] { "m123456", "m654321" };
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
                .Setup(n => n.GetMatchesList())
                .ReturnsAsync(apiReturnValue);
            return mockMatchApi;
        }

        private ListModel SetupMatchModel(Mock<IMatchResolutionApi> mockMatchApi = null)
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            mockMatchApi ??= SetupMatchResolutionApi();
            var pageModel = new ListModel(
                new NullLogger<ListModel>(),
                mockClaimsProvider,
                mockMatchApi.Object
            );
            return pageModel;
        }
    }
}