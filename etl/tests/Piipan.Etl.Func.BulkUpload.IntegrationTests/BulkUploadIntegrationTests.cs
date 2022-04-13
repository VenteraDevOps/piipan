using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Participants.Api;
using Piipan.Participants.Core;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Database;
using Xunit;
using Azure.Storage.Blobs;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
{
    public class BulkUploadIntegrationTests : DbFixture
    {
        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            SqlMapper.AddTypeHandler(new DateRangeListHandler());
            services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(c =>
            {
                var factory = new Mock<IDbConnectionFactory<ParticipantsDb>>();
                factory
                    .Setup(m => m.Build(It.IsAny<string>()))
                    .ReturnsAsync(() =>
                    {
                        var connection = Factory.CreateConnection();
                        connection.ConnectionString = ConnectionString;
                        return connection;
                    });
                return factory.Object;
            });
            services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();
            services.RegisterParticipantsServices();

            return services.BuildServiceProvider();
        }

        private BlobEventTriggerProcessing BuildFunction()
        {
            var services = BuildServices();
            return new BlobEventTriggerProcessing(
                services.GetService<IParticipantApi>(),
                services.GetService<IParticipantStreamParser>()
            );
        }

        private BulkUpload BuildQueueFunction()
        {
            var services = BuildServices();
            return new BulkUpload(
                services.GetService<IParticipantApi>(),
                services.GetService<IParticipantStreamParser>()
            );
        }

        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            var services = BuildServices();
            ClearParticipants();
            var eventGridEvent = Mock.Of<EventGridEvent>();
            eventGridEvent.Data = new Object();
            // var input = new MemoryStream(File.ReadAllText("example.csv"));
            var input = File.ReadAllText("example.csv");
            BlobServiceClient blobServiceClient = Mock.Of<BlobServiceClient>();
            var containerClient = blobServiceClient.GetBlobContainerClient("containerName");
            // string content = Convert.ToBase64String(File.ReadAllBytes("example.csv"));
            

            BlobClient blobClient = containerClient.GetBlobClient(input);
            
            var logger = Mock.Of<ILogger>();
            var function = BuildFunction();

            // act
            function.Run(
                eventGridEvent,
                blobClient,
                logger
            );

            var records = QueryParticipants("SELECT * from participants;").ToList();

            // assert
            for (int i = 0; i < records.Count(); i++)
            {
                Assert.Equal($"caseid{i + 1}", records.ElementAt(i).CaseId);
                Assert.Equal($"participantid{i + 1}", records.ElementAt(i).ParticipantId);
            }
            Assert.Equal("a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec", records.First().LdsHash);
            Assert.Equal(new DateTime(2021, 05, 15), records.First().ParticipantClosingDate);
            Assert.Equal(new DateRange(new DateTime(2021, 04, 01) , new DateTime(2021, 04, 15)), records.First().RecentBenefitIssuanceDates.First());
            Assert.Equal(new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 30)), records.First().RecentBenefitIssuanceDates.ElementAt(1));
            Assert.Equal(new DateRange(new DateTime(2021, 02, 01), new DateTime(2021, 02, 28)), records.First().RecentBenefitIssuanceDates.ElementAt(2));
            Assert.True(records.First().VulnerableIndividual);
        }
    }
}
