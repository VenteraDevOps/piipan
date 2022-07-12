using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#nullable enable

namespace Piipan.Match.Api.Models
{
    /// <summary>
    /// Full request body for Add Event endpoint in Match Resolution API
    /// </summary>
    public class AddEventRequest
    {
        [JsonProperty("data",
            Required = Required.Always)]
        public AddEventRequestData Data { get; set; } = new AddEventRequestData();
    }

    /// <summary>
    /// Disposition data submitted by states
    /// </summary>
    /// <remarks>
    /// Nullable properties along with NullValueHandling.Ignore allows
    /// null key/values to be removed in JSON serialization.
    /// This is important when inserting match resolution events into the database
    /// and when merging events in match resolution aggregation.
    /// Otherwise, null values would override previously present values when merging.
    /// </remarks>
    public class AddEventRequestData
    {
        [JsonProperty("final_disposition",
            NullValueHandling = NullValueHandling.Ignore)]
        public string? FinalDisposition { get; set; } = null;

        [JsonProperty("initial_action_at",
            NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? InitialActionAt { get; set; } = null;

        [JsonProperty("invalid_match",
            NullValueHandling = NullValueHandling.Ignore)]
        public bool? InvalidMatch { get; set; } = null;

        [JsonProperty("vulnerable_individual",
            NullValueHandling = NullValueHandling.Ignore)]
        public bool? VulnerableIndividual { get; set; } = null;
        
        [JsonProperty("initial_action_taken")]
        public string? InitialActionTaken { get; set; }
        
        [JsonProperty("invalid_match_reason")]
        public string? InvalidMatchReason { get; set; }
        
        [JsonProperty("other_reasoning_for_invalid_match")]
        public string? OtherReasoningForInvalidMatch { get; set; }
        
        [JsonProperty("final_disposition_date")]
        public DateTime? FinalDispositionDate { get; set; }
    }
}
