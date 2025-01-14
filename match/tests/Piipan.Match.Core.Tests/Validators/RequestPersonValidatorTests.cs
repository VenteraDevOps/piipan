using Piipan.Match.Api.Models;
using Piipan.Match.Core.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Piipan.Match.Core.Tests.Validators
{
    public class RequestPersonValidatorTests
    {
        public RequestPersonValidator Validator()
        {
            return new RequestPersonValidator();
        }

        [Fact]
        public void ReturnsErrorWhenHashEmpty()
        {
            var model = new RequestPerson()
            {
                LdsHash = ""
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.LdsHash);
        }

        [Fact]
        public void ReturnsErrorWhenHashMalformed()
        {
            var model = new RequestPerson()
            {
                LdsHash = "Foo"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.LdsHash);
        }

        [Fact]
        public void ReturnsErrorWhenSearchReasonMalformed()
        {
            var model = new RequestPerson()
            {
                SearchReason = "Foo"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.SearchReason);
        }
        [Fact]
        public void ReturnsErrorWhenParticipantIdEmpty()
        {
            var model = new RequestPerson()
            {
                ParticipantId = ""
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.ParticipantId);
        }
        [Fact]
        public void ReturnsErrorWhenParticipantIdLengthExceed()
        {
            var model = new RequestPerson()
            {
                ParticipantId = "1234567890098765432101"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.ParticipantId);
        }
    }
}