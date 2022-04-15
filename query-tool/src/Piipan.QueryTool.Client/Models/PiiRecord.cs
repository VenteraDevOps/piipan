using Piipan.Components.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool.Client.Models
{
    /// <summary>
    /// Represents form input from user for a match query
    /// </summary>
    public class PiiRecord
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
        [DateOfBirthRange("01/01/1900", ErrorMessage = "@@@ must be between 01-01-1900 and today's date")]
        [JsonPropertyName("dob")]
        public DateTime? DateOfBirth { get; set; }

        [UsaRequired]
        [UsaSSN]
        [Display(Name = "Social Security Number")]
        [JsonPropertyName("ssn")]
        public string SocialSecurityNum { get; set; }

        [Display(Name = "Participant ID")]
        [JsonPropertyName("participant_id")]
        public string ParticipantId { get; set; }

        [Display(Name = "Case Number")]
        [JsonPropertyName("case_id")]
        public string CaseId { get; set; }
    }
}
