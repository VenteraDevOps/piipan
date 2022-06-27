using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Etl.Func.BulkUpload.Startup))]

namespace Piipan.Etl.Func.BulkUpload
{
    public class Startup : FunctionsStartup
    {
        public const string DatabaseConnectionString = "DatabaseConnectionString";
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(s =>
            {
                return new AzurePgConnectionFactory<ParticipantsDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(DatabaseConnectionString)
                );
            });
            builder.Services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();

            builder.Services.AddTransient<IBlobClientStream, BlobClientStream>();

            builder.Services.RegisterParticipantsServices();
            builder.Services.RegisterKeyVaultClientServices();
        }
    }
}
