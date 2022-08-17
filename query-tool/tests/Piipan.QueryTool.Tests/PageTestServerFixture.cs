using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.QueryTool.Tests
{
    /// <summary>
    /// A test fixture for rendering CSHTML pages
    /// </summary>
    public class PageTestServerFixture : WebApplicationFactory<PageTestStartup>
    {
        public TService GetRequiredService<TService>()
        {
            if (Server == null)
            {
                CreateDefaultClient();
            }

            return Server.Host.Services.GetRequiredService<TService>();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var hostBuilder = new WebHostBuilder();
            // Solution fixing the problem:
            // https://github.com/dotnet/aspnetcore/issues/17655#issuecomment-581418168
            hostBuilder.ConfigureAppConfiguration((context, b) =>
            {
                context.HostingEnvironment.ApplicationName = typeof(ViewRenderer).Assembly.GetName().Name;
            });
            return hostBuilder.UseStartup<PageTestStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot("src/Piipan.QueryTool/Pages");
        }
    }
}
