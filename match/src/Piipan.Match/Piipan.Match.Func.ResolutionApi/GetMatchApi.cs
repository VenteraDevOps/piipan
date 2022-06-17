using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.States.Core.DataAccessObjects;

namespace Piipan.Match.Func.ResolutionApi
{

    /// <summary>
    /// Azure Function implementing Get Match endpoint for Match Resolution API
    /// </summary>
    public class GetMatchApi : BaseApi
    {
        private readonly IMatchRecordDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;

        private readonly IMatchResAggregator _matchResAggregator;
        private readonly IStateInfoDao _stateInfoDao;
        private readonly IMemoryCache _memoryCache;

        public const string StateInfoCacheName = "StateInfo";

        public GetMatchApi(
            IMatchRecordDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchResAggregator matchResAggregator,
            IStateInfoDao stateInfoDao,
            IMemoryCache memoryCache)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _stateInfoDao = stateInfoDao;
            _memoryCache = memoryCache;
        }

        [FunctionName("GetMatch")]
        public async Task<IActionResult> GetMatch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "matches/{matchId}")] HttpRequest req,
            string matchId,
            ILogger logger)
        {
            LogRequest(logger, req);
            try
            {
                var match = _matchRecordDao.GetRecordByMatchId(matchId);
                var matchResEvents = _matchResEventDao.GetEvents(matchId);
                await Task.WhenAll(match, matchResEvents);

                string requestLocation = req.Headers["X-Request-Location"];

                // National Office, ignore state checks
                if (requestLocation != "*")
                {
                    var states = await _memoryCache.GetOrCreateAsync(StateInfoCacheName, async (e) =>
                    {
                        return await _stateInfoDao.GetStates();
                    });

                    states = states.Where(n => string.Compare(n.StateAbbreviation, requestLocation, true) == 0
                        || string.Compare(n.Region, requestLocation, true) == 0);

                    if (!match.Result.States.Any(s => states.Any(n => string.Compare(n.StateAbbreviation, s, true) == 0)))
                    {
                        logger.LogInformation("(NOTAUTHORIZEDMATCH) user {User} did not have access to match id {MatchId}", req.HttpContext?.User.Identity.Name, matchId);
                        return NotFoundErrorResponse(null);
                    }
                }

                var matchResRecord = _matchResAggregator.Build(match.Result, matchResEvents.Result);
                var response = new MatchResApiResponse() { Data = matchResRecord };
                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogInformation(ex.Message);
                return NotFoundErrorResponse(ex);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }
    }
}
