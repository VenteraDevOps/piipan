namespace Piipan.Notification.Common.Models
{
    public class DispositionUpdatesModel
    {
        public bool MatchId_Changed { get; set; }

        public bool CreatedAt_Changed { get; set; }

        public bool InitState_Changed { get; set; }

        public bool InitStateInvalidMatch_Changed { get; set; }

        public bool InitStateInvalidMatchReason_Changed { get; set; }

        public bool InitStateInitialActionAt_Changed { get; set; }

        public bool InitStateInitialActionTaken_Changed { get; set; }

        public bool InitStateFinalDisposition_Changed { get; set; }

        public bool InitStateFinalDispositionDate_Changed { get; set; }

        public bool InitStateVulnerableIndividual_Changed { get; set; }

        public bool MatchingState_Changed { get; set; }

        public bool MatchingStateInvalidMatch_Changed { get; set; }

        public bool MatchingStateInvalidMatchReason_Changed { get; set; }

        public bool MatchingStateInitialActionAt_Changed { get; set; }

        public bool MatchingStateInitialActionTaken_Changed { get; set; }

        public bool MatchingStateFinalDisposition_Changed { get; set; }

        public bool MatchingStateFinalDispositionDate_Changed { get; set; }

        public bool MatchingStateVulnerableIndividual_Changed { get; set; }

        public bool Status_Changed { get; set; }
    }
}