using System;
using System.Linq;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Services;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantServiceTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();
        private string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
        private ICryptographyClient cryptographyClient;

        public ParticipantServiceTests()
        {
            cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);
        }

        [Theory]
        [InlineData(2)]
        public async void AddUploadAndParticipants(int nParticipants)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var redactionService = Mock.Of<IRedactionService>();
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
    
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);

                var participantDao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger, cryptographyClient);
                var uploadDao = new UploadDao(helper.DbConnFactory(Factory, ConnectionString));

                ParticipantService service = new ParticipantService(participantDao, uploadDao, null, redactionService, serviceLogger, cryptographyClient);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId());

                // Act
                await service.AddParticipants(participants, "test-etag", null);

                long lastUploadId = GetLastUploadId();

                // updatiing lds_hash with encryption
                
                participants.ToList().ForEach(p =>
                {
                    p.LdsHash = cryptographyClient.EncryptToBase64String(p.LdsHash);
                    p.CaseId = cryptographyClient.EncryptToBase64String(p.CaseId);
                    p.ParticipantId = cryptographyClient.EncryptToBase64String(p.ParticipantId);
                    p.UploadId = p.UploadId;
                });

                // Assert
                participants.ToList().ForEach(p =>
                {
                    p.UploadId = lastUploadId;
                    var exists = HasParticipant(p);
                    Assert.True(exists);
                });
            }
        }

       
        [Theory]
        [InlineData(5)]
        public async void AfterException_AddParticipantsRollsTranactionBack(int nParticipants)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var redactionService = Mock.Of<IRedactionService>();
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                
                var participantDao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger, cryptographyClient);
                var uploadDao = new UploadDao(helper.DbConnFactory(Factory, ConnectionString));

                ParticipantService service = new ParticipantService(participantDao, uploadDao, null, redactionService, serviceLogger, cryptographyClient);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId());
                participants.Last().LdsHash = null; //Cause the db commit to fail due to a null hash value

                long lastUploadId = GetLastUploadId();

                try
                {
                    // Act
                    await service.AddParticipants(participants, "test-etag", null);
                    throw new Exception("Test should have failed because of participant with null ldsHash value");
                }
                catch (Exception)
                {
                    long expectedNewUploadId = ++lastUploadId;
                    long actualLastUploadId = GetLastUploadIdWithStatus("COMPLETE");
                    Assert.NotEqual(expectedNewUploadId, actualLastUploadId);


                    // Assert
                    participants.ToList().ForEach(p =>
                    {
                        p.UploadId = lastUploadId;
                        var exists = HasParticipant(p);
                        Assert.False(exists);
                    });

                    long lastFailedUploadId = GetLastUploadIdWithStatus("FAILED");

                    Assert.Equal(expectedNewUploadId, lastFailedUploadId);
                }


            }
        }
    }
}
