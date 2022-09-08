using System;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Metrics.Api
{
    public class ParticipantUploadRequestFilter
    {
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        public int HoursOffset { get; set; }
        [Display(Name = "State")]
        public string State { get; set; }
        public string Status { get; set; }
        public int Page { get; set; } = 1;
        public int PerPage { get; set; } = 53;
    }
}
