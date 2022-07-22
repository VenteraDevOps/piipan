using FluentValidation.TestHelper;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Validators;
using Xunit;


namespace Piipan.Match.Core.Tests.Validators
{
    public class AddEventRequestValidatorTests
    {
        public AddEventRequestValidator Validator()
        {
            return new AddEventRequestValidator();
        }

        [Fact]
        public void ReturnsErrorWhenDataEmpty()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = null
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data);
        }

        #region InitialActionAt

        [Fact]
        public void ReturnsErrorWhen_InitialActionAtIsEmpty_And_InitialActionTakenIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    InitialActionTaken = "Notice Sent"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionAt);
        }

        [Fact]
        public void ReturnsErrorWhen_InitialActionAtIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionAt);
        }

        #endregion InitialActionAt
        #region InitialActionTaken
        [Fact]
        public void ReturnsErrorWhen_InitialActionTakenIsEmpty_And_InitialActionAtIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    InitialActionAt = System.DateTime.Now
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionTaken);
        }

        [Fact]
        public void ReturnsErrorWhen_InitialActionTakenIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.InitialActionTaken);
        }
        #endregion InitialActionTaken
        #region FinalDispositionDate
        [Fact]
        public void ReturnsErrorWhen_FinalDispositionDateIsEmpty_And_FinalDispositionIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    FinalDisposition = "Benefits Denied"
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.FinalDispositionDate);
        }
        #endregion FinalDispositionDate
        #region FinalDisposition
        [Fact]
        public void ReturnsErrorWhen_FinalDispositionIsEmpty_And_FinalDispositionDateIsNot()
        {
            // Setup
            var model = new AddEventRequest()
            {
                Data = new AddEventRequestData
                {
                    FinalDispositionDate = System.DateTime.Now
                }
            };
            // Act
            var result = Validator().TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(result => result.Data.FinalDisposition);
        }
        #endregion FinalDisposition
    }
}
