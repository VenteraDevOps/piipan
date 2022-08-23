using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Metrics.Api;
using Piipan.Notifications.Models;
using Piipan.Notifications.Services;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Enums;
using Piipan.Shared.Cryptography;
using Piipan.States.Core.DataAccessObjects;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for resolving match events and match records
    /// </summary>
    public class MatchEventService : IMatchEventService
    {
        private readonly IActiveMatchRecordBuilder _recordBuilder;
        private readonly IMatchRecordApi _recordApi;
        private readonly IMatchResEventDao _matchResEventDao;
        private readonly IMatchResAggregator _matchResAggregator;
        private readonly ICryptographyClient _cryptographyClient;
        private readonly IParticipantPublishSearchMetric _participantPublishSearchMetric;
        private readonly IParticipantPublishMatchMetric _participantPublishMatchMetric;
        private readonly IStateInfoDao _stateInfoDao;
        private readonly INotificationService _notificationService;

        public MatchEventService(
            IActiveMatchRecordBuilder recordBuilder,
            IMatchRecordApi recordApi,
            IMatchResEventDao matchResEventDao,
            IMatchResAggregator matchResAggregator,
            ICryptographyClient cryptographyClientt,
            IParticipantPublishSearchMetric ParticipantPublishSearchMetric,
            IParticipantPublishMatchMetric participantPublishMatchMetric,
            IStateInfoDao stateInfoDao,
            INotificationService notificationService)
        {
            _recordBuilder = recordBuilder;
            _recordApi = recordApi;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _cryptographyClient = cryptographyClientt;
            _participantPublishSearchMetric = ParticipantPublishSearchMetric;
            _participantPublishMatchMetric = participantPublishMatchMetric;
            _stateInfoDao = stateInfoDao;
            _notificationService = notificationService;

        }

        /// <summary>
        /// Evaluates each new match in the incoming `matchRespone` against any existing matching records.
        /// If an open match record for the match exists, it is reused. Else, a new match record is created.
        /// Each match is subsequently updated to include the resulting `match_id`.
        /// </summary>
        /// <param name="request">The OrchMatchRequest instance derived from the incoming match request</param>
        /// <param name="matchResponse">The OrchMatchResponse instance returned from the match API</param>
        /// <param name="initiatingState">The two-letter postal abbreviation for the state initiating the match request</param>
        /// <returns>The updated `matchResponse` object with `match_id`s</returns>
        public async Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState)
        {
            matchResponse.Data.Results = (await Task.WhenAll(matchResponse.Data.Results.Select(result =>
                ResolvePersonMatches(
                    request.Data.ElementAt(result.Index),
                    result,
                    initiatingState))))
                .OrderBy(result => result.Index)
                .ToList();
            //Build Search Metrics
            ParticipantSearchMetrics participantSearchMetrics = new ParticipantSearchMetrics();
            participantSearchMetrics.Data = new List<ParticipantSearch>();
            foreach (OrchMatchResult requestPerson in matchResponse.Data.Results)
            {
                var participantUploadMetrics = new ParticipantSearch()
                {
                    State = initiatingState,
                    SearchedAt = DateTime.UtcNow,
                    SearchFrom = String.Empty,//Need to Identify Website/Api call
                    SearchReason = request.Data.ElementAt(requestPerson.Index).SearchReason,
                    MatchCreation = String.IsNullOrEmpty(String.Join(",", requestPerson.Matches.Select(p => p.MatchCreation))) ? EnumHelper.GetDisplayName(SearchMatchStatus.MATCHNOTFOUND) : String.Join(",", requestPerson.Matches.Select(p => p.MatchCreation)),
                    MatchCount = requestPerson.Matches.Count()
                };
                participantSearchMetrics.Data.Add(participantUploadMetrics);
            }
            await _participantPublishSearchMetric.PublishSearchdMetric(participantSearchMetrics);
            return matchResponse;
        }

        private async Task<OrchMatchResult> ResolvePersonMatches(RequestPerson person, OrchMatchResult result, string initiatingState)
        {
            // Create a match <-> match record pairing
            var pairs = result.Matches.Select(match =>
                new
                {
                    match,
                    person,
                    record = _recordBuilder
                                .SetMatch(person, match)
                                .SetStates(initiatingState, match.State)
                                .GetRecord()
                });

            result.Matches = (await Task.WhenAll(
                pairs.Select(pair => ResolveSingleMatch(pair.match, pair.record, pair.person))));

            return result;
        }

        private async Task<ParticipantMatch> ResolveSingleMatch(IParticipant match, IMatchRecord record, RequestPerson person)
        {
            var existingRecords = await _recordApi.GetRecords(record);
            ParticipantMatch participantMatchRecord = new ParticipantMatch();
            participantMatchRecord.MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.MATCHNOTFOUND);
            if (existingRecords.Any())
            {
                participantMatchRecord = await Reconcile(match, record, existingRecords);
                participantMatchRecord.MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.EXISTINGMATCH);
            }
            else
            {
                // No existing records
                participantMatchRecord = new ParticipantMatch(match)
                {
                    MatchId = await _recordApi.AddRecord(record),
                    MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.NEWMATCH)
                };
                // New Match is created.  Create new Match entry in the Metrics database
                //Build Search Metrics
                var participantMatchMetrics = new ParticipantMatchMetrics()
                {
                    MatchId = participantMatchRecord.MatchId,
                    InitState = record.Initiator,
                    MatchingState = match.State,
                    MatchingStateVulnerableIndividual = match.VulnerableIndividual,
                    InitStateVulnerableIndividual = person.VulnerableIndividual, // getting VulnerableIndividual from iniator 
                    Status = MatchRecordStatus.Open

                };
                await _participantPublishMatchMetric.PublishMatchMetric(participantMatchMetrics);

                //Send Notification to both initiating state and Matching State.
                // Send template data for any email template which is created based on the requirements.
                // The below logic might change based on the template data for requirements.
                //In future we might end up consolidating the logic based on requirements.
                var states = await _stateInfoDao.GetStates();
                var initState = states?.Where(n => string.Compare(n.StateAbbreviation, record.Initiator, true) == 0).FirstOrDefault();
                var matchingState = states?.Where(n => string.Compare(n.StateAbbreviation, match.State, true) == 0).FirstOrDefault();
                var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");

                EmailTemplateInput emailTemplateInputIs = GetEmailTemplate(participantMatchRecord.MatchId, initState?.State, matchingState?.State, queryToolUrl, initState?.Email);
                emailTemplateInputIs.Topic = "CREATE_MATCH_IS";
                await _notificationService.PublishMessageFromTemplate(emailTemplateInputIs); //Publishing Email for Initiating State:  Based on the requirement

                EmailTemplateInput emailTemplateInputMs = GetEmailTemplate(participantMatchRecord.MatchId, initState?.State, matchingState?.State, queryToolUrl, matchingState?.Email);
                emailTemplateInputMs.Topic = "CREATE_MATCH_MS";
                await _notificationService.PublishMessageFromTemplate(emailTemplateInputMs); //Publishing Email for Matching State : Based on the requirement

            }
            if (participantMatchRecord != null)
            {
                var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");
                participantMatchRecord.MatchUrl = $"{queryToolUrl}/match/{participantMatchRecord.MatchId}";
            }
            return participantMatchRecord;
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

        private async Task<ParticipantMatch> Reconcile(IParticipant match, IMatchRecord pendingRecord, IEnumerable<IMatchRecord> existingRecords)
        {
            var latest = existingRecords.OrderBy(r => r.CreatedAt).Last();

            var events = await _matchResEventDao.GetEvents(latest.MatchId);
            var matchResRecord = _matchResAggregator.Build(latest, events);

            if (matchResRecord.Status == MatchRecordStatus.Closed)
            {
                return new ParticipantMatch(match)
                {
                    MatchId = await _recordApi.AddRecord(pendingRecord)
                };
            }

            // Latest record is open, return its match ID
            return new ParticipantMatch(match)
            {
                MatchId = latest.MatchId
            };

        }
    }
}
