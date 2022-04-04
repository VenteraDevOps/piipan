using Piipan.Components.Validation;
using System;
using Xunit;

namespace Piipan.Components.Tests.Validation
{
    public class DateOfBirthRangeAttributeTests
    {
        [Theory]
        [InlineData("1/1/1900", 1901, 1, 1, true)]
        [InlineData("1/1/1900", 1899, 1, 1, false)]
        [InlineData("1/1/1900", 1900, 1, 1, true)]
        [InlineData("1/1/1900", 2021, 1, 1, true)]
        public void IsValid(string min, int y, int m, int d, bool isValidExpected)
        {
            // Arrange
            var attribute = new DateOfBirthRangeAttribute(min);

            // Act
            var result = attribute.IsValid(new DateTime(y, m, d));

            // Assert
            Assert.Equal(isValidExpected, result);
        }
    }
}