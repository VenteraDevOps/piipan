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

        public static string RecentBenefitIssuanceDatesDisplay(this IParticipant participant)
        {
            return String.Join(", ", participant.RecentBenefitIssuanceDates.Select(dt => dt.Start.ToString("yyyy-MM-dd") +"/" + dt.End.ToString("yyyy-MM-dd")));
        }

        public static string ProtectLocationDisplay(this IParticipant participant)
        {
            if (participant.ProtectLocation == null)
            {
                return "Yes";
            }
            return participant.ProtectLocation.Value ? "Yes" : "No";
        }
    }
}