const controllers = new WeakMap();

export function disconnect(input) {
    controllers.get(input)?.abort();
    controllers.delete(input);
}

export function configure(input, enabled) {
    if (!enabled) { disconnect(input); return; }
    if (controllers.has(input)) return;
    const controller = new AbortController();
    controllers.set(input, controller);
    input.addEventListener('wheel', event => {
        if (document.activeElement !== input || input.disabled || input.readOnly
            || event.ctrlKey || event.metaKey || !event.deltaY || input.step === 'any') return;
        const previous = input.value;
        // Native stepping preserves decimal precision, step alignment and min/max.
        if (event.deltaY < 0) input.stepUp();
        else input.stepDown();
        event.preventDefault();
        if (input.value === previous) return;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }, { passive: false, signal: controller.signal });
}
