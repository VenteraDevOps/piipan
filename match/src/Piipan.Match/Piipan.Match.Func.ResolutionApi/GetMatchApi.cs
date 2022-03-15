using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Func.ResolutionApi.Models;

namespace Piipan.Match.Func.ResolutionApi
{

    /// <summary>
    /// Azure Function implementing Get Match endpoint for Match Resolution API
    /// </summary>
    public class GetMatchApi
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "matches/{matchId}")] HttpRequest req,
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
                var response = new ApiResponse() { Data = matchResRecord };
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

        private void LogRequest(ILogger logger, HttpRequest request)
        {
            logger.LogInformation("Executing request from user {User}", request.HttpContext?.User.Identity.Name);

            string subscription = request.Headers["Ocp-Apim-Subscription-Name"];
            if (subscription != null)
            {
                logger.LogInformation("Using APIM subscription {Subscription}", subscription);
            }

            string username = request.Headers["From"];
            if (username != null)
            {
                logger.LogInformation("on behalf of {Username}", username);
            }
        }

        private ActionResult NotFoundErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.NotFound),
                Title = "NotFoundException",
                Detail = "not found"
            });
            return (ActionResult)new NotFoundObjectResult(errResponse);
        }

        private ActionResult InternalServerErrorResponse(Exception ex)
        {
            var errResponse = new ApiErrorResponse();
            errResponse.Errors.Add(new ApiHttpError()
            {
                Status = Convert.ToString((int)HttpStatusCode.InternalServerError),
                Title = ex.GetType().Name,
                Detail = ex.Message
            });
            return (ActionResult)new JsonResult(errResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
