using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Extensions
{
    public static class OrchMatchErrorExtensions
    {
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
