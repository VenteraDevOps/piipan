using System;
using System.Linq;
using Piipan.Participants.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    public static class ParticipantExtensions
    {
        public static string ParticipantClosingDateDisplay(this IParticipant participant)
        {
            return participant.ParticipantClosingDate?.ToString("yyyy-MM-dd");
        }

        public static string RecentBenefitMonthsDisplay(this IParticipant participant)
        {
            return String.Join(", ", participant.RecentBenefitMonths.Select(dt => dt.ToString("yyyy-MM")));
        }

        public static string VulnerableIndividualDisplay(this IParticipant participant)
        {
            if (participant.VulnerableIndividual == null)
            {
                return "Yes";
            }
            return participant.VulnerableIndividual.Value ? "Yes" : "No";
        }
    }
}