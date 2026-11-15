// Shared geometry; callers retain placement order and margins.
export function sidesFor(preferred, verticalFallback = 'left') {
    const horizontal = verticalFallback === 'right' ? ['right', 'left'] : ['left', 'right'];
    switch (preferred) {
        case 'left': return ['left', 'right', 'top', 'bottom'];
        case 'top': return ['top', 'bottom', ...horizontal];
        case 'bottom': return ['bottom', 'top', ...horizontal];
        default: return ['right', 'left', 'top', 'bottom'];
    }
}

export function coordinates(side, rect, width, height, gap) {
    const left = rect.left + ((rect.width - width) / 2);
    const top = rect.top + ((rect.height - height) / 2);
    switch (side) {
        case 'left': return { left: rect.left - width - gap, top };
        case 'top': return { left, top: rect.top - height - gap };
        case 'bottom': return { left, top: rect.bottom + gap };
        default: return { left: rect.right + gap, top };
    }
}

export function overflow(position, rect, margin) {
    return Math.max(0, margin - position.left)
        + Math.max(0, position.left + rect.width + margin - window.innerWidth)
        + Math.max(0, margin - position.top)
        + Math.max(0, position.top + rect.height + margin - window.innerHeight);
}

export function parseNumber(value, fallback, minimum, maximum) {
    const parsed = Number.parseFloat(value);
    return Number.isFinite(parsed)
        ? Math.min(maximum, Math.max(minimum, parsed))
        : fallback;
}


// Each Blazor-rendered panel owns its state; the top layer prevents clipping.

const instances = new WeakMap();

export function connectPopover(anchor, panel, options) {
    // Element references can expire during Blazor rerenders.
    if (!(anchor instanceof Element) || !(panel instanceof Element)) {
        return false;
    }
    disconnectPopover(anchor);
    const state = {
        open: false,
        pinned: false,
        showTimer: 0,
        hideTimer: 0,
        listeners: [],
        viewportListeners: [],
    };
    instances.set(anchor, state);

    const gap = options?.offset ?? 10;
    const sides = sidesFor(options?.placement);
    const margin = 8;
    // Only interactive panels use the top layer.
    const useTopLayer = options?.clickToToggle && typeof panel.showPopover === 'function';
    if (useTopLayer) panel.setAttribute('popover', 'manual');

    const on = (target, type, handler, opts) => {
        target.addEventListener(type, handler, opts);
        state.listeners.push([target, type, handler, opts]);
    };

    const onViewport = (target, type, handler) => {
        target.addEventListener(type, handler, { capture: true, passive: true });
        state.viewportListeners.push([target, type, handler, { capture: true, passive: true }]);
    };

    const offViewport = () => {
        for (const [target, type, handler, opts] of state.viewportListeners) {
            target.removeEventListener(type, handler, opts);
        }
        state.viewportListeners = [];
    };

    function place() {
        const anchorRect = anchor.getBoundingClientRect();
        if (useTopLayer) {
            panel.style.right = 'auto';
            panel.style.bottom = 'auto';
            panel.style.margin = '0';
        }
        panel.style.left = '0px';
        panel.style.top = '0px';
        const width = panel.offsetWidth;
        const height = panel.offsetHeight;

        let best = null;
        for (const side of sides) {
            const candidate = coordinates(side, anchorRect, width, height, gap);
            const overflow = Math.max(0, margin - candidate.left)
                + Math.max(0, candidate.left + width + margin - window.innerWidth)
                + Math.max(0, margin - candidate.top)
                + Math.max(0, candidate.top + height + margin - window.innerHeight);
            if (best === null || overflow < best.overflow) {
                best = { side, candidate, overflow };
            }
            if (overflow === 0) {
                break;
            }
        }

        const left = Math.min(Math.max(margin, best.candidate.left), Math.max(margin, window.innerWidth - width - margin));
        const top = Math.min(Math.max(margin, best.candidate.top), Math.max(margin, window.innerHeight - height - margin));
        panel.style.left = `${left}px`;
        panel.style.top = `${top}px`;
        panel.setAttribute('data-sui-popover-placement', best.side);
    }

    function show() {
        if (state.open) {
            return;
        }
        state.open = true;
        if (useTopLayer && !panel.matches(':popover-open')) panel.showPopover();
        panel.classList.add('sui-popover--open');
        panel.setAttribute('aria-hidden', 'false');
        place();
        onViewport(window, 'scroll', place);
        onViewport(window, 'resize', place);
    }

    function hide() {
        if (!state.open) {
            return;
        }
        state.open = false;
        state.pinned = false;
        clearTimeout(state.showTimer);
        clearTimeout(state.hideTimer);
        offViewport();
        panel.classList.remove('sui-popover--open');
        panel.setAttribute('aria-hidden', 'true');
        if (useTopLayer && panel.matches(':popover-open')) panel.hidePopover();
    }

    function scheduleShow() {
        clearTimeout(state.hideTimer);
        if (!state.open) {
            clearTimeout(state.showTimer);
            state.showTimer = setTimeout(show, options?.showDelay ?? 140);
        }
    }

    function scheduleHide() {
        if (state.pinned) return;
        clearTimeout(state.showTimer);
        if (state.open) {
            clearTimeout(state.hideTimer);
            state.hideTimer = setTimeout(hide, options?.hideDelay ?? 160);
        }
    }

    function onKeyDown(event) {
        if (event.key === 'Escape') hide();
    }

    state.cleanup = () => {
        clearTimeout(state.showTimer);
        clearTimeout(state.hideTimer);
        offViewport();
        if (useTopLayer && panel.matches(':popover-open')) panel.hidePopover();
    };

    if (options?.clickToToggle) {
        on(anchor, 'click', () => {
            clearTimeout(state.showTimer);
            clearTimeout(state.hideTimer);
            if (state.pinned) hide();
            else {
                state.pinned = true;
                show();
            }
        });
        on(document, 'pointerdown', (event) => {
            if (state.pinned && !anchor.contains(event.target) && !panel.contains(event.target)) hide();
        });
    }

    on(anchor, 'mouseenter', scheduleShow);
    on(anchor, 'mouseleave', scheduleHide);
    on(anchor, 'focusin', scheduleShow);
    on(anchor, 'focusout', scheduleHide);
    // Moving onto the panel must keep it open.
    on(panel, 'mouseenter', () => clearTimeout(state.hideTimer));
    on(panel, 'mouseleave', scheduleHide);
    on(document, 'keydown', onKeyDown);
    return true;
}

export function disconnectPopover(anchor) {
    const state = anchor instanceof Element ? instances.get(anchor) : undefined;
    if (!state) {
        return;
    }
    for (const [target, type, handler, opts] of state.listeners) {
        target.removeEventListener(type, handler, opts);
    }
    state.listeners = [];
    state.cleanup();
    state.open = false;
    instances.delete(anchor);
}
