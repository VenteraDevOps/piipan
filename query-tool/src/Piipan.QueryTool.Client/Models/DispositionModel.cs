using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Piipan.Components.Validation;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.QueryTool.Client.Models
{
    public class DispositionModel
    {
        [Display(Name = "Initial Action Date")]
        [JsonProperty("initial_action_at")]
        [UsaRequiredIf(nameof(InitialActionTaken), ErrorMessage = "@@@ is required")]
        public DateTime? InitialActionAt { get; set; }


        [Display(Name = "Initial Action Taken")]
        [JsonProperty("initial_action_taken")]
        [UsaRequiredIf(nameof(InitialActionAt), ErrorMessage = "@@@ is required because a date has been selected")]
        public string InitialActionTaken { get; set; }

        private bool? _invalidMatch = null;
        [JsonProperty("invalid_match")]
        public bool? InvalidMatch
        {
            get => _invalidMatch;
            set
            {
                _invalidMatch = value;
                InvalidMatchChanged?.Invoke();
            }
        }

        public Action InvalidMatchChanged { get; set; }

        [Display(Name = "Invalid Match Reason")]
        [JsonProperty("invalid_match_reason")]
        public string? InvalidMatchReason { get; set; }
        [Display(Name = "Reason for Other")]
        [JsonProperty("other_reasoning_for_invalid_match")]
        public string? OtherReasoningForInvalidMatch { get; set; }
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

        public DispositionModel() { }
        public DispositionModel(Disposition disposition)
        {
            this.InitialActionAt = disposition.InitialActionAt;
            this.InitialActionTaken = disposition.InitialActionTaken;
            this.FinalDisposition = disposition.FinalDisposition;
            this.FinalDispositionDate = disposition.FinalDispositionDate;
            this.InvalidMatch = disposition.InvalidMatch;
            this.InvalidMatchReason = disposition.InvalidMatchReason;
            this.OtherReasoningForInvalidMatch = disposition.OtherReasoningForInvalidMatch;
            this.VulnerableIndividual = disposition.VulnerableIndividual;
            this.State = disposition.State;
        }


    }
}
