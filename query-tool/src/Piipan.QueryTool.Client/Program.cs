using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Components.Modals;
using Piipan.Components.Routing;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddModalManager();
builder.Services.AddPiipanNavigationManager();
await builder.Build().RunAsync();
