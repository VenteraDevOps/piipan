using Piipan.Metrics.Api;

namespace Piipan.Dashboard.Client.Models
{
    public class ParticipantUploadModel
    {
        public List<ParticipantUpload> ParticipantUploadResults { get; set; } = new List<ParticipantUpload>();
        public ParticipantUploadRequestFilter ParticipantUploadFilter { get; set; } = new();
        public ParticipantUploadStatistics Statistics { get; set; } = new();
        public long Total { get; set; }
        public string PageParams { get; set; }
    }
}
