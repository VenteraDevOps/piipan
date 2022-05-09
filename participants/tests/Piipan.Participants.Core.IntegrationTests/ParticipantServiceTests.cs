using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Services;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantServiceTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();

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
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var participantDao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger);
                var uploadDao = new UploadDao(helper.DbConnFactory(Factory, ConnectionString));

                ParticipantService service = new ParticipantService(participantDao, uploadDao, null, serviceLogger);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId());

                // Act
                await service.AddParticipants(participants,"test-etag");

                long lastUploadId = GetLastUploadId();

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
                var serviceLogger = Mock.Of<ILogger<ParticipantService>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var participantDao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger);
                var uploadDao = new UploadDao(helper.DbConnFactory(Factory, ConnectionString));

                ParticipantService service = new ParticipantService(participantDao, uploadDao, null, serviceLogger);

                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId());
                participants.Last().LdsHash = null; //Cause the db commit to fail due to a null hash value

                long lastUploadId = GetLastUploadId();

                try
                {
                    // Act
                    await service.AddParticipants(participants, "test-etag");
                    throw new Exception("Test should have failed because of participant with null ldsHash value");
                }
                catch (Exception)
                {
                    long expectedNewUploadId = ++lastUploadId;
                    long actualLastUploadId = GetLastSuccessfulUploadId();
                    Assert.NotEqual(expectedNewUploadId, actualLastUploadId);

                    // Assert
                    participants.ToList().ForEach(p =>
                    {
                        p.UploadId = lastUploadId;
                        var exists = HasParticipant(p);
                        Assert.False(exists);
                    });
                }


            }
        }
    }
}
