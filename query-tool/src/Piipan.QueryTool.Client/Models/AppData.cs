namespace Piipan.QueryTool.Client.Models
{
    public record AppData
    {
        public string HelpDeskEmail { get; set; }

        public bool IsAuthorized { get; set; } = true;
    }
}
