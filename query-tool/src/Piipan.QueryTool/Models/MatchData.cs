using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Api.Serializers;
using Piipan.Participants.Api.Models;

namespace Piipan.QueryTool.Pages
{
    public class MatchData
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("match_status")]
        public string Status { get; set; }


        public MatchData() { }

        public MatchData(MatchData p)
        {
            MatchId = p.MatchId;
            Status = p.Status;

        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}