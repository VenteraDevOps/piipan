using Piipan.Components.Validation;
using Xunit;

namespace Piipan.Components.Tests.Validation
{
    /// <summary>
    /// Tests associated with the UsaRequired attribute
    /// </summary>
    public class UsaRequiredIfAttributeTests
    {
        public class DummyModel
        {
            public string SourceString { get; set; }

            [UsaRequiredIf(nameof(SourceString))]
            public string TargetString { get; set; }
        }

        /// <summary>
        /// Validate a UsaRequiredIf attribute attached to a dummy model using various parameters
        /// </summary>
        /// <param name="value">The value of the associated field</param>
        /// <param name="isValidExpected">Whether or not we expect this field to have an error after the value is entered</param>
        [Theory]
        [InlineData("", "test", null, null)]
        [InlineData(null, "test", null, null)]
        [InlineData("", "", null, null)]
        [InlineData(null, null, null, null)]
        [InlineData("test", "", null, "Target Value cannot be empty because source value is not null")]
        [InlineData("test", "test", null, null)]
        [InlineData("expected", "", "expected", "Target Value cannot be empty because source value is set to the expected value")]
        [InlineData("expected", "test", "expected", null)]
        [InlineData("notexpected", "", "expected", null)]
        [InlineData("notexpected", "test", "expected", null)]
        public void UsaRequiredIf_ValidateTests(string sourceValue, string targetValue, string requiredIfEqualText, string errorText)
        {
            // Arrange
            var attribute = new UsaRequiredIfAttribute(nameof(DummyModel.SourceString), requiredIfEqualText)
            {
                ErrorMessage = errorText
            };

            var dummyModel = new DummyModel()
            {
                SourceString = sourceValue,
                TargetString = targetValue
            };

            // Act
            var validationResult = attribute.GetValidationResult(targetValue, new System.ComponentModel.DataAnnotations.ValidationContext(dummyModel));

            // Assert
            Assert.Equal(errorText, validationResult?.ErrorMessage);
        }

    }
}
