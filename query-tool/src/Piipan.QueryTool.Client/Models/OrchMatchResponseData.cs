namespace Piipan.QueryTool.Client.Models
{
    public class OrchMatchResponseData
    {
        public List<OrchMatchResult> Results { get; set; } = new List<OrchMatchResult>();

        public List<OrchMatchError> Errors { get; set; } = new List<OrchMatchError>();
    }
}
