using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Services
{
    public class StateInfoService : IStateInfoService
    {
        public const string CacheKey = "StateInfo";
        private readonly IMatchResolutionApi _matchResolutionApi;
        private readonly IMemoryCache _memoryCache;

        public StateInfoService(IMatchResolutionApi matchResolutionApi, IMemoryCache memoryCache)
        {
            _matchResolutionApi = matchResolutionApi;
            _memoryCache = memoryCache;
        }

        public async Task<StateInfoResponse> GetStateInfoAsync()
        {
            return await _memoryCache.GetOrCreateAsync(CacheKey, (cacheEntry) =>
            {
                return _matchResolutionApi.GetStates();
            });
        }
    }
}
