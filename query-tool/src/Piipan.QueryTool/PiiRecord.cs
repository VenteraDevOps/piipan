using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool
{
    /// <summary>
    /// Represents form input from user for a match query
    /// </summary>
    public class PiiRecord
    {
        [Required]
        [Display(Name = "First name")]
        [JsonPropertyName("first")]
        public string FirstName { get; set; }

        [Display(Name = "Middle name")]
        [JsonPropertyName("middle")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [JsonPropertyName("last")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of birth")]
        [DataType(DataType.Date),
            DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DateOfBirthRange("01/01/1900", ErrorMessage = "Date of birth must be between 01-01-1900 and today's date")]
        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}-\d{2}-\d{4}$",
            ErrorMessage = "SSN must have the form XXX-XX-XXXX.")]
        [Display(Name = "SSN")]
        [JsonPropertyName("ssn")]
        public string SocialSecurityNum { get; set; }

        [Display(Name = "Partcipant Id")]
        [JsonPropertyName("participant_id")]
        public string ParticipantId { get; set; }

        [Display(Name = "Case Number")]
        [JsonPropertyName("case_id")]
        public string CaseId { get; set; }
    }
}
