using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Piipan.Components.Validation;
using static Piipan.Components.Validation.ValidationConstants;
namespace Piipan.QueryTool.Client.Models
{
    /// <summary>
    /// Represents form input from user for a match query
    /// </summary>
    public class PiiQuery
    {
        [UsaRequired]
        [UsaName]
        [Display(Name = "Last Name")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [UsaRequired]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date),
            DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DateOfBirthRange("01/01/1900", ErrorMessage = $"{ValidationFieldPlaceholder} must be between 01-01-1900 and today's date")]
        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth { get; set; }

        [UsaRequired]
        [UsaSSN]
        [Display(Name = "Social Security Number")]
        [JsonPropertyName("ssn")]
        public string SocialSecurityNum { get; set; }

        [UsaRequired]
        [Display(Name = "Participant ID")]
        [RegularExpression("^[A-Za-z0-9-_]+$", ErrorMessage = $"{ValidationFieldPlaceholder} must contain uppercase letters (A-Z), lowercase letters (a-z), numbers (0-9), underscore (_), dash (-).")]
        [MaxLength(20, ErrorMessage = $"{ValidationFieldPlaceholder} can be maximum 20 characters.")]
        [JsonPropertyName("participant_id")]
        public string ParticipantId { get; set; }

        [Display(Name = "Case Number")]
        [JsonPropertyName("case_id")]
        [RegularExpression("^[A-Za-z0-9-_]+$", ErrorMessage = $"{ValidationFieldPlaceholder} must contain uppercase letters (A-Z), lowercase letters (a-z), numbers (0-9), underscore (_), dash (-).")]
        [MaxLength(20, ErrorMessage = $"{ValidationFieldPlaceholder} can be maximum 20 characters.")]
        public string CaseId { get; set; }

        [UsaRequired]
        [Display(Name = "Search Reason")]
        [JsonPropertyName("search_reason")]
        public String SearchReason { get; set; }
    }
}
