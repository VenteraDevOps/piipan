export function getCursorPosition(element) {
    return element.selectionStart;
}
export function setCursorPosition(element, position) {
    return element.setSelectionRange(position, position);
}
export function setValue(element, value) {
    element.value = value;
}