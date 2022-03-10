using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using Piipan.Participants.Api.Models;

namespace Piipan.Etl.Func.BulkUpload.Models
{
    public class Participant : IParticipant
    {
        public string LdsHash { get; set; } = null!;
        public string State { get; set; }
        public string CaseId { get; set; } = null!;
        public string ParticipantId { get; set; } = null!;
        public DateTime? ParticipantClosingDate { get; set; }
        public IEnumerable<DateTime> RecentBenefitMonths { get; set; } = new List<DateTime>();
        // Set Boolean values here, based on:
        // https://joshclose.github.io/CsvHelper/examples/configuration/attributes/
        // Values should mimic what is set in the Bulk Upload import schema
        [BooleanTrueValues("true")]
        [BooleanFalseValues("false")]
        public bool? ProtectLocation { get; set; }
    }
}