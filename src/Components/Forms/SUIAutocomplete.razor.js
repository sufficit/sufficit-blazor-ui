export { openSelectMenu, closeSelectMenu, revealActiveOption } from './SUISelect.razor.js?v=2';
const listeners = new WeakMap();
export function connect(input) {
    if (!input?.isConnected || listeners.has(input)) return;
    const keydown = event => {
        if (event.isComposing || event.ctrlKey || event.metaKey || event.altKey) return;
        const open = input.getAttribute('aria-expanded') === 'true';
        const active = input.getAttribute('aria-activedescendant');
        if ((open && active && ['ArrowDown','ArrowUp','Home','End','Enter'].includes(event.key))
            || (open && event.key === 'Escape')) event.preventDefault();
    };
    input.addEventListener('keydown', keydown);
    listeners.set(input, keydown);
}
export function disconnect(input) {
    const listener = listeners.get(input);
    if (listener) input.removeEventListener('keydown', listener);
    listeners.delete(input);
}
