using Piipan.Components.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.QueryTool.Client.Models
{
    public class MatchSearchRequest
    {
        [UsaRequired]
        [StringLength(7, ErrorMessage = "@@@ must be 7 characters", MinimumLength = 7)]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "@@@ contains invalid characters")]
        [Display(Name = "Match ID")]
        public string MatchId { get; set; }
    }
}
