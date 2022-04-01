using System;
using System.Data;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Services;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Shared.Database;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
{
    /// <summary>
    /// Integration tests for saving csv records to the database on a bulk upload
    /// </summary>
    public class BulkUploadIntegrationTests : DbFixture
    {
        private const string FAKE_CONTAINER_URL = "https://myTest.blob.core.windows.net/mycontainer/example.csv";

        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
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

            services.AddTransient<ICustomerEncryptedBlobRetrievalService>(x =>
            {
                var blobRetrievalService = new Mock<ICustomerEncryptedBlobRetrievalService>();
                var input = new MemoryStream(File.ReadAllBytes("example.csv"));
                var blobClient = new Mock<BlobClient>();
                blobClient.Setup(x => x.OpenRead(0, null, null, default)).Returns(input);

                blobRetrievalService
                    .Setup(x => x.RetrieveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(blobClient.Object);

                return blobRetrievalService.Object;
            });

            services.RegisterParticipantsServices();

            return services.BuildServiceProvider();
        }

        private BulkUpload BuildFunction()
        {
            var services = BuildServices();
            return new BulkUpload(
                services.GetService<IParticipantApi>(),
                services.GetService<IParticipantStreamParser>(),
                services.GetService<ICustomerEncryptedBlobRetrievalService>()
            );
        }
                
        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            var services = BuildServices();
            ClearParticipants();
            var eventGridEvent = Mock.Of<EventGridEvent>();
            eventGridEvent.Data = new StorageBlobCreatedEventData() { Url = FAKE_CONTAINER_URL };
            
            var logger = Mock.Of<ILogger>();
            var function = BuildFunction();

            // act
            await function.Run(
                eventGridEvent,
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
            Assert.Equal(new DateTime(2021, 04, 30), records.First().RecentBenefitMonths.First());
            Assert.Equal(new DateTime(2021, 03, 31), records.First().RecentBenefitMonths.ElementAt(1));
            Assert.Equal(new DateTime(2021, 02, 28), records.First().RecentBenefitMonths.ElementAt(2));
            Assert.True(records.First().ProtectLocation);
        }
    }
}
