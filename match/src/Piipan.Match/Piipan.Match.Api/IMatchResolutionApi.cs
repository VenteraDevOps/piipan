﻿using System.Threading.Tasks;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.Match.Api
{
    public interface IMatchResolutionApi
    {
        Task<MatchResApiResponse> GetMatch(string matchId, string requestLocation);
        Task<MatchResListApiResponse> GetMatches();
    }
}
