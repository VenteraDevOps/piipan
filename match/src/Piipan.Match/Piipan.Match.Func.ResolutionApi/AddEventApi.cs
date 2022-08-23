using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Metrics.Api;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Piipan.Shared.Http;
using Piipan.States.Core.DataAccessObjects;

#nullable enable

namespace Piipan.Match.Func.ResolutionApi
{
    /// <summary>
    /// Azure Function implementing Disposition Update endpoint for Match Resolution API
    /// </summary>
    public class AddEventApi : BaseApi
    {
        private readonly IMatchRecordDao _matchRecordDao;
        private readonly IMatchResEventDao _matchResEventDao;
        private readonly IMatchResAggregator _matchResAggregator;
        private readonly IStreamParser<AddEventRequest> _requestParser;
        private readonly IParticipantPublishMatchMetric _participantPublishMatchMetric;
        private readonly IStateInfoDao _stateInfoDao;
        private readonly INotificationService _notificationService;
        public readonly string UserActor = "user";
        public readonly string SystemActor = "system";
        public readonly string ClosedDelta = "{\"status\": \"closed\"}";

        public AddEventApi(
            IMatchRecordDao matchRecordDao,
            IMatchResEventDao matchResEventDao,
            IMatchResAggregator matchResAggregator,
            IStreamParser<AddEventRequest> requestParser,
            IParticipantPublishMatchMetric participantPublishMatchMetric,
            IStateInfoDao stateInfoDao,
            INotificationService notificationService)
        {
            _matchRecordDao = matchRecordDao;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _requestParser = requestParser;
            _participantPublishMatchMetric = participantPublishMatchMetric;
            _stateInfoDao = stateInfoDao;
            _notificationService = notificationService;
        }

        [FunctionName("AddEvent")]
        public async Task<IActionResult> AddEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "matches/{matchId}/disposition")] HttpRequest req,
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

                // Additional validation here that couldn't be done in the AddEventRequestValidator since we didn't have a match object to compare against
                if (reqObj.Data.FinalDispositionDate != null && reqObj.Data.FinalDispositionDate.Value < match.Result.CreatedAt?.Date)
                {
                    string errorPrefix = reqObj.Data.FinalDisposition switch
                    {

                        "Benefits Approved" => "Benefits Start Date",
                        "Benefits Terminated" => "Benefits End Date",
                        _ => "Final Disposition Date"
                    };
                    throw new ValidationException("request validation failed",
                        new ValidationFailure[] {
                            new ValidationFailure(nameof(reqObj.Data.FinalDispositionDate), $"{errorPrefix} cannot be before the match date of {match.Result.CreatedAt.Value.ToString("MM/dd/yyyy")}")
                        });
                }

                // If last event is same as this event, return not allowed
                IMatchResEvent? lastEvent = matchResEvents.Result.LastOrDefault();
                if (lastEvent is IMatchResEvent)
                {
                    var deltaObj = JsonConvert.DeserializeObject<Disposition>(lastEvent.Delta);
                    if (lastEvent.ActorState == state &&
                        deltaObj is Disposition &&
                        JsonConvert.SerializeObject(deltaObj) == JsonConvert.SerializeObject(reqObj.Data))
                    {
                        string message = "Duplicate action not allowed";
                        logger.LogError(message);
                        return UnprocessableEntityResponse(message);
                    }
                }
                // insert event
                var newEvent = new MatchResEventDbo()
                {
                    MatchId = matchId,
                    ActorState = state,
                    Actor = req.Headers["From"].ToString() ?? UserActor,
                    Delta = JsonConvert.SerializeObject(reqObj.Data)
                };
                var successfulEventAdd = await _matchResEventDao.AddEvent(newEvent);
                var updatedMatchResEvents = matchResEvents.Result.ToList();
                if (successfulEventAdd != 0)
                {
                    updatedMatchResEvents.Add(newEvent);
                }
                // determine if match should be closed
                await DetermineClosure(reqObj, match.Result, updatedMatchResEvents);
                // Update the latest record to the Metrics database.
                var matchResEventsAfterUpdate = _matchResEventDao.GetEvents(matchId);
                await Task.WhenAll(match, matchResEventsAfterUpdate);
                var matchResRecordAfterUpdate = _matchResAggregator.Build(match.Result, matchResEventsAfterUpdate.Result);

                //Build Search Metrics
                var participantMatchMetrics = new ParticipantMatchMetrics()
                {
                    MatchId = matchResRecordAfterUpdate.MatchId,
                    InitState = matchResRecordAfterUpdate.Initiator,
                    MatchingState = matchResRecordAfterUpdate.States[1],
                    CreatedAt = matchResRecordAfterUpdate.CreatedAt,
                    Status = matchResRecordAfterUpdate.Status,
                    InitStateInvalidMatch = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatch).FirstOrDefault(),
                    InitStateInvalidMatchReason = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatchReason).FirstOrDefault(),
                    InitStateInitialActionAt = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionAt).FirstOrDefault(),
                    InitStateInitialActionTaken = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionTaken).FirstOrDefault(),
                    InitStateFinalDisposition = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDisposition).FirstOrDefault(),
                    InitStateFinalDispositionDate = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDispositionDate).FirstOrDefault(),
                    InitStateVulnerableIndividual = matchResRecordAfterUpdate.Dispositions.Where(r => r.State == matchResRecordAfterUpdate.Initiator).Select(r => r.VulnerableIndividual).FirstOrDefault(),
                    MatchingStateInvalidMatch = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatch).FirstOrDefault(),
                    MatchingStateInvalidMatchReason = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InvalidMatchReason).FirstOrDefault(),
                    MatchingStateInitialActionAt = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionAt).FirstOrDefault(),
                    MatchingStateInitialActionTaken = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.InitialActionTaken).FirstOrDefault(),
                    MatchingStateFinalDisposition = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDisposition).FirstOrDefault(),
                    MatchingStateFinalDispositionDate = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.FinalDispositionDate).FirstOrDefault(),
                    MatchingStateVulnerableIndividual = matchResRecordAfterUpdate.Dispositions.Where(r => r.State != matchResRecordAfterUpdate.Initiator).Select(r => r.VulnerableIndividual).FirstOrDefault(),
                };
                await _participantPublishMatchMetric.PublishMatchMetric(participantMatchMetrics);

                //Send Notification to both initiating state and Matching State.
                // Send template data for any email template which is created based on the requirements.
                // The below logic might change based on the template data for requirements.
                //In future we might end up consolidating the logic based on requirements.
                var states = await _stateInfoDao.GetStates();
                var initState = states?.Where(n => string.Compare(n.StateAbbreviation, matchResRecordAfterUpdate.States[0], true) == 0).FirstOrDefault();
                var matchingState = states?.Where(n => string.Compare(n.StateAbbreviation, matchResRecordAfterUpdate.States[1], true) == 0).FirstOrDefault();
                var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");

                EmailTemplateInput emailTemplateInputIs = GetEmailTemplate(matchResRecordAfterUpdate.MatchId, initState?.State, matchingState?.State, queryToolUrl, initState?.Email);
                emailTemplateInputIs.Topic = "UPDATE_MATCH_RES_IS";
                await _notificationService.PublishMessageFromTemplate(emailTemplateInputIs); //Publishing Email for Initiating State:  Based on the requirement

                EmailTemplateInput emailTemplateInputMs = GetEmailTemplate(matchResRecordAfterUpdate.MatchId, initState?.State, matchingState?.State, queryToolUrl, matchingState?.Email);
                emailTemplateInputMs.Topic = "UPDATE_MATCH_RES_MS";
                await _notificationService.PublishMessageFromTemplate(emailTemplateInputMs); //Publishing Email for Matching State : Based on the requirement

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
        private static EmailTemplateInput GetEmailTemplate(string MatchId, string initState, string matchingState, string queryToolUrl, string email)
        {
            return new EmailTemplateInput()
            {
                TemplateData = new
                {
                    MatchId = MatchId,
                    InitState = initState,
                    MatchingState = matchingState,
                    MatchingUrl = $"{queryToolUrl}/match/{MatchId}",

                },
                EmailTo = email
            };
        }
        private async Task DetermineClosure(
            AddEventRequest reqObj,
            IMatchRecord match,
            IEnumerable<IMatchResEvent> matchResEvents
        )
        {
            if (String.IsNullOrEmpty(reqObj.Data.FinalDisposition) && reqObj.Data.InvalidMatch != true) return;

            var dispositions = GetFinalDispositions(matchResEvents);
            if (dispositions.Count() == (match.States.Count()))
            {
                var closedEvent = new MatchResEventDbo()
                {
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
            List<String> statesReadyToClose = new List<String>();
            return matchResEvents.Where(e =>
            {
                Disposition? delta = JsonConvert.DeserializeObject<Disposition>(e.Delta);
                if (((!String.IsNullOrEmpty(delta?.FinalDisposition) && delta?.FinalDispositionDate != null) || delta?.InvalidMatch == true) && !statesReadyToClose.Contains(e.ActorState))
                {
                    statesReadyToClose.Add(e.ActorState);
                    return true;
                }
                return false;
            }).ToList();
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
