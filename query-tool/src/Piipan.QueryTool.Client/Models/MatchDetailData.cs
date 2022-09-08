namespace Piipan.QueryTool.Client.Models
{
    public enum MatchDetailReferralPage
    {
        Self,
        MatchSearch,
        Query,
        Other
    }
    public class MatchDetailData
    {
        public bool? SaveSuccess { get; set; }

        // The values you were attempting to save when a save fails
        public DispositionModel FailedDispositionModel { get; set; }

        public MatchDetailReferralPage ReferralPage { get; set; } = MatchDetailReferralPage.Self;
    }
}
