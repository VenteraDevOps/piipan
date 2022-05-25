using Piipan.Match.Api.Models;
using FluentValidation;
using Piipan.Match.Core.Enums;

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

            RuleFor(q => q.LdsHash).Matches(HashRegex);
            RuleFor(x => x.SearchReason).IsEnumName(typeof(ValidSearchReasons));
        }
    }
}