using Piipan.Match.Api.Models;
using System.Linq;

namespace Piipan.QueryTool.Extensions
{
    public static class OrchMatchResponseDataExtensions
    {
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
