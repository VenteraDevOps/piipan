using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;
using Newtonsoft.Json;

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Initial participant data for each related state in a match
    /// </summary>
    public class Participant
    {
        [JsonProperty("case_id")]
        public string CaseId { get; set; }
        [JsonProperty("participant_closing_date")]
        public DateTime ParticipantClosingDate { get; set; }
        [JsonProperty("participant_id")]
        public string ParticipantId { get; set; }
        [JsonProperty("recent_benefit_issuance_dates")]
        public DateTime[][] RecentBenefitIssuanceDates { get; set; }
        public string State { get; set; }
    }
}
