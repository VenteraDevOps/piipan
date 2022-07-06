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
        [JsonProperty("initial_action_at")]
        public DateTime? InitialActionAt { get; set; }
        [JsonProperty("initial_action_taken")]
        public string InitialActionTaken { get; set; }
        [JsonProperty("invalid_match")]
        public bool? InvalidMatch { get; set; }
        [JsonProperty("final_disposition")]
        [Display(Name = "Final Disposition Taken")]
        public string FinalDisposition { get; set; }
        [Display(Name = "Final Disposition Date")]
        [JsonProperty("final_disposition_date")]
        public DateTime? FinalDispositionDate { get; set; }
        [JsonProperty("vulnerable_individual")]
        public bool? VulnerableIndividual { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
