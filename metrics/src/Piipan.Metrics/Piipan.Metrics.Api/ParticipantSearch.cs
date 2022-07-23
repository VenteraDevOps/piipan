using System;
using Newtonsoft.Json;

namespace Piipan.Metrics.Api
{
    /// <summary>
    /// Data Mapper for participant_uploads table in metrics database
    /// </summary>
    public class ParticipantSearch
    {
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("search_reason")]
        public string SearchReason { get; set; }
        [JsonProperty("search_from")]
        public string SearchFrom { get; set; }
        [JsonProperty("match_creation")]
        public string MatchCreation{ get; set; }
        [JsonProperty("match_count")]
        public int MatchCount { get; set; }
        [JsonProperty("searched_at")]
        public DateTime SearchedAt { get; set; }
    }
}
