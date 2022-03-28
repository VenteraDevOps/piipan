namespace Piipan.QueryTool.Client.Models
{
    public class OrchMatchResult
    {
        public int Index { get; set; }

        public IEnumerable<ParticipantMatch> Matches { get; set; } = new List<ParticipantMatch>();
    }
}
