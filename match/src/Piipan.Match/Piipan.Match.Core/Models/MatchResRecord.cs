using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Aggregate match resolution data that a MatchResAggregator returns
    /// </summary>
    public class MatchResRecord
    {
        public Disposition[] Dispositions { get; set; } = Array.Empty<Disposition>();
        public Participant[] Participants { get; set; } = Array.Empty<Participant>();
        public string Status { get; set; } = "open";

    }
}
