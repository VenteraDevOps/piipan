using FluentValidation;
using Piipan.Match.Api.Models;

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

            When(x => x.Data != null, () =>
            {

                RuleFor(r => r.Data.InitialActionAt).Custom((val, context) =>
                {
                    if (val == null && !string.IsNullOrEmpty(context.InstanceToValidate.Data.InitialActionTaken))
                    {
                        context.AddFailure("Initial Action Date is required");
                    }
                    else if (val == null && !string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                    {
                        context.AddFailure("Initial Action Date is required because a Final Disposition has been selected");
                    }
                });

                RuleFor(r => r.Data.InitialActionTaken).Custom((val, context) =>
                {
                    if (string.IsNullOrEmpty(val) && context.InstanceToValidate.Data.InitialActionAt != null)
                    {
                        context.AddFailure("Initial Action Taken is required because a date has been selected");
                    }
                    else if (string.IsNullOrEmpty(val) && !string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                    {
                        context.AddFailure("Initial Action Taken is required because a Final Disposition has been selected");
                    }
                });

                RuleFor(r => r.Data.FinalDispositionDate).Custom((val, context) =>
                {
                    if (val == null && !string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                    {
                        context.AddFailure("Final Disposition Date is required");
                    }
                });

                RuleFor(r => r.Data.FinalDisposition).Custom((val, context) =>
                {
                    if (string.IsNullOrEmpty(val) && context.InstanceToValidate.Data.FinalDispositionDate != null)
                    {
                        context.AddFailure("Final Disposition Taken is required because a date has been selected");
                    }
                });
            });
        }
    }
}
