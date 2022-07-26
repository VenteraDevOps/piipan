using Microsoft.Extensions.DependencyInjection;
using Piipan.Match.Api;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Services;

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
            serviceCollection.AddTransient<IMatchEventService, MatchEventService>();

        }
    }
}
