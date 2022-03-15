using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Components.Forms
{
    public partial class UsaForm
    {
        private bool HasErrors => currentErrors?.Count() > 0;
        private EditContext editContext;
        private bool ShowAlertBox { get; set; } = false;
        protected override void OnInitialized()
        {
            editContext = new EditContext(Model);

            editContext.EnableDataAnnotationsValidation();
        }

        public List<UsaFormGroup> FormGroups { get; set; } = new();

        private List<(UsaFormGroup FormGroup, IEnumerable<string> Errors)> currentErrors = new();

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
        async Task SubmitForm()
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
    }
}
