using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Func.ResolutionApi.Models;
using FluentValidation;

#nullable enable

namespace Piipan.Match.Func.ResolutionApi
{
        /// <summary>
        /// Azure Function implementing Disposition Update endpoint for Match Resolution API
        /// </summary>
       public class AddEventApi
       {
            private readonly IMatchRecordDao _matchRecordDao;
            private readonly IMatchResEventDao _matchResEventDao;
            private readonly IMatchResAggregator _matchResAggregator;
            private readonly IStreamParser<AddEventRequest> _requestParser;
            public readonly string UserActor = "user";
            public readonly string SystemActor = "system";
            public readonly string ClosedDelta = "{\"status\": \"closed\"}";

            public AddEventApi(
                IMatchRecordDao matchRecordDao,
                IMatchResEventDao matchResEventDao,
                IMatchResAggregator matchResAggregator,
                IStreamParser<AddEventRequest> requestParser)
            {
                _matchRecordDao = matchRecordDao;
                _matchResEventDao = matchResEventDao;
                _matchResAggregator = matchResAggregator;
                _requestParser = requestParser;
            }

            [FunctionName("AddEvent")]
            public async Task<IActionResult> AddEvent(
                [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "matches/{matchId}/disposition")] HttpRequest req,
                string matchId,
                ILogger logger)
            {
                LogRequest(logger, req);

                try
                {
                    var reqObj = await _requestParser.Parse(req.Body);
                    var match = _matchRecordDao.GetRecordByMatchId(matchId);
                    var matchResEvents = _matchResEventDao.GetEvents(matchId);
                    await Task.WhenAll(match, matchResEvents);
                    // If state does not belong to match, return unauthorized
                    string state = req.Headers["X-Initiating-State"];
                    state = state.ToLower();
                    if (!match.Result.States.Contains(state))
                    {
                        return UnauthorizedErrorResponse();
                    }
                    // If match is closed, return unauthorized
                    var matchResRecord = _matchResAggregator.Build(match.Result, matchResEvents.Result);
                    if (matchResRecord.Status == MatchRecordStatus.Closed)
                    {
                        return UnauthorizedErrorResponse();
                    }
                    // If last event is same as this event, return not allowed
                    IMatchResEvent? lastEvent = matchResEvents.Result.LastOrDefault();
                    if (lastEvent is IMatchResEvent)
                    {
                        var deltaObj = JsonConvert.DeserializeObject<AddEventRequestData>(lastEvent.Delta);
                        if (lastEvent.ActorState == state &&
                            deltaObj is AddEventRequestData &&
                            JsonConvert.SerializeObject(deltaObj) == JsonConvert.SerializeObject(reqObj.Data))
                        {
                            string message = "Duplicate action not allowed";
                            logger.LogError(message);
                            return UnprocessableEntityResponse(message);
                        }
                    }
                    // insert event
                    var newEvent = new MatchResEventDbo() {
                        MatchId = matchId,
                        ActorState = state,
                        Actor = req.Headers["From"].ToString() ?? UserActor,
                        Delta = JsonConvert.SerializeObject(reqObj.Data)
                    };
                    await _matchResEventDao.AddEvent(newEvent);
                    // determine if match should be closed
                    await DetermineClosure(reqObj, match.Result, matchResEvents.Result);
                    return new OkResult();
                }
                catch (StreamParserException ex)
                {
                    logger.LogError(ex.Message);
                    return DeserializationErrorResponse(ex);
                }
                catch (ValidationException ex)
                {
                    logger.LogError(ex.Message);
                    return ValidationErrorResponse(ex);
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex.Message);
                    return NotFoundErrorResponse(ex);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    return InternalServerErrorResponse(ex);
                }
            }

            private async Task DetermineClosure(
                AddEventRequest reqObj,
                IMatchRecord match,
                IEnumerable<IMatchResEvent> matchResEvents
            )
            {
                if (String.IsNullOrEmpty(reqObj.Data.FinalDisposition)) return;

                var dispositions = GetFinalDispositions(matchResEvents);
                if (dispositions.Count() == (match.States.Count() - 1))
                {
                    var closedEvent = new MatchResEventDbo() {
                        MatchId = match.MatchId,
                        Actor = SystemActor,
                        Delta = ClosedDelta
                    };
                    await _matchResEventDao.AddEvent(closedEvent);
                }
            }

            private IEnumerable<IMatchResEvent> GetFinalDispositions(
                IEnumerable<IMatchResEvent> matchResEvents
            )
            {
                return matchResEvents.Where(e => {
                    AddEventRequestData? delta = JsonConvert.DeserializeObject<AddEventRequestData>(e.Delta);
                    return !String.IsNullOrEmpty(delta?.FinalDisposition);
                });
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

            private ActionResult UnprocessableEntityResponse(string message)
            {
                var errResponse = new ApiErrorResponse();
                errResponse.Errors.Add(new ApiHttpError()
                {
                    Status = Convert.ToString((int)HttpStatusCode.UnprocessableEntity),
                    Title = "UnprocessableEntity",
                    Detail = message
                });
                return (ActionResult)new UnprocessableEntityObjectResult(errResponse);
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

            private ActionResult UnauthorizedErrorResponse()
            {

                return (ActionResult)new UnauthorizedResult();
            }

            private ActionResult DeserializationErrorResponse(Exception ex)
            {
                var errResponse = new ApiErrorResponse();
                errResponse.Errors.Add(new ApiHttpError()
                {
                    Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                    Title = Convert.ToString(ex.GetType()),
                    Detail = ex.Message
                });
                return (ActionResult)new BadRequestObjectResult(errResponse);
            }

            private ActionResult ValidationErrorResponse(ValidationException exception)
            {
                var errResponse = new ApiErrorResponse();
                foreach (var failure in exception.Errors)
                {
                    errResponse.Errors.Add(new ApiHttpError()
                    {
                        Status = Convert.ToString((int)HttpStatusCode.BadRequest),
                        Title = failure.ErrorCode,
                        Detail = failure.ErrorMessage
                    });
                }
                return (ActionResult)new BadRequestObjectResult(errResponse);
            }
       }
}
