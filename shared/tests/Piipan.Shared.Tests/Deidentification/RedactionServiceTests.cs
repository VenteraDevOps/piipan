using Piipan.Shared.Deidentification;
using System.Collections.Generic;
using Xunit;

namespace Piipan.Shared.Tests.Deidentification
{
    public class RedactionServiceTests
    {
        private readonly RedactionService redactionService = new();

        /// <summary>
        /// When the list of items to redact is null, nothing is redacted and the original string is returned.
        /// </summary>
        public void RedactsNothingWhenNullList()
        {
            // Arrange
            string stringToEvaluate = "This is a plain-text string ABCDEFG";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, null);

            // Assert
            Assert.Equal(stringToEvaluate, redactedString);
        }

        /// <summary>
        /// When the list of items to redact is empty, nothing is redacted and the original string is returned.
        /// </summary>
        public void RedactsNothingWhenEmptyList()
        {
            // Arrange
            string stringToEvaluate = "This is a plain-text string ABCDEFG";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, new List<string>());

            // Assert
            Assert.Equal(stringToEvaluate, redactedString);
        }

        /// <summary>
        /// When the list of items doesn't have any strings in it that matches the input string,
        /// nothing is redacted and the original string is returned.
        /// </summary>
        public void RedactsNothingWhenNotFound()
        {
            // Arrange
            string stringToEvaluate = "This is a plain-text string ABCDEFG";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, new List<string> { "FGHIJKL" });

            // Assert
            Assert.Equal(stringToEvaluate, redactedString);
        }

        /// <summary>
        /// When the list of items has a string that matches a string in the input value, the original string is returned but with a redaction
        /// </summary>
        public void RedactsStringWhenFound()
        {
            // Arrange
            string stringToEvaluate = "This is a plain-text string ABCDEFG";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, new List<string> { "ABCDEFG" });

            // Assert
            Assert.Equal("This is a plain-text string REDACTED", redactedString);
        }

        /// <summary>
        /// When the list of items has multiple strings that match strings in the input value, 
        /// the original string is returned but with all of the redactions
        /// </summary>
        public void RedactsMultipleStringsWhenFound()
        {
            // Arrange
            string stringToEvaluate = "This is a ABCDEFG string ABCDEFG FGHIJK";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, new List<string> { "ABCDEFG", "FGHIJK" });

            // Assert
            Assert.Equal("This is a REDACTED string REDACTED REDACTED", redactedString);
        }

        /// <summary>
        /// When the list of items has a string that matches a string in the input value, 
        /// the original string is returned but with a redaction even if the casing is not the same
        /// </summary>
        public void RedactsStringWhenCaseInsensitiveFound()
        {
            // Arrange
            string stringToEvaluate = "This is a plain-text string ABCDEFG";

            // Act
            string redactedString = redactionService.Redact(stringToEvaluate, new List<string> { "abcdefg" });

            // Assert
            Assert.Equal("This is a plain-text string REDACTED", redactedString);
        }
    }
}
