const controllers = new WeakMap();

export function disconnect(input) {
    controllers.get(input)?.controller.abort();
    controllers.delete(input);
}

export function configure(input, enabled, wheelStep = null, arrowKeysEnabled = true, arrowKeyStep = null, spinnerStep = null) {
    if (!enabled && wheelStep == null && arrowKeysEnabled && arrowKeyStep == null && spinnerStep == null) { disconnect(input); return; }
    const existing = controllers.get(input);
    if (existing) {
        Object.assign(existing, { enabled, wheelStep, arrowKeysEnabled, arrowKeyStep, spinnerStep });
        return;
    }
    const controller = new AbortController();
    const state = { controller, enabled, wheelStep, arrowKeysEnabled, arrowKeyStep, spinnerStep };
    controllers.set(input, state);

    const isEditable = () => document.activeElement === input && !input.matches(':disabled') && !input.readOnly;
    const increment = (direction, configuredStep) => {
        const previous = input.value;
        if (configuredStep != null) {
            const step = Number(configuredStep);
            const current = input.valueAsNumber;
            if (!Number.isFinite(current) || !Number.isFinite(step) || step <= 0) return;
            // Preserve centavos rather than snapping to a multiple of the interaction increment.
            let next = Number((current + direction * step).toFixed(10));
            if (input.min !== '') next = Math.max(Number(input.min), next);
            if (input.max !== '') next = Math.min(Number(input.max), next);
            input.value = String(next);
        } else {
            if (input.step === 'any') return;
            if (direction > 0) input.stepUp();
            else input.stepDown();
        }
        if (input.value === previous) return;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    };

    // Explicit controls avoid native step snapping/validation changing cents.
    // Delegation also handles spinner buttons added/removed by later renders.
    input.parentElement.addEventListener('click', event => {
        const button = event.target.closest('button[data-numeric-direction]');
        if (!button || !input.parentElement.contains(button) || state.spinnerStep == null
            || button.disabled || input.matches(':disabled') || input.readOnly) return;
        increment(Number(button.dataset.numericDirection), state.spinnerStep);
    }, { signal: controller.signal });

    input.addEventListener('wheel', event => {
        if (!isEditable() || event.ctrlKey || event.metaKey || !event.deltaY) return;
        // A keyboard-only controller must not change legacy wheel behavior.
        if (!state.enabled && state.wheelStep == null) return;
        if (state.enabled && state.wheelStep == null && input.step === 'any') return;
        event.preventDefault();
        if (state.enabled) increment(event.deltaY < 0 ? 1 : -1, state.wheelStep);
    }, { passive: false, signal: controller.signal });

    input.addEventListener('keydown', event => {
        if (!isEditable() || (event.key !== 'ArrowUp' && event.key !== 'ArrowDown') || event.isComposing) return;
        if (!state.arrowKeysEnabled) { event.preventDefault(); return; }
        if (state.arrowKeyStep == null || event.ctrlKey || event.metaKey || event.altKey) return;
        event.preventDefault();
        increment(event.key === 'ArrowUp' ? 1 : -1, state.arrowKeyStep);
    }, { signal: controller.signal });
}
