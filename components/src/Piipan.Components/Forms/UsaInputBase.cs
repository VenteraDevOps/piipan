using Microsoft.AspNetCore.Components.Forms;
using Piipan.Components.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piipan.Components.Enums;
using Microsoft.AspNetCore.Components;

namespace Piipan.Components.Forms
{
    /// <summary>
    /// This is the base Input class, from which other inputs can be derived. There is interaction with the FormGroup for things like Validation and Label creation
    /// </summary>
    /// <typeparam name="T">The type that the value of this component will be bound to</typeparam>
    public class UsaInputBase<T> : InputBase<T>
    {
        [CascadingParameter]
        public UsaFormGroup FormGroup { get; set; }

        [Parameter]
        public virtual int? Width { get; set; }

        protected ElementReference? ElementReference { get; set; }

        protected string Id { get => $"{FormGroup?.GroupId}-input"; }

        /// <summary>
        /// When this input is created, update the form group's properties that are dependant on field
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            FormGroup.PreverificationChecks = PreverificationChecks;
            FormGroup.FieldIdentifier = this.FieldIdentifier;
            FormGroup.InputElementId = Id;
            FormGroup.Label = ValueExpression.GetAttribute<T, DisplayAttribute>()?.Name ?? ValueExpression.Name;
            FormGroup.Required = ValueExpression.HasAttribute<T, RequiredAttribute>();
        }

        /// <summary>
        /// By default, there are no preverfication checks that fail. This can be overridden.
        /// </summary>
        protected virtual Task<List<string>> PreverificationChecks()
        {
            return Task.FromResult<List<string>>(null);
        }

        /// <summary>
        /// When the field blurs, fire off validation
        /// </summary>
        protected async Task BlurField()
        {
            if (FormGroup != null)
            {
                await FormGroup.GetValidationErrorsAsync(EditContext);
                StateHasChanged();
            }
        }

        protected override bool TryParseValueFromString(string value, [MaybeNullWhen(false)] out T result, [NotNullWhen(false)] out string validationErrorMessage)
        {
            result = default;
            validationErrorMessage = null;
            return true;
        }
    }
}
