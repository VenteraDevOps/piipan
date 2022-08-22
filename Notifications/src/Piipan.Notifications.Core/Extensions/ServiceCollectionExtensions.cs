using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Core.Services;

namespace Piipan.Match.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterNotificationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<INotificationPublish, NotificationPublish>();

        }
    }
}
