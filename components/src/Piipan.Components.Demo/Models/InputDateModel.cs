using Piipan.Components.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Demo.Models
{
    public class InputDateModel
    {
        [Display(Name = "Optional Date")]
        public DateTime? NotRequiredDate { get; set; }

        [UsaRequired]
        [Display(Name = "Required Date")]
        public DateTime? RequiredDate { get; set; }
    }
}
