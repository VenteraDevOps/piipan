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
    public class UsaInputBase<T> : InputBase<T>
    {
        [CascadingParameter]
        public UsaFormGroup FormGroup { get; set; }

        [Parameter]
        public virtual int? Width { get; set; }

        protected ElementReference? ElementReference { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            FormGroup.PreverificationChecks = PreverificationChecks;
            FormGroup.FieldIdentifier = this.FieldIdentifier;
            
            FormGroup.Label = ValueExpression.GetAttribute<T, DisplayAttribute>()?.Name ?? ValueExpression.Name;
            FormGroup.Required = ValueExpression.HasAttribute<T, RequiredAttribute>();
        }

        protected virtual Task<List<string>> PreverificationChecks()
        {
            return Task.FromResult<List<string>>(null);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                FormGroup.InputElement = ElementReference;
            }
        }

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
