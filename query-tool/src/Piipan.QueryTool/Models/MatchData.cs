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

        [JsonProperty("lds_hash",
            NullValueHandling = NullValueHandling.Ignore)]
        public string LdsHash { get; set; }

        [JsonProperty("match_status")]
        public string Status { get; set; }
        
        [JsonProperty("match_creation_date")]
        public string MatchCreationDate { get; set; }



        [JsonProperty("querying_state")]
        public string QueryingState { get; set; }

        [JsonProperty("querying_state_vulnerable_status")]
        public bool? QueryingStateVulnerableStatus { get; set; }

        [JsonProperty("querying_state_match_validity")]
        public bool? QueryingStateMatchValidity { get; set; }

        [JsonProperty("querying_state_initial_action")]
        public string QueryingStateInitialAction { get; set; }

        [JsonProperty("querying_state_date_initial_action")]
        public DateTime? QueryingStateDateInitialAction { get; set; }

        [JsonProperty("querying_state_final_disposition")]
        public string QueryingStateFinalDisposition { get; set; }

        [JsonProperty("querying_state_date_final_disposition")]
        public DateTime? QueryingStateDateFinalDisposition { get; set; }

        [JsonProperty("querying_state_case_id")]
        public string QueryingStateCaseId { get; set; }

        [JsonProperty("querying_state_participant_id")]
        public string QueryingStateParticipantId { get; set; }

        [JsonProperty("querying_state_benefits_end_month")]
        [JsonConverter(typeof(JsonConverters.MonthEndConverter))]
        public DateTime? QueryingStateBenefitsEndDate { get; set; }

        [JsonProperty("querying_state_recent_benefit_months")]
        public DateTime? QueryingStateRecentBenefitMonths { get; set; }
        
        [JsonProperty("querying_state_email_address")]
        public string QueryingStateEmailAddress { get; set; }

        [JsonProperty("querying_state_phone_address")]
        public string QueryingStatePhoneAddress { get; set; }



        [JsonProperty("match_state")]
        public string MatchState { get; set; }

        [JsonProperty("match_state_vulnerable_status")]
        public bool? MatchStateVulnerableStatus { get; set; }

        [JsonProperty("match_state_match_validity")]
        public bool? MatchStateMatchValidity { get; set; }

        [JsonProperty("match_state_initial_action")]
        public string MatchStateInitialAction { get; set; }

        [JsonProperty("match_state_date_initial_action")]
        public DateTime? MatchStateDateInitialAction { get; set; }

        [JsonProperty("match_state_final_disposition")]
        public string MatchStateFinalDisposition { get; set; }

        [JsonProperty("match_state_date_final_disposition")]
        public DateTime? MatchStateDateFinalDisposition { get; set; }

        [JsonProperty("match_state_case_id")]
        public string MatchStateCaseId { get; set; }

        [JsonProperty("match_state_participant_id")]
        public string MatchStateParticipantId { get; set; }

        [JsonProperty("match_state_benefits_end_month")]
        public DateTime? MatchStateBenefitsEndDate { get; set; }

        [JsonProperty("match_state_recent_benefit_months")]
        public DateTime? MatchStateRecentBenefitMonths { get; set; }
                
        [JsonProperty("match_state_email_address")]
        public string MatchStateEmailAddress { get; set; }

        [JsonProperty("match_state_phone_address")]
        public string MatchStatePhoneAddress { get; set; }



        [JsonProperty("protect_location")]
        public bool? ProtectLocation { get; set; }

        public MatchData() { }

        public MatchData(MatchData p)
        {
            LdsHash = p.LdsHash;
            MatchId = p.MatchId;
            LdsHash = p.LdsHash;
            Status = p.Status;
            MatchCreationDate = p.MatchCreationDate;
            QueryingState = p.QueryingState;
            QueryingStateVulnerableStatus = p.QueryingStateVulnerableStatus;
            QueryingStateMatchValidity = p.QueryingStateMatchValidity;
            QueryingStateInitialAction = p.QueryingStateInitialAction;
            QueryingStateDateInitialAction = p.QueryingStateDateInitialAction;
            QueryingStateFinalDisposition = p.QueryingStateFinalDisposition;
            QueryingStateDateFinalDisposition = p.QueryingStateDateFinalDisposition;
            QueryingStateCaseId = p.QueryingStateCaseId;
            QueryingStateParticipantId = p.QueryingStateParticipantId;
            QueryingStateBenefitsEndDate = p.QueryingStateBenefitsEndDate;
            QueryingStateRecentBenefitMonths = p.QueryingStateRecentBenefitMonths;
            QueryingStateRecentBenefitMonths = p.QueryingStateRecentBenefitMonths;
            QueryingStateEmailAddress = p.QueryingStateEmailAddress;
            QueryingStatePhoneAddress = p.QueryingStatePhoneAddress;
            MatchState = p.MatchState;
            MatchStateVulnerableStatus = p.MatchStateVulnerableStatus;
            MatchStateMatchValidity = p.MatchStateMatchValidity;
            MatchStateInitialAction = p.MatchStateInitialAction;
            MatchStateDateInitialAction = p.MatchStateDateInitialAction;
            MatchStateFinalDisposition = p.MatchStateFinalDisposition;
            MatchStateDateFinalDisposition = p.MatchStateDateFinalDisposition;
            MatchStateCaseId = p.MatchStateCaseId;
            MatchStateParticipantId = p.MatchStateParticipantId;
            MatchStateBenefitsEndDate = p.MatchStateBenefitsEndDate;
            MatchStateRecentBenefitMonths = p.MatchStateRecentBenefitMonths;
            MatchStateEmailAddress = p.MatchStateEmailAddress;
            MatchStatePhoneAddress = p.MatchStatePhoneAddress;
            ProtectLocation = p.ProtectLocation;

        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }
}