﻿using Piipan.Components.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Piipan.QueryTool.Shared
{
    /// <summary>
    /// Represents form input from user for a match query
    /// </summary>
    public class PiiRecord
    {
        [UsaRequired]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [UsaRequired]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date),
            DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateOfBirth { get; set; }

        [UsaRequired]
        [Display(Name = "Social Security Number")]
        public string? SocialSecurityNum { get; set; }

        [Display(Name = "Participant ID")]
        public string? ParticipantId { get; set; }

        [Display(Name = "Case Number")]
        public string? CaseId { get; set; }
    }
}
