using Piipan.Components.Validation;
using Xunit;

namespace Piipan.Components.Tests.Validation
{
    public class UsaSSNAttributeTests
    {
        [Theory]
        [InlineData("123-12-1234", true)]
        [InlineData("12-123-1234", false)]
        [InlineData("123121234", false)]
        [InlineData("123-12-123", false)]
        [InlineData("123-12-123A", false)]
        [InlineData("", true)] // empty will work. If it's required, a seperate required attribute should be used in addition to this
        public void IsValid(string ssn, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Act
            var result = attribute.IsValid(ssn);

            // Assert
            Assert.Equal(isValidExpected, result);
        }

        [Fact]
        public void CorrectErrorMessageIsUsed()
        {
            // Arrange
            var attribute = new UsaSSNAttribute();

            // Assert
            Assert.Equal(ValidationConstants.SSNInvalidMessage, attribute.ErrorMessage);
        }
    }
}
