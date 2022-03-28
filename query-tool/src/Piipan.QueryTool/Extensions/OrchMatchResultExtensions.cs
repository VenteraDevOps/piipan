using Piipan.Match.Api.Models;
using System.Linq;

namespace Piipan.QueryTool.Extensions
{
    public static class OrchMatchResultExtensions
    {
        public static Client.Models.OrchMatchResult ToSharedOrchMatchResult(this OrchMatchResult result)
        {
            return new Client.Models.OrchMatchResult
            {
                Index = result.Index,
                Matches = result.Matches?.Select(n => n?.ToSharedParticipantMatch()).ToList()
            };
        }
    }
}
