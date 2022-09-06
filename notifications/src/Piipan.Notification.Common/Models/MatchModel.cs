namespace Piipan.Notification.Common.Models
{
    public class MatchModel
    {
        public string MatchId { get; set; }
        public string InitState { get; set; }
        public string MatchingState { get; set; }
        public string MatchingUrl { get; set; }
        public DateTime InitialActionBy { get; set; }
    }
}