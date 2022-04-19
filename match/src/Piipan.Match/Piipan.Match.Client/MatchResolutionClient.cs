﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.Http;

namespace Piipan.Match.Client
{
    /// <summary>
    /// HTTP client for interacting with Piipan.Match.Func.Api
    /// </summary>
    public class MatchResolutionClient : IMatchResolutionApi
    {
        private readonly IAuthorizedApiClient<MatchResolutionClient> _apiClient;

        /// <summary>
        /// Intializes a new instance of MatchClient
        /// </summary>
        /// <param name="apiClient">an API client instance scoped to MatchClient</param>
        public MatchResolutionClient(IAuthorizedApiClient<MatchResolutionClient> apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Sends a POST request to the /find_matches endpoint using the API client's configured base URL.
        /// Includes in the request headers the identifier of the state initiating the request
        /// </summary>
        /// <param name="request">A collection of participants to attempt to find matches for</param>
        /// <param name="initiatingState">The two character identifier of the state initiating the request</param>
        /// <returns></returns>
        public async Task<MatchResApiResponse> GetMatch(string matchId)
        {
            var (response, _) = await _apiClient.TryGetAsync<MatchResApiResponse>($"matches/{matchId}");
            return response;
        }
    }
}
