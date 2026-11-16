import { sidesFor, coordinates, overflow } from './SUIPopover.razor.js';
const instances = new WeakMap();

export function sync(anchor, panel, reference, options) {
    if (!(anchor instanceof Element) || !(panel instanceof HTMLElement)) return;
    let state = instances.get(panel);
    if (!state) {
        state = { options, reference, open: false, frame: 0 };
        instances.set(panel, state);
        state.place = () => {
            if (!panel.matches(':popover-open')) return;
            const margin = 12;
            panel.style.maxWidth = `${Math.min(state.options.maxWidth, Math.max(0, innerWidth - margin * 2))}px`;
            const rect = anchor.getBoundingClientRect(), size = panel.getBoundingClientRect();
            let best;
            for (const side of sidesFor(state.options.placement)) {
                const position = coordinates(side, rect, size.width, size.height, state.options.offset);
                const score = overflow(position, size, margin);
                if (!best || score < best.score) best = { ...position, score };
                if (score === 0) break;
            }
            panel.style.left = `${Math.max(margin, Math.min(best.left, innerWidth - size.width - margin))}px`;
            panel.style.top = `${Math.max(margin, Math.min(best.top, innerHeight - size.height - margin))}px`;
        };
        state.schedule = () => {
            if (!state.open || state.frame) return;
            state.frame = requestAnimationFrame(() => { state.frame = 0; state.place(); });
        };
        state.toggle = event => {
            if (event.newState === 'closed' && state.open) {
                state.open = false;
                state.reference.invokeMethodAsync('DismissAsync').catch(() => {});
            }
        };
        state.key = event => {
            if (event.key !== 'Escape' || event.target.closest('[popover]:popover-open') !== panel) return;
            event.preventDefault(); event.stopPropagation();
            panel.hidePopover();
            anchor.querySelector('button, a[href], input, [tabindex]')?.focus({ preventScroll: true });
        };
        panel.addEventListener('keydown', state.key, true);
        panel.addEventListener('toggle', state.toggle);
        window.addEventListener('resize', state.schedule);
        window.addEventListener('scroll', state.schedule, true);
        state.observer = new ResizeObserver(state.schedule);
        state.observer.observe(anchor);
        state.observer.observe(panel);
    }
    state.options = options;
    state.reference = reference;
    state.open = options.open;
    if (options.open) {
        if (!panel.matches(':popover-open')) panel.showPopover({ source: anchor.querySelector('button, a[href], input, [tabindex]') });
        state.place();
    } else if (panel.matches(':popover-open')) panel.hidePopover();
}

export function disconnect(panel) {
    const state = instances.get(panel);
    if (!state) return;
    state.open = false;
    cancelAnimationFrame(state.frame);
    state.observer.disconnect();
    panel.removeEventListener('keydown', state.key, true);
    panel.removeEventListener('toggle', state.toggle);
    window.removeEventListener('resize', state.schedule);
    window.removeEventListener('scroll', state.schedule, true);
    if (panel.matches(':popover-open')) panel.hidePopover();
    instances.delete(panel);
}
