using Microsoft.Extensions.DependencyInjection;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Services;
using Piipan.Shared.Deidentification;

namespace Piipan.Participants.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterParticipantsServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRedactionService, RedactionService>();
            serviceCollection.AddTransient<IParticipantBulkInsertHandler, ParticipantBulkInsertHandler>();
            serviceCollection.AddTransient<IParticipantDao, ParticipantDao>();
            serviceCollection.AddTransient<IUploadDao, UploadDao>();
            serviceCollection.AddTransient<IStateService, StateService>();
            serviceCollection.AddTransient<IParticipantApi, ParticipantService>();
            serviceCollection.AddTransient<IParticipantPublishUploadMetric, ParticipantPublishUploadMetric>();
        }
    }
}
