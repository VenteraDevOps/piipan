using Piipan.Match.Api.Models;
using System.Linq;

namespace Piipan.QueryTool.Extensions
{
    /// <summary>
    /// Converting OrchMatchResult in Piipan.Match.Api.Models to the OrchMatchResult in the client class.
    /// Need to find a better long-term solution for this. The reason we can't use the one in Piipan.Match.Api
    /// on the client side is it will bring in a lot of extra, potentially sensitive DLLs to the client side (web assembly)
    /// that could be decompiled.
    /// TODO: Create a client side shared library only
    /// </summary>
    public static class OrchMatchResultExtensions
    {
        /// <summary>
        /// Converts a Piipan.Match.Api.Models.OrchMatchResult to the Client.Models.OrchMatchResult
        /// </summary>
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
