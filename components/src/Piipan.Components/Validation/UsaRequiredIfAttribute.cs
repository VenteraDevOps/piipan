using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Validation
{
    /// <summary>
    /// UsaRequiredIf can be used to make one property dependent on another. For example:
    /// 
    /// public class Foo
    /// {
    ///    public string Prop1 { get; set; }
    ///    
    ///    [UsaRequiredIf(nameof(Prop1))]
    ///    public string Prop2 { get; set; } // This property is required ONLY if Prop1 has a value
    ///    
    ///    [UsaRequiredIf(nameof(Prop1), "Test")]
    ///    public string Prop3 { get; set; } // This property is required ONLY if Prop1 has the value of "Test"
    /// }
    /// </summary>
    public class UsaRequiredIfAttribute : ValidationAttribute
    {
        public string SourceProperty { get; set; }
        private readonly string _sourcePropertyTestValue;

        public UsaRequiredIfAttribute(string sourceProperty, string sourcePropertyTestValue = null) : base()
        {
            ErrorMessage = ValidationConstants.RequiredMessage;
            SourceProperty = sourceProperty;
            _sourcePropertyTestValue = sourcePropertyTestValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string currentValueAsString = value?.ToString();

            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();

            var sourcePropertyValue = type.GetProperty(SourceProperty).GetValue(instance, null);

            // If the source property test value doesn't have a value, we should make our property required if the source property has ANY value
            if (string.IsNullOrEmpty(_sourcePropertyTestValue))
            {
                if (!string.IsNullOrEmpty(sourcePropertyValue?.ToString()) && string.IsNullOrEmpty(currentValueAsString))
                {
                    return new ValidationResult(ErrorMessage, new string[] { validationContext.MemberName });
                }
            }
            // If the source property test value DOES have a value, we should make our property required ONLY if the source property value is equal to the test value
            else
            {
                if (sourcePropertyValue?.ToString() == _sourcePropertyTestValue && string.IsNullOrEmpty(currentValueAsString))
                {
                    return new ValidationResult(ErrorMessage, new string[] { validationContext.MemberName });
                }
            }

            return ValidationResult.Success;
        }
    }
}
