using Piipan.Match.Api.Models;
using FluentValidation;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates the API match rsolution Add Event request from a client
    /// </summary>
    public class AddEventRequestValidator : AbstractValidator<AddEventRequest>
    {
        public AddEventRequestValidator()
        {
            RuleFor(r => r.Data)
                .NotNull()
                .NotEmpty();
        }
    }
}
