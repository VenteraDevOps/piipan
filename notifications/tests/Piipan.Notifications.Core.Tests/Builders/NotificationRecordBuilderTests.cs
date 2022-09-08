using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Builders;
using Xunit;

namespace Piipan.Notifications.Core.Tests.Builders
{
    public class NotificationRecordBuilderTests
    {
        [Fact]
        public void SetMatchRecord()
        {
            // Arrange
            var builder = new NotificationRecordBuilder();
            var matchRecord = new MatchModel()
            {
                MatchId = "foo",
                InitState = "ea",
                MatchingState = "eb",
                MatchingUrl = "http://test.com",
            };

            // Act
            var record = builder
                .SetMatchModel(matchRecord)
                .GetRecord();

            // Assert
            Assert.True(record.MatchRecord == matchRecord);

        }
        [Fact]
        public void SetEmailToRecordMS()
        {
            // Arrange
            var builder = new NotificationRecordBuilder();

            string EmailTo = "eb@nac.com";


            // Act
            var record = builder
                .SetEmailMatchingStateModel(EmailTo)
                .GetRecord();

            // Assert
            Assert.True(record.EmailToRecordMS.EmailTo == EmailTo);

        }
        [Fact]
        public void SetEmailToRecord()
        {
            // Arrange
            var builder = new NotificationRecordBuilder();

            string EmailTo = "eb@nac.com";


            // Act
            var record = builder
                .SetEmailToModel(EmailTo)
                .GetRecord();

            // Assert
            Assert.True(record.EmailToRecord.EmailTo == EmailTo);

        }
        [Fact]
        public void SetSetDispositionModelRecord()
        {
            // Arrange
            var builder = new NotificationRecordBuilder();

            var disposition = new DispositionModel()
            {
                MatchId = "foo",
                InitState = "ea",
                MatchingState = "eb"
            };


            // Act
            var record = builder
                .SetDispositionModel(disposition)
                .GetRecord();

            // Assert
            Assert.True(record.MatchResEvent == disposition);

        }
        [Fact]
        public void Builder_IsReusable()
        {
            // Arrange
            var builder = new NotificationRecordBuilder();

            // Act
            var recordA = builder.GetRecord();

            // Builder should reset internal MatchRecordDbo
            // object after GetRecord() is called
            var recordB = builder.GetRecord();

            // Assert
            Assert.False(Object.ReferenceEquals(recordA, recordB));
        }

    }
}
