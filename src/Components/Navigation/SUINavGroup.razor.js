const flyoutControllers = new WeakMap();

function triggerFor(flyout) {
    return flyout.parentElement?.querySelector(':scope > .sui-rail-trigger');
}

function placeFlyout(flyout) {
    if (!flyout || flyout.hidden || getComputedStyle(flyout).display === 'none') {
        return;
    }

    const trigger = triggerFor(flyout);
    if (!trigger) {
        return;
    }

    const margin = 12;
    const configuredGap = Number.parseFloat(
        getComputedStyle(flyout).getPropertyValue('--sui-rail-flyout-gap'));
    const horizontalGap = Number.isFinite(configuredGap) ? configuredGap : 20;
    const triggerRect = trigger.getBoundingClientRect();
    const availableWithTopAligned = Math.max(160, window.innerHeight - triggerRect.top - margin);
    const availableWithBottomAligned = Math.max(160, triggerRect.bottom - margin);
    const preferredHeight = Math.max(availableWithTopAligned, availableWithBottomAligned);
    flyout.style.setProperty('--sui-rail-flyout-max-height', `${Math.floor(preferredHeight)}px`);

    flyout.style.positionAnchor = 'none';
    flyout.style.insetInlineStart = 'auto';
    flyout.style.left = `${Math.round(triggerRect.right + horizontalGap)}px`;
    flyout.style.top = `${margin}px`;

    const flyoutRect = flyout.getBoundingClientRect();
    const fitsTopAligned = flyoutRect.height <= availableWithTopAligned;
    const preferredTop = fitsTopAligned
        ? triggerRect.top
        : triggerRect.bottom - flyoutRect.height;
    const maxTop = Math.max(margin, window.innerHeight - flyoutRect.height - margin);
    const top = Math.min(Math.max(margin, preferredTop), maxTop);
    const preferredLeft = triggerRect.right + horizontalGap;
    const maxLeft = Math.max(margin, window.innerWidth - flyoutRect.width - margin);
    const left = preferredLeft + flyoutRect.width <= window.innerWidth - margin
        ? preferredLeft
        : Math.min(maxLeft, Math.max(margin, triggerRect.left - flyoutRect.width - horizontalGap));

    flyout.style.left = `${Math.round(left)}px`;
    flyout.style.top = `${Math.round(top)}px`;
}

export function connectRailFlyout(flyout) {
    if (!flyout || flyoutControllers.has(flyout)) {
        return;
    }

    const controller = {
        frame: 0,
        schedule: null,
        resizeObserver: null,
    };
    controller.schedule = () => {
        if (controller.frame) {
            cancelAnimationFrame(controller.frame);
        }
        controller.frame = requestAnimationFrame(() => {
            controller.frame = 0;
            placeFlyout(flyout);
        });
    };
    controller.resizeObserver = typeof ResizeObserver === 'function'
        ? new ResizeObserver(controller.schedule)
        : null;
    controller.resizeObserver?.observe(flyout);
    const trigger = triggerFor(flyout);
    if (trigger) {
        controller.resizeObserver?.observe(trigger);
    }
    window.addEventListener('resize', controller.schedule, { passive: true });
    window.addEventListener('scroll', controller.schedule, { passive: true, capture: true });
    flyoutControllers.set(flyout, controller);
    controller.schedule();
}

export function updateRailFlyout(flyout) {
    const controller = flyoutControllers.get(flyout);
    if (controller) {
        controller.schedule();
    } else {
        placeFlyout(flyout);
    }
}

export function disconnectRailFlyout(flyout) {
    const controller = flyoutControllers.get(flyout);
    if (!controller) {
        return;
    }

    window.removeEventListener('resize', controller.schedule);
    window.removeEventListener('scroll', controller.schedule, true);
    controller.resizeObserver?.disconnect();
    if (controller.frame) {
        cancelAnimationFrame(controller.frame);
    }
    flyoutControllers.delete(flyout);
}

export function isRailInteractionActive(flyout) {
    if (!(flyout instanceof HTMLElement)) {
        return false;
    }

    const group = flyout.closest('.sui-rail-group');
    const activeElement = document.activeElement;
    return flyout.matches(':hover')
        || group?.matches(':hover') === true
        || (activeElement instanceof Element && group?.contains(activeElement) === true);
}
