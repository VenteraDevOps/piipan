using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Npgsql;

namespace Piipan.Shared.Database
{
    /// <summary>
    /// A factory for creating cloud-specific connections to Azure-hosted
    /// databases when using a managed identity for authentication.
    /// </summary>
    public class AzurePgConnectionFactory<T> : IDbConnectionFactory<T>
    {
        // Environment variables (and placeholder) established
        // during initial function app provisioning in IaC
        public const string CloudName = "CloudName";
        public const string PasswordPlaceholder = "{password}";
        public const string GovernmentCloud = "AzureUSGovernment";

        // Resource ids for open source software databases in the public and
        // US government clouds. Set the desired active cloud, then see:
        // `az cloud show --query endpoints.ossrdbmsResourceId`
        public const string CommercialId = "https://ossrdbms-aad.database.windows.net";
        public const string GovermentId = "https://ossrdbms-aad.database.usgovcloudapi.net";

        private readonly AzureServiceTokenProvider _tokenProvider;
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;

        /// <summary>
        /// Create a new instance of AzurePgConnectionFactory
        /// </summary>
        /// <param name="tokenProvider">An instance of AzureServiceTokenProvider</param>
        /// <param name="dbProviderFactory">An instance of DbProviderFactory</param>
        /// <param name="connectionString">The connection string used for building connections</param>
        public AzurePgConnectionFactory(
            AzureServiceTokenProvider tokenProvider,
            DbProviderFactory dbProviderFactory,
            string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string must be set to a value.");
            }

            _tokenProvider = tokenProvider;
            _dbProviderFactory = dbProviderFactory;
            _connectionString = connectionString;
        }

        /// <summary>
        /// Build and return a database connection
        /// </summary>
        /// <param name="database">(Optional) The database to connect to. Overrides any existing database value.</param>
        public async Task<IDbConnection> Build(string database = null)
        {
            var resourceId = CommercialId;
            var cn = Environment.GetEnvironmentVariable(CloudName);
            if (cn == GovernmentCloud)
            {
                resourceId = GovermentId;
            }

            var builder = new NpgsqlConnectionStringBuilder(_connectionString);

            if (builder.Password == PasswordPlaceholder)
            {
                var token = await _tokenProvider.GetAccessTokenAsync(resourceId);
                builder.Password = token;
            }

            if (!String.IsNullOrEmpty(database))
            {
                builder.Database = database;
            }

            var connection = _dbProviderFactory.CreateConnection();

            connection.ConnectionString = builder.ConnectionString;

            return connection;
        }
    }
}
