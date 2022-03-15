using Piipan.Components.Validation;
using Xunit;

namespace Piipan.Components.Tests.Validation
{
    public class UsaRequiredAttributeTests
    {
        [Theory]
        [InlineData("value", true)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void IsValid(string value, bool isValidExpected)
        {
            // Arrange
            var attribute = new UsaRequiredAttribute();

            // Act
            var result = attribute.IsValid(value);

            // Assert
            Assert.Equal(isValidExpected, result);
        }

        [Fact]
        public void CorrectErrorMessageIsUsed()
        {
            // Arrange
            var attribute = new UsaRequiredAttribute();

            // Assert
            Assert.Equal(ValidationConstants.RequiredMessage, attribute.ErrorMessage);
        }

    }
}
