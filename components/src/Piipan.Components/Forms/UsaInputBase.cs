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

        /// <summary>
        /// When this input is created, update the form group's properties that are dependant on field
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            FormGroup.PreverificationChecks = PreverificationChecks;
            FormGroup.FieldIdentifier = this.FieldIdentifier;
            
            FormGroup.Label = ValueExpression.GetAttribute<T, DisplayAttribute>()?.Name ?? ValueExpression.Name;
            FormGroup.Required = ValueExpression.HasAttribute<T, RequiredAttribute>();
        }

        /// <summary>
        /// After this renders the first time, set the FormGroup's input element to this so that it can be focused if an error occurs.
        /// You cannot do this in the OnInitialized, since the ElementReference doesn't exist yet.
        /// </summary>
        /// <param name="firstRender">Whether or not this component is rendering for the first time</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                FormGroup.InputElement = ElementReference;
            }
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
