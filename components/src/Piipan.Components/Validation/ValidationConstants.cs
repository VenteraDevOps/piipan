namespace Piipan.Components.Validation
{
    /// <summary>
    /// Common validation messages that can be used in multiple client-side projects. @@@ is a placeholder, that will be replaced later by
    /// the consuming application. It will put in the name of the field, and optionally create a link that when clicked, focuses the cursor
    /// into that field.
    /// </summary>
    public class ValidationConstants
    {
        /// <summary>
        /// Used when a field is invalid, such as an invalid date
        /// </summary>
        public const string InvalidMessage = "@@@ is invalid";

        /// <summary>
        /// Used for when a field is required, and is not filled in
        /// </summary>
        public const string RequiredMessage = "@@@ is required";

        /// <summary>
        /// Used for social security number validation, when the SSN is not in the correct format
        /// </summary>
        public const string SSNInvalidMessage = "@@@ must have the form ###-##-####";
    }
}
