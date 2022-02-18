using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace Piipan.Shared.Database
{
    public class BasicPgConnectionFactory<T> : IDbConnectionFactory<T>
    {
        public readonly DbProviderFactory _dbProviderFactory;
        public readonly string _connectionString;

        public BasicPgConnectionFactory(DbProviderFactory dbProviderFactory, string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string must be set to a value.");
            }

            _dbProviderFactory = dbProviderFactory;
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> Build(string database = null)
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);

            if (!String.IsNullOrEmpty(database))
            {
                builder.Database = database;
            }

            var connection = await Task.FromResult(_dbProviderFactory.CreateConnection());

            connection.ConnectionString = builder.ConnectionString;

            return connection;
        }
    }
}
