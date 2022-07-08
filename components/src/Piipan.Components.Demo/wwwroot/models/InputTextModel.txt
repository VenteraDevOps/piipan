using Piipan.Components.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Demo.Models
{
    /// <summary>
    /// A model that simulates an object that has a required text field, and not required text field.
    /// </summary>
    public class InputTextModel
    {
        [Display(Name = "Optional Field")]
        public string NotRequiredField { get; set; }

        [UsaRequired]
        [Display(Name = "Required Field")]
        public string RequiredField { get; set; }
    }
}
