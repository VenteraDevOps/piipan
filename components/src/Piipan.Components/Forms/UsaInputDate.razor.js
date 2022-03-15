export function doesDateHaveBadInput(element) {
    return element?.validity?.badInput || false;
}