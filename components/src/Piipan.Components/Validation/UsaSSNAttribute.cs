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
            ErrorMessage = ValidationConstants.SSNInvalidMessage;
        }

        public override bool IsValid(object value)
        {
            if (!base.IsValid(value))
            {
                ErrorMessage = ValidationConstants.SSNInvalidMessage;
                return false;
            }
            var stringValue = value?.ToString();

            // If the SSN is required, we'll pick it up with a UsaRequired attribute. Don't flag it here
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }
            // if we're here, our string should be ###-##-####.
            try
            {
                var areaNumber = int.Parse(stringValue[0..3]);
                if (areaNumber == 0 || areaNumber == 666 || areaNumber >= 900)
                {
                    ErrorMessage = $"The first three numbers of @@@ cannot be {stringValue[0..3]}";
                    return false;
                }
                if (stringValue[4..6] == "00")
                {
                    ErrorMessage = $"The middle two numbers of @@@ cannot be 00";
                    return false;
                }
                if (stringValue[7..11] == "0000")
                {
                    ErrorMessage = $"The last four numbers of @@@ cannot be 0000";
                    return false;
                }
            }
            catch
            {
                ErrorMessage = "@@@ is invalid";
                return false;
            }
            return true;
                
        }
    }
}
