using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NpgsqlTypes;
using Piipan.Shared.Utilities;
using Xunit;

namespace Piipan.Participants.Core.Tests
{
    public class DateRangeListHandlerTests
    {
        [Fact]
        public void ParseThrowsForBadInput()
        {
            // Arrange 
            var handler = new DateRangeListHandler();

            // Act / Assert
            Assert.Throws<InvalidCastException>(() => handler.Parse("not a List<DateRange> "));
        }

        [Fact]
        public void ParseReturnsList()
        {
            // Arrange
            var handler = new DateRangeListHandler();

            // Act
            var response = handler.Parse(new NpgsqlRange<DateTime>[] {new NpgsqlRange<DateTime>(DateTime.Now, DateTime.UtcNow)});

            Assert.NotNull(response);
            Assert.IsType<List<DateRange>>(response);
            Assert.Equal(1, response.Count());
        }

        [Fact]
        public void SetValueSetsParameterValue()
        {
            // Arrange
            var handler = new DateRangeListHandler();
            var parameter = new Mock<IDbDataParameter>();

            // Act
            handler.SetValue(parameter.Object, new List<DateRange> { new DateRange(
                DateTime.Now,
                DateTime.Now) });
            // Assert
            parameter.VerifySet(m => m.Value = It.IsAny<string>(), Times.Once);
        }
    }
}
