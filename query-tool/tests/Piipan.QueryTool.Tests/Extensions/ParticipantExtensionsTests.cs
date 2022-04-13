using System;
using System.Collections.Generic;
using Moq;
using Piipan.Participants.Api.Models;
using Piipan.QueryTool.Extensions;
using Piipan.Shared.API.Utilities;
using Xunit;

#nullable enable

namespace Piipan.QueryTool.Tests.Extensions
{
    public class ParticipantExtensionsTests
    {
        [Fact]
        public void ParticipantClosingDateDisplay_Null()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.ParticipantClosingDate)
                .Returns<DateTime?>(null);

            // Act
            var result = participant.Object.ParticipantClosingDateDisplay();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ParticipantClosingDateDisplay()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.ParticipantClosingDate)
                .Returns(new DateTime(2021, 5, 31));

            // Act
            var result = participant.Object.ParticipantClosingDateDisplay();

            // Assert
            Assert.Equal("2021-05-31", result);
        }

        [Fact]
        public void RecentBenefitIssuanceDatesDisplay_Empty()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitIssuanceDates)
                .Returns(new List<DateRange>());

            // Act
            var result = participant.Object.RecentBenefitIssuanceDatesDisplay();

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void RecentBenefitIssuanceDatesDisplay_Single()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitIssuanceDates)
                .Returns(new List<DateRange>
                {
                    new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1))
                });

            // Act
            var result = participant.Object.RecentBenefitIssuanceDatesDisplay();

            // Assert
            Assert.Equal("2021-04-01/2021-05-01", result);
        }

        [Fact]
        public void RecentBenefitIssuanceDatesDisplay_Multiple()
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.RecentBenefitIssuanceDates)
                .Returns(new List<DateRange>
                {
                    new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                    new DateRange(new DateTime(2021, 6, 1),new DateTime(2021, 7, 1)),
                    new DateRange(new DateTime(2021, 02, 28),new DateTime(2021, 3, 15))
                });

            // Act
            var result = participant.Object.RecentBenefitIssuanceDatesDisplay();

            // Assert
            Assert.Equal("2021-04-01/2021-05-01, 2021-06-01/2021-07-01, 2021-02-28/2021-03-15", result);
        }

        [Theory]
        [InlineData(null, "Yes")]
        [InlineData(true, "Yes")]
        [InlineData(false, "No")]
        public void VulnerableIndividualDisplay(bool? VulnerableIndividual, string expected)
        {
            // Arrange
            var participant = new Mock<IParticipant>();
            participant
                .Setup(m => m.VulnerableIndividual)
                .Returns(VulnerableIndividual);

            // Act
            var result = participant.Object.VulnerableIndividualDisplay();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}