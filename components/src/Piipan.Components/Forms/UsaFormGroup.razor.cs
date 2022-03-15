using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Piipan.Components.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Components.Forms
{
    public partial class UsaFormGroup
    {
        // prefix with a letter so it's a valid ID
        public string GroupId { get; set; } = "g" + Guid.NewGuid().ToString();
        public bool HasErrors => ValidationMessages?.Count > 0;
        public bool Required { get; set; }
        public string Label { get; set; }
        public FieldIdentifier FieldIdentifier { get; set; }
        public InputStatus Status { get; private set; } = InputStatus.None;
        public ElementReference? InputElement { get; set; }

        public List<string> ValidationMessages { get; set; } = new List<string>();
        public Func<Task<List<string>>> PreverificationChecks = null;

        public async Task GetValidationErrorsAsync(EditContext editContext)
        {
            List<string> preverficiationErrors = PreverificationChecks == null ? null : (await PreverificationChecks());
            if (preverficiationErrors?.Count > 0)
            {
                ValidationMessages = preverficiationErrors;
            }
            else
            {
                editContext.Validate();
                ValidationMessages = editContext.GetValidationMessages(FieldIdentifier).ToList();
            }
            StateHasChanged();
            Status = HasErrors ? InputStatus.Error : InputStatus.None;
            Form.UpdateState();
        }
        protected override void OnInitialized()
        {
            Form.FormGroups.Add(this);
        }
    }
}
