using System;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Notification.Common;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Match.Func.Api.Tests
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

            // Act
            target.Configure(builder.Object);
            var provider = services.BuildServiceProvider();
            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);
            Environment.SetEnvironmentVariable(Startup.DatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");
            Environment.SetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");

            Environment.SetEnvironmentVariable("EventGridNotificationEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridNotificationKeyString", "example");

            // Assert
            Assert.NotNull(provider.GetService<IMatchApi>());
            Assert.NotNull(provider.GetService<IValidator<OrchMatchRequest>>());
            Assert.NotNull(provider.GetService<IValidator<RequestPerson>>());
            Assert.NotNull(provider.GetService<IStreamParser<OrchMatchRequest>>());
            Assert.NotNull(provider.GetService<IDbConnectionFactory<ParticipantsDb>>());
            Assert.NotNull(provider.GetService<IDbConnectionFactory<CollaborationDb>>());
            Assert.NotNull(provider.GetService<IMatchIdService>());
            Assert.NotNull(provider.GetService<IMatchRecordApi>());
            Assert.NotNull(provider.GetService<IMatchRecordDao>());
            Assert.NotNull(provider.GetService<IViewRenderService>());
            Assert.NotNull(provider.GetService<IMatchEventService>());
            Assert.NotNull(provider.GetService<IActiveMatchRecordBuilder>());
            Assert.NotNull(provider.GetService<IMatchResEventDao>());
            Assert.NotNull(provider.GetService<IMatchResAggregator>());
        }
    }
}
