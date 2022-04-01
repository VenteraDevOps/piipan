using Piipan.Match.Api.Models;
using System.Linq;

namespace Piipan.QueryTool.Extensions
{
    /// <summary>
    /// Converting ParticipantMatch in Piipan.Match.Api.Models to the ParticipantMatch in the client class.
    /// Need to find a better long-term solution for this. The reason we can't use the one in Piipan.Match.Api
    /// on the client side is it will bring in a lot of extra, potentially sensitive DLLs to the client side (web assembly)
    /// that could be decompiled.
    /// TODO: Create a client side shared library only
    /// </summary>
    public static class ParticipantMatchExtensions
    {
        /// <summary>
        /// Converts a Piipan.Match.Api.Models.ParticipantMatch to the Client.Models.ParticipantMatch
        /// </summary>
        public static Client.Models.ParticipantMatch ToSharedParticipantMatch(this ParticipantMatch participantMatch)
        {
            return new Client.Models.ParticipantMatch
            {
                MatchId = participantMatch.MatchId,
                LdsHash = participantMatch.LdsHash,
                State = participantMatch.State,
                CaseId = participantMatch.CaseId,
                ParticipantId = participantMatch.ParticipantId,
                ParticipantClosingDate = participantMatch.ParticipantClosingDate,
                RecentBenefitIssuanceDates = participantMatch.RecentBenefitIssuanceDates?
                    .Select(n => new Client.Utilities.DateRange { End = n.End, Start = n.Start }),
                ProtectLocation = participantMatch.ProtectLocation
            };
        }
    }
}
