using Piipan.Metrics.Api;

namespace Piipan.Dashboard.Client.Models
{
    public class ParticipantUploadModel
    {
        public List<ParticipantUpload> ParticipantUploadResults { get; set; } = new List<ParticipantUpload>();

        //optional? below
        public string? NextPageParams { get; private set; }
        public string? PrevPageParams { get; private set; }
        public string? StateQuery { get; private set; }
        public static int PerPageDefault = 10;
    }
}
