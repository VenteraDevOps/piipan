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

        public IEnumerable<DateTime> RecentBenefitMonths { get; set; } = new List<DateTime>();

        public bool? ProtectLocation { get; set; }
    }
}
