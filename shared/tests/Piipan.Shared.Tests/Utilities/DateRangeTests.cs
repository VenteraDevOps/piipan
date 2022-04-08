using Piipan.Shared.Utilities;
using System;
using Xunit;

namespace Piipan.Shared.Tests.Utilities
{
    public class DateRangeTests
    {
        [Fact]
        public void DateRange_Properties()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-5);

            // Arrange
            var dateRange = new DateRange(startDate, endDate);

            // Assert
            Assert.Equal(startDate, dateRange.Start);
            Assert.Equal(endDate, dateRange.End);
        }

        [Fact]
        public void DateRange_Equals()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-5);

            // Arrange
            var dateRange = new DateRange(startDate, endDate);
            var dateRange2 = new DateRange(startDate, endDate);

            // Assert
            Assert.Equal(dateRange, dateRange2);
            Assert.Equal(dateRange.GetHashCode(), dateRange2.GetHashCode());
        }

        [Fact]
        public void DateRange_StartMismatch()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-5);
            var startDate2 = endDate.AddDays(-1);

            // Arrange
            var dateRange = new DateRange(startDate, endDate);
            var dateRange2 = new DateRange(startDate2, endDate);

            // Assert
            Assert.NotEqual(dateRange, dateRange2);
            Assert.NotEqual(dateRange.GetHashCode(), dateRange2.GetHashCode());
        }

        [Fact]
        public void DateRange_EndMismatch()
        {
            var endDate = DateTime.Now;
            var endDate2 = endDate.AddDays(1);
            var startDate = endDate.AddDays(-5);
            
            // Arrange
            var dateRange = new DateRange(startDate, endDate);
            var dateRange2 = new DateRange(startDate, endDate2);

            // Assert
            Assert.NotEqual(dateRange, dateRange2);
            Assert.NotEqual(dateRange.GetHashCode(), dateRange2.GetHashCode());
        }
    }
}
