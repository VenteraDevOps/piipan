using System;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Razor;
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
using Piipan.Notification.Core.Extensions;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.States.Core.DataAccessObjects;

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
            builder.Services.AddTransient<IDbConnectionFactory<StateInfoDb>>(s =>
            {
                return new AzurePgConnectionFactory<StateInfoDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)
                );
            });
            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            builder.Services.AddSingleton<DiagnosticListener>(listener);
            builder.Services.AddSingleton<DiagnosticSource>(listener);
            builder.Services.AddMvc()
                    .AddApplicationPart(typeof(Piipan.Notification.Common.ViewRenderService).GetTypeInfo().Assembly)
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization();

            builder.Services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Templates/{0}" + RazorViewEngine.ViewExtension);
            });

            builder.Services.RegisterNotificationServices();
            builder.Services.RegisterMatchServices();
            builder.Services.RegisterParticipantsServices();
            builder.Services.RegisterKeyVaultClientServices();


        }
    }
}