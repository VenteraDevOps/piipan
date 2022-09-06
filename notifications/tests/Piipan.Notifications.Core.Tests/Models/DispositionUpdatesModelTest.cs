using Piipan.Notification.Common.Models;
using Xunit;
namespace Piipan.Notifications.Core.Tests.Models
{
    public class DispositionUpdatesModelTest
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var record = new DispositionUpdatesModel
            {
                MatchId_Changed = false,
                InitStateFinalDispositionDate_Changed = false,
                CreatedAt_Changed = true,
                Status_Changed = false
            };

            // Act / Assert
            Assert.False(record.Equals(null));
        }

    }
}
