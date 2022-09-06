using System;

namespace Piipan.Metrics.Api
{
    public class ParticipantUploadStatisticsRequest
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public int HoursOffset { get; set; }
    }
}
