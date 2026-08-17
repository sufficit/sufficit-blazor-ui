const controllers = new WeakMap();
let activePopover = null;

function isTopLayerOpen(popover) {
    try {
        return popover.matches(':popover-open');
    } catch {
        return false;
    }
}

function stopPositioning(popover) {
    const controller = controllers.get(popover);
    if (!controller?.positioning) {
        return;
    }

    window.removeEventListener('resize', controller.place);
    window.removeEventListener('scroll', controller.place, true);
    controller.resizeObserver?.disconnect();
    if (controller.frame) {
        cancelAnimationFrame(controller.frame);
    }
    controller.positioning = false;
}

function placePopover(trigger, popover) {
    if (!trigger?.isConnected || !popover?.isConnected || !isTopLayerOpen(popover)) {
        return;
    }

    const margin = 12;
    const gap = 4;
    const triggerRect = trigger.getBoundingClientRect();
    const availableBelow = Math.max(0, window.innerHeight - triggerRect.bottom - gap - margin);
    const availableAbove = Math.max(0, triggerRect.top - gap - margin);
    const desiredHeight = Math.max(popover.scrollHeight, popover.getBoundingClientRect().height);
    const openBelow = availableBelow >= Math.min(desiredHeight, 280) || availableBelow >= availableAbove;
    const availableHeight = Math.max(180, openBelow ? availableBelow : availableAbove);
    popover.style.setProperty('--sui-date-field-available-height', `${Math.floor(availableHeight)}px`);

    const popoverRect = popover.getBoundingClientRect();
    const direction = getComputedStyle(trigger).direction;
    const preferredLeft = direction === 'rtl'
        ? triggerRect.right - popoverRect.width
        : triggerRect.left;
    const maxLeft = Math.max(margin, window.innerWidth - popoverRect.width - margin);
    const left = Math.min(Math.max(margin, preferredLeft), maxLeft);
    const preferredTop = openBelow
        ? triggerRect.bottom + gap
        : triggerRect.top - popoverRect.height - gap;
    const maxTop = Math.max(margin, window.innerHeight - popoverRect.height - margin);
    const top = Math.min(Math.max(margin, preferredTop), maxTop);

    popover.style.left = `${Math.round(left)}px`;
    popover.style.top = `${Math.round(top)}px`;
}

function startPositioning(trigger, popover) {
    const controller = controllers.get(popover);
    if (!controller || controller.positioning || !isTopLayerOpen(popover)) {
        return;
    }

    controller.place = () => {
        if (controller.frame) {
            cancelAnimationFrame(controller.frame);
        }
        controller.frame = requestAnimationFrame(() => {
            controller.frame = 0;
            placePopover(trigger, popover);
        });
    };
    controller.resizeObserver = typeof ResizeObserver === 'function'
        ? new ResizeObserver(controller.place)
        : null;
    controller.resizeObserver?.observe(trigger);
    controller.resizeObserver?.observe(popover);
    window.addEventListener('resize', controller.place, { passive: true });
    window.addEventListener('scroll', controller.place, { passive: true, capture: true });
    controller.positioning = true;
}

function hidePopover(popover) {
    stopPositioning(popover);
    if (isTopLayerOpen(popover) && typeof popover.hidePopover === 'function') {
        popover.hidePopover();
    }
    if (activePopover === popover) {
        activePopover = null;
    }
}

export function connectDateField(root, trigger, popover, dotnet) {
    if (!root || !trigger || !popover || controllers.has(popover)) {
        return;
    }

    const controller = {
        dotnet,
        frame: 0,
        place: null,
        resizeObserver: null,
        positioning: false,
        onPointerDown: null,
        onKeyDown: null,
    };
    controller.onPointerDown = event => {
        if (!isTopLayerOpen(popover) && !popover.classList.contains('sui-date-field__popover--open')) {
            return;
        }
        if (!root.contains(event.target) && !popover.contains(event.target)) {
            dotnet.invokeMethodAsync('CloseFromJs');
        }
    };
    controller.onKeyDown = event => {
        const triggerKeys = ['Enter', ' ', 'ArrowDown', 'Escape'];
        const dayKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', 'PageUp', 'PageDown', 'Enter', ' '];
        const isDay = event.target instanceof Element && event.target.closest('.sui-date-field__day');
        if ((event.target === trigger && triggerKeys.includes(event.key))
            || (isDay && dayKeys.includes(event.key))
            || (popover.contains(event.target) && event.key === 'Escape')) {
            event.preventDefault();
        }
    };

    document.addEventListener('pointerdown', controller.onPointerDown, true);
    root.addEventListener('keydown', controller.onKeyDown);
    controllers.set(popover, controller);
}

export function openDateField(trigger, popover) {
    if (!trigger || !popover) {
        return;
    }

    if (activePopover && activePopover !== popover) {
        const previous = controllers.get(activePopover);
        previous?.dotnet.invokeMethodAsync('CloseFromJs');
        hidePopover(activePopover);
    }

    if (typeof popover.showPopover === 'function' && !isTopLayerOpen(popover)) {
        try {
            popover.showPopover();
        } catch (error) {
            console.warn('[Sufficit UI] Date field could not enter the top layer; using the inline fallback.', error);
        }
    }

    activePopover = popover;
    startPositioning(trigger, popover);
    placePopover(trigger, popover);
}

export function focusDate(popover, isoDate) {
    const day = popover?.querySelector(`[data-sui-date="${isoDate}"]`);
    day?.focus({ preventScroll: true });
}

export function closeDateField(popover, trigger, restoreFocus) {
    hidePopover(popover);
    if (restoreFocus) {
        trigger?.focus({ preventScroll: true });
    }
}

export function disconnectDateField(root, popover) {
    const controller = controllers.get(popover);
    if (!controller) {
        return;
    }

    hidePopover(popover);
    document.removeEventListener('pointerdown', controller.onPointerDown, true);
    root?.removeEventListener('keydown', controller.onKeyDown);
    controllers.delete(popover);
}
