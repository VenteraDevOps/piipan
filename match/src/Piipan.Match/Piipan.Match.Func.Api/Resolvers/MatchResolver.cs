using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Piipan.Match.Func.Api.Extensions;
using Piipan.Participants.Api;

namespace Piipan.Match.Func.Api.Resolvers
{
    public class MatchResolver : IMatchResolver
    {
        private readonly IParticipantApi _participantApi;
        private readonly IValidator<RequestPerson> _requestPersonValidator;

        public MatchResolver(
            IParticipantApi participantApi,
            IValidator<RequestPerson> requestPersonValidator)
        {
            _participantApi = participantApi;
            _requestPersonValidator = requestPersonValidator;
        }

        public async Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request)
        {
            var response = new OrchMatchResponse();
            for (int i = 0; i < request.Data.Count; i++)
            {
                var person = request.Data[i];
                var personValidation = await _requestPersonValidator.ValidateAsync(person);
                if (personValidation.IsValid)
                {
                    var result = await PersonMatch(request.Data[i], i);
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

        private async Task<OrchMatchResult> PersonMatch(RequestPerson person, int index)
        {
            var states = await _participantApi.GetStates();

            var matches = await states
                .SelectManyAsync(state => _participantApi.GetParticipants(state, person.LdsHash));

            return new OrchMatchResult
            {
                Index = index,
                Matches = matches
            };
        }
    }
}