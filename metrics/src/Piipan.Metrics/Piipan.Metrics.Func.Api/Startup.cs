using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Extensions;
using Piipan.Shared.Database;

[assembly: FunctionsStartup(typeof(Piipan.Metrics.Func.Api.Startup))]

namespace Piipan.Metrics.Func.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        public const string DatabaseConnectionString = "DatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddTransient<IDbConnectionFactory<MetricsDb>>(s =>
            {
                return new AzurePgConnectionFactory<MetricsDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(DatabaseConnectionString)
                );
            });

            builder.Services.RegisterCoreServices();
        }
    }
}