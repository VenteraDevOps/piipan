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
    }
}
