using System;
using System.Data.Common;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Extensions;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Validators;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.Api.Startup))]

namespace Piipan.Match.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public const string DatabaseConnectionString = "DatabaseConnectionString";
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            builder.Services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            builder.Services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            builder.Services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            builder.Services.AddTransient<IMatchResAggregator, MatchResAggregator>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(s =>
            {
                return new AzurePgConnectionFactory<ParticipantsDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(DatabaseConnectionString)
                );
            });
            builder.Services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new AzurePgConnectionFactory<CollaborationDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)
                );
            });

            builder.Services.RegisterMatchServices();
            builder.Services.RegisterParticipantsServices();
        }
    }
}
