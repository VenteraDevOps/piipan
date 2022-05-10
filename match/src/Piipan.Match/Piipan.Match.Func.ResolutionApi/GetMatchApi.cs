using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using System;
using System.Threading.Tasks;

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

        public GetMatchApi(
            IMatchRecordDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchResAggregator matchResAggregator)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
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
