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
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Match.Func.ResolutionApi.Startup))]

namespace Piipan.Match.Func.ResolutionApi
{
    public class Startup : FunctionsStartup
    {
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IMatchRecordDao, MatchRecordDao>();
            builder.Services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            builder.Services.AddTransient<IMatchResAggregator, MatchResAggregator>();
            builder.Services.AddTransient<IValidator<AddEventRequest>, AddEventRequestValidator>();
            builder.Services.AddTransient<IStreamParser<AddEventRequest>, AddEventRequestParser>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new AzurePgConnectionFactory<CollaborationDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)
                );
            });
        }
    }
}
