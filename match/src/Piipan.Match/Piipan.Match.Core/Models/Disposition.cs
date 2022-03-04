using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;
using Newtonsoft.Json;
namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Disposition data for each related state in a match
    /// </summary>
    public class Disposition
    {
        [JsonProperty("initial_action_at")]
        public DateTime InitialActionAt { get; set; }
        [JsonProperty("invalid_match")]
        public bool InvalidMatch { get; set; } = false;
        [JsonProperty("final_disposition")]
        public string FinalDisposition { get; set; }
        [JsonProperty("protect_location")]
        public bool? ProtectLocation { get; set; }
        public string State { get; set; }
    }
}
