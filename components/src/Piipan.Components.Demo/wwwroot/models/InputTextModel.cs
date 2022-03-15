using Piipan.Components.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Demo.Models
{
    public class InputTextModel
    {
        [Display(Name = "Optional Field")]
        public string? NotRequiredField { get; set; }

        [UsaRequired]
        [Display(Name = "Required Field")]
        public string? RequiredField { get; set; }
    }
}
