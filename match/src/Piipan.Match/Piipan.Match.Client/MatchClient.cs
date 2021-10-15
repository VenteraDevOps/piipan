﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Shared.Http;

namespace Piipan.Match.Client
{
    public class MatchClient : IMatchApi
    {
        private readonly IAuthorizedApiClient<MatchClient> _apiClient;

        public MatchClient(IAuthorizedApiClient<MatchClient> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<OrchMatchResponse> FindMatches(OrchMatchRequest request, string initiatingState)
        {
            return await _apiClient
                .PostAsync<OrchMatchRequest, OrchMatchResponse>("find_matches", request, () => 
                {
                    return new List<(string, string)>
                    {
                        ("X-Initiating-State", initiatingState)
                    };
                });
        }
    }
}
