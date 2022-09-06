
async function validateForm(event) {
    const valid = await event.target.dotNetReference.invokeMethodAsync('ValidateForm');
    await event.target.dotNetReference.invokeMethodAsync('PresubmitForm');
    if (valid) {
        event.target.submit();
    }
}
export function RegisterFormValidation(formId, dotNetReference) {
    const form = document.getElementById(formId);
    form.dotNetReference = dotNetReference;
    form.addEventListener('submit', validateForm);
}