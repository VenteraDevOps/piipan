using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.Models
{
    public class ParticipantDto : IParticipant
    {
        public string LdsHash { get; set; }
        public string State { get; set; }
        public string CaseId { get; set; }
        public string ParticipantId { get; set; }
        public DateTime? ParticipantClosingDate { get; set; }
        public IEnumerable<DateTime> RecentBenefitMonths { get; set; }
        public bool? VulnerableIndividual { get; set; }

        public ParticipantDto()
        {
            
        }

        public ParticipantDto(IParticipant participant)
        {
            LdsHash = participant.LdsHash;
            State = participant.State;
            CaseId = participant.CaseId;
            ParticipantId = participant.ParticipantId;
            ParticipantClosingDate = participant.ParticipantClosingDate;
            RecentBenefitMonths = participant.RecentBenefitMonths;
            VulnerableIndividual = participant.VulnerableIndividual;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ParticipantDto p = obj as ParticipantDto;
            if (p == null)
            {
                return false;
            }

            return 
                LdsHash == p.LdsHash &&
                State == p.State &&
                CaseId == p.CaseId &&
                ParticipantId == p.ParticipantId &&
                ParticipantClosingDate.Value.Date == p.ParticipantClosingDate.Value.Date &&
                RecentBenefitMonths.SequenceEqual(p.RecentBenefitMonths) &&
                VulnerableIndividual == p.VulnerableIndividual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                LdsHash,
                State,
                CaseId,
                ParticipantId,
                ParticipantClosingDate,
                RecentBenefitMonths,
                VulnerableIndividual
            );
        }
    }
}