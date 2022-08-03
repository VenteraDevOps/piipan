using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace Piipan.Match.Api.Models.Resolution
{
    /// <summary>
    /// Disposition data for each related state in a match
    /// </summary>
    public class Disposition
    {
        [Display(Name = "Inital Action Date")]
        [JsonProperty("initial_action_at",
            NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? InitialActionAt { get; set; }
        [Display(Name = "Initial Action Taken")]
        [JsonProperty("initial_action_taken")]
        public string InitialActionTaken { get; set; }
        [JsonProperty("invalid_match",
            NullValueHandling = NullValueHandling.Ignore)]
        public bool? InvalidMatch { get; set; }
        [Display(Name = "Invalid Match Reason")]
        [JsonProperty("invalid_match_reason")]
        public string? InvalidMatchReason { get; set; }
        [Display(Name = "Reason for Other")]
        [JsonProperty("other_reasoning_for_invalid_match")]
        public string? OtherReasoningForInvalidMatch { get; set; }
        [JsonProperty("final_disposition",
            NullValueHandling = NullValueHandling.Ignore)]
        [Display(Name = "Final Disposition Taken")]
        public string FinalDisposition { get; set; }
        [Display(Name = "Final Disposition Date")]
        [JsonProperty("final_disposition_date")]
        public DateTime? FinalDispositionDate { get; set; }
        [JsonProperty("vulnerable_individual",
            NullValueHandling = NullValueHandling.Ignore)]
        public bool? VulnerableIndividual { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
