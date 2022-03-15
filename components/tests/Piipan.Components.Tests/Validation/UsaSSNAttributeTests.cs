using Piipan.Components.Validation;
using Xunit;

namespace Piipan.Components.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaSSN attribute
    /// </summary>
    public class UsaSSNAttributeTests
    {
        /// <summary>
        /// Verify it is either valid or not given multiple inputs
        /// </summary>
        /// <param name="ssn">The value of the associated field</param>
        /// <param name="isValidExpected">Whether or not we expect this field to have an error after the value is entered</param>
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

        /// <summary>
        /// Validate that if an error occurs that the correct error message is returned
        /// </summary>
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
