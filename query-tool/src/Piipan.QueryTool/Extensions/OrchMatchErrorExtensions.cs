using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    /// <summary>
    /// Converting OrchMatchError in Piipan.Match.Api.Models to the OrchMatchError in the client class.
    /// Need to find a better long-term solution for this. The reason we can't use the one in Piipan.Match.Api
    /// on the client side is it will bring in a lot of extra, potentially sensitive DLLs to the client side (web assembly)
    /// that could be decompiled.
    /// TODO: Create a client side shared library only
    /// </summary>
    public static class OrchMatchErrorExtensions
    {
        /// <summary>
        /// Converts a Piipan.Match.Api.Models.OrchMatchError to the Client.Models.OrchMatchError
        /// </summary>
        public static Client.Models.OrchMatchError ToSharedOrchMatchError(this OrchMatchError error)
        {
            return new Client.Models.OrchMatchError
            {
                Index = error.Index,
                Code = error.Code,
                Title = error.Title,
                Detail = error.Detail
            };
        }
    }
}
