using Piipan.Match.Api.Models;
using System.Linq;

namespace Piipan.QueryTool.Extensions
{
    /// <summary>
    /// Converting OrchMatchResponseData in Piipan.Match.Api.Models to the OrchMatchResponseData in the client class.
    /// Need to find a better long-term solution for this. The reason we can't use the one in Piipan.Match.Api
    /// on the client side is it will bring in a lot of extra, potentially sensitive DLLs to the client side (web assembly)
    /// that could be decompiled.
    /// TODO: Create a client side shared library only
    /// </summary>
    public static class OrchMatchResponseDataExtensions
    {
        /// <summary>
        /// Converts a Piipan.Match.Api.Models.OrchMatchResponseData to the Client.Models.OrchMatchResponseData
        /// </summary>
        public static Client.Models.OrchMatchResponseData ToSharedOrchMatchResponseData(this OrchMatchResponseData responseData)
        {
            return new Client.Models.OrchMatchResponseData
            {
                Errors = responseData.Errors?.Select(n => n?.ToSharedOrchMatchError()).ToList(),
                Results = responseData.Results?.Select(n => n?.ToSharedOrchMatchResult()).ToList()
            };
        }
    }
}
