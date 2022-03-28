using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Piipan.Match.Api.Serializers;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Api.Models
{
    public class ParticipantMatch : IParticipant
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("lds_hash",
            NullValueHandling = NullValueHandling.Ignore)]
        public string LdsHash { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("case_id")]
        public string CaseId { get; set; }

        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }

        [JsonProperty("participant_closing_date")]
        [JsonConverter(typeof(JsonConverters.DateTimeConverter))]
        public DateTime? ParticipantClosingDate { get; set; }

        [JsonProperty("recent_benefit_months")]
        [JsonConverter(typeof(JsonConverters.MonthEndArrayConverter))]
        public IEnumerable<DateTime> RecentBenefitMonths { get; set; } = new List<DateTime>();

        [JsonProperty("vulnerable_individual")]
        public bool? VulnerableIndividual { get; set; }

        public ParticipantMatch() { }

        public ParticipantMatch(IParticipant p)
        {
            LdsHash = p.LdsHash;
            State = p.State;
            CaseId = p.CaseId;
            ParticipantId = p.ParticipantId;
            ParticipantClosingDate = p.ParticipantClosingDate;
            RecentBenefitMonths = p.RecentBenefitMonths;
            VulnerableIndividual = p.VulnerableIndividual;
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
