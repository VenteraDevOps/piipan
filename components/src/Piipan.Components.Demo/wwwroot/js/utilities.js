(function piipanUtilities() {
    if (!window.piipan) {
        window.piipan = {};
    }
    if (!window.piipan.utilities) {
        window.piipan.utilities = {};
    }
    window.piipan.utilities.getCursorPosition = (element) => {
        return element.selectionStart;
    }
    window.piipan.utilities.setCursorPosition = (element, position) => {
        element.setSelectionRange(position, position);
    }
    window.piipan.utilities.setValue = (element, value) => {
        element.value = value;
    }
    window.piipan.utilities.focusElement = (id) => {
        document.getElementById(id)?.focus();
    }
    window.piipan.utilities.doesElementHaveInvalidInput = (id) => {
        return document.getElementById(id)?.validity?.badInput || false;
    }
    window.piipan.utilities.validateForm = (form) => {
        window.setTimeout(() => {
            form.submit();
        }, 2000)
    }
    window.piipan.utilities.registerFormValidation = (form) => {
        form.addEventListener('submit', document.getElementById(form));
    }
}());