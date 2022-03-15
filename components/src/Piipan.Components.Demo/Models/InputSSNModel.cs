using Piipan.Components.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Demo.Models
{
    public class InputSSNModel
    {
        [UsaSSN]
        [Display(Name = "Optional SSN")]
        public string? OptionalSSN { get; set; }

        [UsaSSN]
        [UsaRequired]
        [Display(Name = "Required SSN")]
        public string? RequiredSSN { get; set; }
    }
}
