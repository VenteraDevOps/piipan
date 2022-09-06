using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Piipan.Dashboard.Client.Models;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddModalManager();
        builder.Services.AddPiipanNavigationManager();
        builder.Services.AddSingleton<AppData>();
        await builder.Build().RunAsync();
    }
}