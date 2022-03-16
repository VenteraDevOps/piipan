﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Components.Forms
{
    public partial class UsaForm
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        private bool HasErrors => currentErrors?.Count() > 0;
        private EditContext editContext;
        private bool ShowAlertBox { get; set; } = false;

        public List<UsaFormGroup> FormGroups { get; set; } = new List<UsaFormGroup>();
        private List<(UsaFormGroup FormGroup, IEnumerable<string> Errors)> currentErrors = new List<(UsaFormGroup FormGroup, IEnumerable<string> Errors)>();

        /// <summary>
        /// Set the edit context of this form when it's initialized
        /// </summary>
        protected override void OnInitialized()
        {
            editContext = new EditContext(Model);
        }

        /// <summary>
        /// Update the state of the form, such as after a field changes its value
        /// </summary>
        public void UpdateState()
        {
            currentErrors.Clear();
            foreach (var formGroup in FormGroups)
            {
                if (formGroup.ValidationMessages.Any())
                {
                    currentErrors.Add((formGroup, formGroup.ValidationMessages));
                }
            }
            StateHasChanged();
        }
        
        private async Task SubmitForm()
        {
            currentErrors.Clear();
            foreach (var formGroup in FormGroups)
            {
                await formGroup.GetValidationErrorsAsync(editContext);
            }
            UpdateState();
            ShowAlertBox = currentErrors.Count != 0;

            if (OnSubmit != null)
            {
                await OnSubmit(currentErrors.Count == 0);
            }
        }

        private async Task FocusElement(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("piipan.utilities.focusElement", elementId);
        }
    }
}
