using System;
using System.Data.Common;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Piipan.Shared.Database;
using Piipan.States.Core.DataAccessObjects;

[assembly: FunctionsStartup(typeof(Piipan.States.Func.Api.Startup))]

namespace Piipan.States.Func.Api
{
    public class Startup : FunctionsStartup
    {
        public const string CollaborationDatabaseConnectionString = "CollaborationDatabaseConnectionString";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddTransient<IStateInfoDao, StateInfoDao>();

            builder.Services.AddSingleton<DbProviderFactory>(NpgsqlFactory.Instance);
            builder.Services.AddTransient<IDbConnectionFactory<StateInfoDb>>(s =>
            {
                return new AzurePgConnectionFactory<StateInfoDb>(
                    new AzureServiceTokenProvider(),
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(CollaborationDatabaseConnectionString)
                );
            });
        }
    }
}
