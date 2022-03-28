using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    public static class ParticipantMatchExtensions
    {
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
                RecentBenefitMonths = participantMatch.RecentBenefitMonths,
                ProtectLocation = participantMatch.ProtectLocation
            };
        }
    }
}
