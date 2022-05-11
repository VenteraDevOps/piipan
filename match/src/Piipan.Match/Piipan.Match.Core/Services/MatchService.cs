using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Extensions;
using Piipan.Participants.Api;
using Piipan.Match.Core.Enums;
using System;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for discovering participant matches between states
    /// </summary>
    public class MatchService : IMatchApi
    {
        private readonly IParticipantApi _participantApi;
        private readonly IValidator<RequestPerson> _requestPersonValidator;

        /// <summary>
        /// Initializes a new instance of MatchService
        /// </summary>
        public MatchService(
            IParticipantApi participantApi,
            IValidator<RequestPerson> requestPersonValidator)
        {
            _participantApi = participantApi;
            _requestPersonValidator = requestPersonValidator;
        }

        /// <summary>
        /// Finds and returns matches for each participant in the request
        /// </summary>
        /// <param name="request">A collection of participants to attempt to find matches for</param>
        /// <returns>A collection of match results and inline errors for malformed participant requests</returns>
        public async Task<OrchMatchResponse> FindMatches(OrchMatchRequest request, string initiatingState)
        {
            var response = new OrchMatchResponse();
            for (int i = 0; i < request.Data.Count; i++)
            {
                var person = request.Data[i];
                var personValidation = await _requestPersonValidator.ValidateAsync(person);
                if (personValidation.IsValid)
                {
                    var result = await PersonMatch(request.Data[i], i, initiatingState);
                    response.Data.Results.Add(result);
                }
                else
                {
                    response.Data.Errors.AddRange(personValidation.Errors.Select(e =>
                    {
                        return new OrchMatchError
                        {
                            Index = i,
                            Code = e.ErrorCode,
                            Detail = e.ErrorMessage
                        };
                    }));
                }
            }

            return response;
        }

        private async Task<OrchMatchResult> PersonMatch(RequestPerson person, int index, string initiatingState)
        {
            var states = await _participantApi.GetStates();
            //Removing the initiatingState from the list of states for Match.
            states = states.Where(state => state != initiatingState);

            var matches = (await states
                .SelectManyAsync(state => _participantApi.GetParticipants(state, person.LdsHash)))
                .Select(p => new ParticipantMatch(p));

            return new OrchMatchResult
            {
                Index = index,
                Matches = matches
            };
        }
    }
}
