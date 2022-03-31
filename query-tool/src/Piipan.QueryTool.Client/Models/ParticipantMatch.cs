namespace Piipan.QueryTool.Client.Models
{
    public class ParticipantMatch
    {
        public string? MatchId { get; set; }

        public string? LdsHash { get; set; }

        public string? State { get; set; }

        public string? CaseId { get; set; }

        public string? ParticipantId { get; set; }

        public DateTime? ParticipantClosingDate { get; set; }

        //public IEnumerable<DateRange> RecentBenefitIssuanceDates { get; set; } = new List<DateRange>();

        public bool? ProtectLocation { get; set; }
    }
}
