namespace Piipan.Notification.Common.Models
{
    public class DispositionModel
    {
        public string MatchId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string InitState { get; set; }

        public bool? InitStateInvalidMatch { get; set; }

        public string? InitStateInvalidMatchReason { get; set; }

        public DateTime? InitStateInitialActionAt { get; set; }

        public string InitStateInitialActionTaken { get; set; }

        public string InitStateFinalDisposition { get; set; }

        public DateTime? InitStateFinalDispositionDate { get; set; }

        public bool? InitStateVulnerableIndividual { get; set; }

        public string MatchingState { get; set; }

        public bool? MatchingStateInvalidMatch { get; set; }

        public string? MatchingStateInvalidMatchReason { get; set; }

        public DateTime? MatchingStateInitialActionAt { get; set; }

        public string MatchingStateInitialActionTaken { get; set; }

        public string MatchingStateFinalDisposition { get; set; }

        public DateTime? MatchingStateFinalDispositionDate { get; set; }

        public bool? MatchingStateVulnerableIndividual { get; set; }

        public string Status { get; set; }
    }
}