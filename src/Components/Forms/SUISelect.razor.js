const menuControllers = new WeakMap();
const triggerControllers = new WeakMap();
let activeMenu = null;

function isOpenPopover(menu) {
    try {
        return menu.matches(':popover-open');
    } catch {
        return false;
    }
}

function placeMenu(trigger, menu) {
    if (!trigger?.isConnected || !menu?.isConnected || !isOpenPopover(menu)) {
        return;
    }

    const margin = 12;
    const gap = 4;
    const triggerRect = trigger.getBoundingClientRect();
    menu.style.setProperty('--sui-select-trigger-width', `${Math.round(triggerRect.width)}px`);

    const availableBelow = Math.max(0, window.innerHeight - triggerRect.bottom - gap - margin);
    const availableAbove = Math.max(0, triggerRect.top - gap - margin);
    const measuredHeight = Math.min(280, Math.max(menu.scrollHeight, menu.getBoundingClientRect().height));
    const openBelow = availableBelow >= Math.min(measuredHeight, 160)
        || availableBelow >= availableAbove;
    const availableHeight = Math.max(80, openBelow ? availableBelow : availableAbove);
    menu.style.setProperty('--sui-select-menu-available-height', `${Math.floor(availableHeight)}px`);

    const menuRect = menu.getBoundingClientRect();
    const direction = getComputedStyle(trigger).direction;
    const preferredLeft = direction === 'rtl'
        ? triggerRect.right - menuRect.width
        : triggerRect.left;
    const maxLeft = Math.max(margin, window.innerWidth - menuRect.width - margin);
    const left = Math.min(Math.max(margin, preferredLeft), maxLeft);
    const preferredTop = openBelow
        ? triggerRect.bottom + gap
        : triggerRect.top - menuRect.height - gap;
    const maxTop = Math.max(margin, window.innerHeight - menuRect.height - margin);
    const top = Math.min(Math.max(margin, preferredTop), maxTop);

    menu.style.left = `${Math.round(left)}px`;
    menu.style.top = `${Math.round(top)}px`;
}

function releaseMenu(menu) {
    const controller = menuControllers.get(menu);
    if (!controller) {
        return;
    }

    window.removeEventListener('resize', controller.place);
    window.removeEventListener('scroll', controller.place, true);
    controller.resizeObserver?.disconnect();
    if (controller.frame) {
        cancelAnimationFrame(controller.frame);
    }
    menuControllers.delete(menu);
}

export function connectSelectTrigger(trigger) {
    if (!trigger || triggerControllers.has(trigger)) {
        return;
    }

    const onKeyDown = event => {
        if (['ArrowDown', 'ArrowUp', 'Home', 'End', 'Enter', ' ', 'Escape'].includes(event.key)) {
            event.preventDefault();
        }
    };
    trigger.addEventListener('keydown', onKeyDown);
    triggerControllers.set(trigger, onKeyDown);
}

export function disconnectSelectTrigger(trigger) {
    const onKeyDown = triggerControllers.get(trigger);
    if (!onKeyDown) {
        return;
    }

    trigger.removeEventListener('keydown', onKeyDown);
    triggerControllers.delete(trigger);
}

export function openSelectMenu(trigger, menu) {
    if (!trigger || !menu) {
        return;
    }

    if (activeMenu && activeMenu !== menu) {
        closeSelectMenu(activeMenu);
    }

    if (typeof menu.showPopover !== 'function') {
        return;
    }

    if (!isOpenPopover(menu)) {
        try {
            menu.showPopover();
        } catch (error) {
            console.warn('[Sufficit UI] Select menu could not enter the top layer; using the inline fallback.', error);
            return;
        }
    }

    activeMenu = menu;
    if (!menuControllers.has(menu)) {
        const controller = {
            frame: 0,
            place: null,
            resizeObserver: null,
        };
        controller.place = () => {
            if (controller.frame) {
                cancelAnimationFrame(controller.frame);
            }
            controller.frame = requestAnimationFrame(() => {
                controller.frame = 0;
                placeMenu(trigger, menu);
            });
        };
        controller.resizeObserver = typeof ResizeObserver === 'function'
            ? new ResizeObserver(controller.place)
            : null;
        controller.resizeObserver?.observe(trigger);
        controller.resizeObserver?.observe(menu);
        window.addEventListener('resize', controller.place, { passive: true });
        window.addEventListener('scroll', controller.place, { passive: true, capture: true });
        menuControllers.set(menu, controller);
    }

    placeMenu(trigger, menu);
}

export function closeSelectMenu(menu) {
    if (!menu) {
        return;
    }

    releaseMenu(menu);
    if (isOpenPopover(menu) && typeof menu.hidePopover === 'function') {
        menu.hidePopover();
    }
    if (activeMenu === menu) {
        activeMenu = null;
    }
}
