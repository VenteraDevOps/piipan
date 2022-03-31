using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;
using Newtonsoft.Json;

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Aggregate match resolution data that a MatchResAggregator returns
    /// </summary>
    public class MatchResRecord
    {
        [JsonProperty("dispositions")]
        public Disposition[] Dispositions { get; set; } = Array.Empty<Disposition>();
        [JsonProperty("initiator")]
        public string Initiator { get; set; }
        [JsonProperty("match_id")]
        public string MatchId { get; set; }
        [JsonProperty("participants")]
        public Participant[] Participants { get; set; } = Array.Empty<Participant>();
        [JsonProperty("states")]
        public string[] States { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; } = MatchRecordStatus.Open;
    }
}
