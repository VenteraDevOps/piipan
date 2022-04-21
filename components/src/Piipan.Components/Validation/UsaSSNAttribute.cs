using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Validation
{
    /// <summary>
    /// An attribute that defaults to the correct validation for social security numbers (SSN)
    /// </summary>
    public class UsaSSNAttribute : RegularExpressionAttribute
    {
        public UsaSSNAttribute()
            : base(@"^\d{3}-\d{2}-\d{4}$")
        {
            ErrorMessage = ValidationConstants.SSNInvalidFormatMessage;
        }

        public override bool IsValid(object value)
        {
            if (!base.IsValid(value))
            {
                ErrorMessage = ValidationConstants.SSNInvalidFormatMessage;
                return false;
            }
            var stringValue = value?.ToString();

            // If the SSN is required, we'll pick it up with a UsaRequired attribute. Don't flag it here
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            // If we're here, our string will be ###-##-#### because we already checked it against the base Regex.
            var areaNumber = int.Parse(stringValue[0..3]);
            if (areaNumber == 0 || areaNumber == 666 || areaNumber >= 900)
            {
                ErrorMessage = string.Format(ValidationConstants.SSNInvalidFirstThreeDigitsMessage, stringValue[0..3]);
                return false;
            }
            if (stringValue[4..6] == "00")
            {
                ErrorMessage = ValidationConstants.SSNInvalidMiddleTwoDigitsMessage;
                return false;
            }
            if (stringValue[7..11] == "0000")
            {
                ErrorMessage = ValidationConstants.SSNInvalidLastFourDigitsMessage;
                return false;
            }

            return true;

        }
    }
}
