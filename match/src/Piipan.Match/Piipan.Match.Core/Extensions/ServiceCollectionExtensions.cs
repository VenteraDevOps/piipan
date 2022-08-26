using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Api;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Services;
using Piipan.Notifications.Services;
using Piipan.States.Core.DataAccessObjects;

namespace Piipan.Match.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMatchServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IMatchApi, MatchService>();
            serviceCollection.AddTransient<IMatchIdService, MatchIdService>();
            serviceCollection.AddTransient<IActiveMatchRecordBuilder, ActiveMatchRecordBuilder>();
            serviceCollection.AddTransient<IMatchRecordDao, MatchRecordDao>();
            serviceCollection.AddTransient<IMatchRecordApi, MatchRecordService>();
            serviceCollection.AddTransient<IParticipantPublishSearchMetric, ParticipantPublishSearchMetric>();
            serviceCollection.AddTransient<IParticipantPublishMatchMetric, ParticipantPublishMatchMetric>();
            serviceCollection.AddTransient<IMatchEventService, MatchEventService>();
            serviceCollection.AddTransient<IStateInfoDao, StateInfoDao>();
            serviceCollection.AddTransient<INotificationService, NotificationService>();
            serviceCollection.AddTransient<INotificationPublish, NotificationPublish>();

        }
    }
}
