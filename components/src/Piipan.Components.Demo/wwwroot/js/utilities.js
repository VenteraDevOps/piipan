function getCursorPosition(element) {
    return element.selectionStart;
}
function piipan_SetCursorPosition(element, position) {
    return element.setSelectionRange(position, position);
}
function piipan_SetValue(element, value) {
    element.value = value;
}

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
}());