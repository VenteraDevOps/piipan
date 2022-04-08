using Newtonsoft.Json;
using System;
namespace Piipan.Match.Api.Models.Resolution
{
    /// <summary>
    /// Disposition data for each related state in a match
    /// </summary>
    public class Disposition
    {
        [JsonProperty("initial_action_at")]
        public DateTime? InitialActionAt { get; set; }
        [JsonProperty("invalid_match")]
        public bool InvalidMatch { get; set; } = false;
        [JsonProperty("final_disposition")]
        public string FinalDisposition { get; set; }
        [JsonProperty("protect_location")]
        public bool? ProtectLocation { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
