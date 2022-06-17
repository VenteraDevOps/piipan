using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new Mock<IFunctionsHostBuilder>();
            builder
                .Setup(m => m.Services)
                .Returns(services);

            var target = new Startup();

            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            Environment.SetEnvironmentVariable(Startup.DatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            Environment.SetEnvironmentVariable("EventGridEndPoint","http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString","example");
            target.Configure(builder.Object);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDbConnectionFactory<ParticipantsDb>>());
            Assert.NotNull(provider.GetService<IParticipantApi>());
            Assert.NotNull(provider.GetService<IParticipantStreamParser>());
            Assert.NotNull(provider.GetService<IBlobClientStream>());
        }
    }
}
