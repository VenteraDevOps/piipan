using Piipan.Match.Api.Models.Resolution;
using System;

namespace Piipan.QueryTool.Tests.Builders
{
    public static class MatchResApiResponseBuilder
    {
        public static MatchResApiResponse Build(Action<MatchResApiResponse> options = null)
        {
            MatchResApiResponse matchResApiResponse = new MatchResApiResponse()
            {
                Data = new MatchResRecord
                {
                    Initiator = "ea",
                    MatchId = "m123456",
                    States = new string[] { "ea", "eb" }
                }
            };

            options?.Invoke(matchResApiResponse);
            return matchResApiResponse;
        }
    }
}
