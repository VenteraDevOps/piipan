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
        [UsaRequiredIf(
            nameof(FinalDisposition), "", "@@@ is required because a Final Disposition has been selected",
            nameof(InitialActionTaken), "", "@@@ is required"
        )]
        public DateTime? InitialActionAt { get; set; }

        private string _initialActionTaken;
        [Display(Name = "Initial Action Taken")]
        [JsonProperty("initial_action_taken")]
        [UsaRequiredIf(
            nameof(FinalDisposition), "", "@@@ is required because a Final Disposition has been selected",
            nameof(InitialActionAt), "", "@@@ is required because a date has been selected"
        )]
        public string InitialActionTaken
        {
            get => _initialActionTaken;
            set
            {
                _initialActionTaken = value;
                InitialActionChanged?.Invoke();
            }
        }

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
        public Action InitialActionChanged { get; set; }

        [Display(Name = "Invalid Match Reason")]
        [JsonProperty("invalid_match_reason")]
        public string? InvalidMatchReason { get; set; }
        [Display(Name = "Reason for Other")]
        [JsonProperty("other_reasoning_for_invalid_match")]
        public string? OtherReasoningForInvalidMatch { get; set; }

        [JsonProperty("final_disposition")]
        [Display(Name = "Final Disposition Taken")]
        [UsaRequiredIf(nameof(FinalDispositionDate), "", "@@@ is required because a date has been selected")]
        public string FinalDisposition { get; set; }

        [Display(Name = "Final Disposition Date")]
        [JsonProperty("final_disposition_date")]
        [UsaRequiredIf(nameof(FinalDisposition), "", "@@@ is required")]
        // If the Match Date is not set this won't validate. We set it in MatchDetail.razor.
        // So this is client-side validation ONLY. Server-side validation will also occur in
        // AddEventApi's AddEvent method.
        [UsaMinimumDate(nameof(MatchDate), ErrorMessage = "@@@ cannot be before the match date of {0}")]
        public DateTime? FinalDispositionDate { get; set; }

        [JsonProperty("vulnerable_individual")]
        public bool? VulnerableIndividual { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }

        public DateTime? MatchDate { get; set; }

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
