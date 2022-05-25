using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Xunit;

namespace Piipan.Participants.Core.IntegrationTests
{
    [Collection("Core.IntegrationTests")]
    public class ParticipantDaoTests : DbFixture
    {
        private ParticipantTestDataHelper helper = new ParticipantTestDataHelper();

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void AddParticipants(int nParticipants)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger);
                var participants = helper.RandomParticipants(nParticipants, GetLastUploadId());

                // Act
                await dao.AddParticipants(participants);

                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p));
                });
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(20)]
        public async void GetParticipants(int nMatches)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var randoms = helper.RandomParticipants(nMatches, GetLastUploadId());
                var participants = randoms.ToList().Select(p =>
                {
                    return new ParticipantDbo
                    {
                        // make the hashes and upload id match for all of them
                        LdsHash = randoms.First().LdsHash,
                        State = randoms.First().State,
                        CaseId = p.CaseId,
                        ParticipantId = p.ParticipantId,
                        ParticipantClosingDate = p.ParticipantClosingDate,
                        RecentBenefitIssuanceDates = p.RecentBenefitIssuanceDates,
                        VulnerableIndividual = p.VulnerableIndividual,
                        UploadId = randoms.First().UploadId
                    };
                });

                participants.ToList().ForEach(p => Insert(p));

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger);

                // Act
                var matches = await dao.GetParticipants("ea", randoms.First().LdsHash, randoms.First().UploadId);

                // Assert
                Assert.True(participants.OrderBy(p => p.CaseId).SequenceEqual(matches.OrderBy(p => p.CaseId)));
            }
        }
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public async void DeleteOldParticipants(int nParticipants)
        {
            using (var conn = Factory.CreateConnection())
            {
                // Arrange
                conn.ConnectionString = ConnectionString;
                conn.Open();
                ClearParticipants();

                var logger = Mock.Of<ILogger<ParticipantDao>>();
                var bulkLogger = Mock.Of<ILogger<ParticipantBulkInsertHandler>>();
                var bulkInserter = new ParticipantBulkInsertHandler(bulkLogger);
                var dao = new ParticipantDao(helper.DbConnFactory(Factory, ConnectionString), bulkInserter, logger);
                InsertUpload();
                var participants = helper.RandomParticipants(nParticipants, GetLastUploadIdWithStatus("COMPLETE"));

                // Act
                await dao.AddParticipants(participants);

                // Insert New Participants
                InsertUpload();
                var participantsNew = helper.RandomParticipants(nParticipants, GetLastUploadIdWithStatus("COMPLETE"));

                await dao.AddParticipants(participantsNew);
                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p));
                });
                participantsNew.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p));
                });
                // Now Delete Old Participants.

               await dao.DeleteOldParticipantsExcept(string.Empty, GetLastUploadIdWithStatus("COMPLETE"));
                // Assert
                participants.ToList().ForEach(p =>
                {
                    Assert.False(HasParticipant(p));
                });
                participantsNew.ToList().ForEach(p =>
                {
                    Assert.True(HasParticipant(p));
                });
            }
        }
    }
}
