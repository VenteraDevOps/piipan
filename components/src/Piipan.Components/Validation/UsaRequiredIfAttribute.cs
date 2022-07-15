using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Validation
{
    public class UsaRequiredIfAttribute : ValidationAttribute
    {
        public string SourceProperty { get; set; }
        private readonly string _value;

        public UsaRequiredIfAttribute(string property, string value = null) : base()
        {
            ErrorMessage = ValidationConstants.RequiredMessage;
            SourceProperty = property;
            _value = value;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value?.ToString();

            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();

            var proprtyvalue = type.GetProperty(SourceProperty).GetValue(instance, null);
            if (string.IsNullOrEmpty(_value))
            {
                if (!string.IsNullOrEmpty(proprtyvalue?.ToString()) && string.IsNullOrEmpty(valueAsString))
                {
                    return new ValidationResult(ErrorMessage, new string[] { validationContext.MemberName });
                }
            }
            else
            {
                if (proprtyvalue?.ToString() == _value && string.IsNullOrEmpty(valueAsString))
                {
                    return new ValidationResult(ErrorMessage, new string[] { validationContext.MemberName });
                }
            }

            return ValidationResult.Success;
        }
    }
}
