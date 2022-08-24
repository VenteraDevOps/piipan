using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Piipan.Notifications.Api;
using Piipan.Shared.Authentication;
using Piipan.Shared.Http;

namespace Piipan.Notifications.Client.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterStatesClientServices(this IServiceCollection serviceCollection, IHostEnvironment env)
        {
            serviceCollection.Configure<AzureTokenProviderOptions<NotificationClient>>(options =>
            {
                var appId = Environment.GetEnvironmentVariable("StatesApiAppId");
                options.ResourceUri = $"api://{appId}";
            });

            // Add token credential services if it hasn't already been added
            if (env.IsDevelopment())
            {
                serviceCollection.TryAddTransient<TokenCredential, AzureCliCredential>();
            }
            else
            {
                serviceCollection.TryAddTransient<TokenCredential, ManagedIdentityCredential>();
            }

            serviceCollection.AddHttpClient<NotificationClient>((c) =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("StatesApiUri"));
            });
            serviceCollection.AddTransient<ITokenProvider<NotificationClient>, AzureTokenProvider<NotificationClient>>();
            serviceCollection.AddTransient<IAuthorizedApiClient<NotificationClient>, AuthorizedJsonApiClient<NotificationClient>>();
            serviceCollection.AddTransient<INotificationApi, NotificationClient>();
        }
    }
}
