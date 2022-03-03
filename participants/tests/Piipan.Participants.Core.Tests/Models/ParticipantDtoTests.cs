using System;
using System.Collections.Generic;
using Piipan.Participants.Core.Models;
using Xunit;

namespace Piipan.Participants.Core.Tests.Models
{
    public class ParticipantDtoTests
    {
        [Fact]
        public void Equals_NullObj()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };

            // Act / Assert
            Assert.False(lhs.Equals(null));
        }

        [Fact]
        public void Equals_WrongType()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new
            {
                value = 5
            };

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
        }

        [Fact]
        public void Equals_HashCode_LdsHashMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash + "2",
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_CaseIdMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId + "2",
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ParticipantIdMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId + "2",
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_BenefitsEndDateMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate + TimeSpan.FromDays(1),
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_RecentBenefitsMonthsMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = new List<DateTime> { DateTime.UtcNow },
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_ProtectLocationMismatch()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = !lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.False(lhs.Equals(rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Equals_Match()
        {
            // Arrange
            var lhs = new ParticipantDto
            {
                LdsHash = "l",
                CaseId = "c",
                ParticipantId = "p",
                ParticipantClosingDate = DateTime.UtcNow.Date,
                RecentBenefitMonths = new List<DateTime>(),
                ProtectLocation = false
            };
            var rhs = new ParticipantDto
            {
                LdsHash = lhs.LdsHash,
                CaseId = lhs.CaseId,
                ParticipantId = lhs.ParticipantId,
                ParticipantClosingDate = lhs.ParticipantClosingDate,
                RecentBenefitMonths = lhs.RecentBenefitMonths,
                ProtectLocation = lhs.ProtectLocation
            };
            

            // Act / Assert
            Assert.True(lhs.Equals(rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}