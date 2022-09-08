using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.QueryTool.Pages;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class NotAuthorizedPageTests : BasePageTest
    {
        [Fact]
        public async Task TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // Avoid rendering the Blazor component... it doesn't work through unit testing. By setting to WebAssembly it won't render.
            pageModel.RenderMode = Microsoft.AspNetCore.Mvc.Rendering.RenderMode.WebAssembly;

            var renderer = SetupRenderingApi();

            // act
            var (page, output) = await renderer.RenderPage("/Pages/NotAuthorized.cshtml", pageModel);


            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("Not Authorized", page.ViewContext.ViewData["Title"]);
            Assert.Contains("Piipan.QueryTool.Client.Components.NotAuthorized", output);
        }

        [Fact]
        public void TestNotAuthorizedStatusCode()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var pageModel = new NotAuthorizedModel(mockServiceProvider);

            // act
            var pageResult = pageModel.OnGet();
            // assert
            Assert.IsType<PageResult>(pageResult);
            Assert.Equal(403, (pageResult as PageResult).StatusCode);
        }
    }
}