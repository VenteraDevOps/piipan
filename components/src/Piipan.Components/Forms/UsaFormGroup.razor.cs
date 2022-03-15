﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Piipan.Components.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Components.Forms
{
    public partial class UsaFormGroup : IDisposable
    {
        // prefix with a letter so it's a valid ID
        public string GroupId { get; set; } = "g" + Guid.NewGuid().ToString();
        public bool Required { get; set; }
        public string Label { get; set; }
        public FieldIdentifier FieldIdentifier { get; set; }
        public InputStatus Status { get; private set; } = InputStatus.None;
        public ElementReference? InputElement { get; set; }

        public List<string> ValidationMessages { get; set; } = new List<string>();
        public Func<Task<List<string>>> PreverificationChecks { get; set; } = null;

        private bool HasErrors => ValidationMessages?.Count > 0;

        /// <summary>
        /// Get all of the errors associated with this form group. First do prevalidation checks, and then if they pass validate the whole edit context.
        /// </summary>
        /// <param name="editContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// When this form group is initialized, add this form group to the form
        /// </summary>
        protected override void OnInitialized()
        {
            Form.FormGroups.Add(this);
        }

        /// <summary>
        /// When this form group is disposed, remove it from the form
        /// </summary>
        public void Dispose()
        {
            Form.FormGroups.Remove(this);
        }
    }
}
