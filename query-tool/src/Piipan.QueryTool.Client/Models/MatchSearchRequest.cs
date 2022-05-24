using System.ComponentModel.DataAnnotations;
using Piipan.Components.Validation;
using static Piipan.Components.Validation.ValidationConstants;

namespace Piipan.QueryTool.Client.Models
{
    public class MatchSearchRequest
    {
        [UsaRequired]
        [StringLength(7, ErrorMessage = $"{ValidationFieldPlaceholder} must be 7 characters", MinimumLength = 7)]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = $"{ValidationFieldPlaceholder} contains invalid characters")]
        [Display(Name = "Match ID")]
        public string MatchId { get; set; }
    }
}
