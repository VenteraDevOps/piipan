using Piipan.Match.Api.Models;
using FluentValidation;
using Piipan.Shared.API.Enums;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class RequestPersonValidator : AbstractValidator<RequestPerson>
    {
        public RequestPersonValidator()
        {
            const string HashRegex = "^[a-z0-9]{128}$";
            const string AlphanumericRegex = "^[A-Za-z0-9]+$";

            RuleFor(q => q.LdsHash).Matches(HashRegex);
            RuleFor(q => q.ParticipantId).Matches(AlphanumericRegex);
            RuleFor(q => q.ParticipantId).MaximumLength(20).WithName("Participant Id");
            RuleFor(x => x.SearchReason).IsEnumName(typeof(ValidSearchReasons));
        }
    }
}