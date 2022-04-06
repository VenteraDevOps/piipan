using System;
using System.Collections.Generic;
using System.Text;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Shared.Utilities;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests.Models
{
    public class ParticipantTests
    {
        [Fact]
        public void Getters_Setters_Match()
        {
            // Arrange
            DateTime currentDate = DateTime.UtcNow.Date;
            List<DateRange> recentBenefitIssuanceDates = new List<DateRange>();

            var participant = new Participant
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = currentDate,
                RecentBenefitIssuanceDates = recentBenefitIssuanceDates,
                ProtectLocation = false
            };

            // Act / Assert
            Assert.Equal("l", participant.LdsHash);
            Assert.Equal("c", participant.CaseId);
            Assert.Equal("p", participant.ParticipantId);
            Assert.Equal(currentDate, participant.ParticipantClosingDate);
            Assert.False(participant.ProtectLocation);
            Assert.Same(recentBenefitIssuanceDates, participant.RecentBenefitIssuanceDates);
        }
    }
}
