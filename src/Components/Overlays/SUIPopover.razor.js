// Rich popover: hover/focus surface for arbitrary content. Unlike the text tooltip
// (one shared element fed by data attributes), each SUIPopover owns its panel element
// because Blazor renders the content. Positioning is fixed, so no clipping ancestor
// between the anchor and the viewport can cut the panel off.

const instances = new WeakMap();

function sidesFor(preferred) {
    switch (preferred) {
        case 'left': return ['left', 'right', 'top', 'bottom'];
        case 'right': return ['right', 'left', 'top', 'bottom'];
        case 'top': return ['top', 'bottom', 'left', 'right'];
        case 'bottom': return ['bottom', 'top', 'left', 'right'];
        default: return ['right', 'left', 'top', 'bottom'];
    }
}

function coordinates(side, anchorRect, width, height, gap) {
    switch (side) {
        case 'left':
            return {
                left: anchorRect.left - width - gap,
                top: anchorRect.top + ((anchorRect.height - height) / 2),
            };
        case 'right':
            return {
                left: anchorRect.right + gap,
                top: anchorRect.top + ((anchorRect.height - height) / 2),
            };
        case 'top':
            return {
                left: anchorRect.left + ((anchorRect.width - width) / 2),
                top: anchorRect.top - height - gap,
            };
        default:
            return {
                left: anchorRect.left + ((anchorRect.width - width) / 2),
                top: anchorRect.bottom + gap,
            };
    }
}

export function connectPopover(anchor, panel, options) {
    const state = {
        open: false,
        showTimer: 0,
        hideTimer: 0,
        listeners: [],
        viewportListeners: [],
    };
    instances.set(anchor, state);

    const gap = options?.offset ?? 10;
    const sides = sidesFor(options?.placement);
    const margin = 8;

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
        clearTimeout(state.showTimer);
        clearTimeout(state.hideTimer);
        offViewport();
        panel.classList.remove('sui-popover--open');
        panel.setAttribute('aria-hidden', 'true');
    }

    function scheduleShow() {
        clearTimeout(state.hideTimer);
        if (!state.open) {
            clearTimeout(state.showTimer);
            state.showTimer = setTimeout(show, options?.showDelay ?? 140);
        }
    }

    function scheduleHide() {
        clearTimeout(state.showTimer);
        if (state.open) {
            clearTimeout(state.hideTimer);
            state.hideTimer = setTimeout(hide, options?.hideDelay ?? 160);
        }
    }

    function onKeyDown(event) {
        if (event.key === 'Escape') {
            hide();
        }
    }

    on(anchor, 'mouseenter', scheduleShow);
    on(anchor, 'mouseleave', scheduleHide);
    on(anchor, 'focusin', scheduleShow);
    on(anchor, 'focusout', scheduleHide);
    // The panel is part of the hover target: moving from the anchor onto the panel
    // (links, meters) must keep the surface open.
    on(panel, 'mouseenter', () => clearTimeout(state.hideTimer));
    on(panel, 'mouseleave', scheduleHide);
    on(document, 'keydown', onKeyDown);
}

export function disconnectPopover(anchor) {
    const state = instances.get(anchor);
    if (!state) {
        return;
    }
    for (const [target, type, handler, opts] of state.listeners) {
        target.removeEventListener(type, handler, opts);
    }
    state.listeners = [];
    state.open = false;
    instances.delete(anchor);
}
