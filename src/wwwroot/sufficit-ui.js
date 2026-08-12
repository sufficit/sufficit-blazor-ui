const railFlyoutObservers = new WeakMap();
const selectMenuControllers = new WeakMap();
let activeSelectMenu = null;

function isOpenPopover(menu) {
    try {
        return menu.matches(":popover-open");
    } catch {
        return false;
    }
}

function placeSelectMenu(trigger, menu) {
    if (!trigger?.isConnected || !menu?.isConnected || !isOpenPopover(menu)) {
        return;
    }

    const margin = 12;
    const gap = 4;
    const triggerRect = trigger.getBoundingClientRect();
    menu.style.setProperty("--sui-select-trigger-width", `${Math.round(triggerRect.width)}px`);

    const availableBelow = Math.max(0, window.innerHeight - triggerRect.bottom - gap - margin);
    const availableAbove = Math.max(0, triggerRect.top - gap - margin);
    const measuredHeight = Math.min(280, Math.max(menu.scrollHeight, menu.getBoundingClientRect().height));
    const openBelow = availableBelow >= Math.min(measuredHeight, 160)
        || availableBelow >= availableAbove;
    const availableHeight = Math.max(80, openBelow ? availableBelow : availableAbove);
    menu.style.setProperty(
        "--sui-select-menu-available-height",
        `${Math.floor(availableHeight)}px`);

    const menuRect = menu.getBoundingClientRect();
    const direction = getComputedStyle(trigger).direction;
    const preferredLeft = direction === "rtl"
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

function releaseSelectMenu(menu) {
    const controller = selectMenuControllers.get(menu);
    if (!controller) {
        return;
    }

    window.removeEventListener("resize", controller.place);
    window.removeEventListener("scroll", controller.place, true);
    controller.resizeObserver?.disconnect();
    selectMenuControllers.delete(menu);
}

export function openSelectMenu(trigger, menu) {
    if (!trigger || !menu) {
        return;
    }

    if (activeSelectMenu && activeSelectMenu !== menu) {
        closeSelectMenu(activeSelectMenu);
    }

    if (typeof menu.showPopover !== "function") {
        // CSS keeps the previous absolute-position fallback for older browsers.
        return;
    }

    if (!isOpenPopover(menu)) {
        try {
            menu.showPopover();
        } catch (error) {
            console.warn("[Sufficit UI] Select menu could not enter the top layer; using the inline fallback.", error);
            return;
        }
    }

    activeSelectMenu = menu;
    if (!selectMenuControllers.has(menu)) {
        const place = () => requestAnimationFrame(() => placeSelectMenu(trigger, menu));
        const resizeObserver = typeof ResizeObserver === "function"
            ? new ResizeObserver(place)
            : null;
        resizeObserver?.observe(trigger);
        resizeObserver?.observe(menu);
        window.addEventListener("resize", place, { passive: true });
        window.addEventListener("scroll", place, { passive: true, capture: true });
        selectMenuControllers.set(menu, { place, resizeObserver });
    }

    placeSelectMenu(trigger, menu);
}

export function closeSelectMenu(menu) {
    if (!menu) {
        return;
    }

    releaseSelectMenu(menu);
    if (isOpenPopover(menu) && typeof menu.hidePopover === "function") {
        menu.hidePopover();
    }
    if (activeSelectMenu === menu) {
        activeSelectMenu = null;
    }
}

function getRailTrigger(flyout) {
    return flyout.parentElement?.querySelector(":scope > .sui-rail-trigger");
}

function placeRailFlyout(flyout) {
    if (!flyout || flyout.hidden || getComputedStyle(flyout).display === "none") {
        return;
    }

    const trigger = getRailTrigger(flyout);
    if (!trigger) {
        return;
    }

    const margin = 12;
    const gap = 8;
    const triggerRect = trigger.getBoundingClientRect();

    // The panel can be taller than the space below a trigger near the bottom of
    // the rail. Give the inner panel an explicit viewport-safe ceiling before the
    // final measurement so the browser never paints part of the menu off-screen.
    const availableAbove = Math.max(160, triggerRect.top - margin - gap);
    const availableBelow = Math.max(160, window.innerHeight - triggerRect.bottom - margin - gap);
    const preferredHeight = Math.max(availableAbove, availableBelow);
    flyout.style.setProperty("--sui-rail-flyout-max-height", `${Math.floor(preferredHeight)}px`);

    // The flyout lives inside the drawer (whose blur creates a fixed containing
    // block), so CSS anchor positioning cannot reliably flip at the viewport
    // edge. Temporarily use explicit coordinates, measure the panel, then place
    // it beside the trigger or above it when there is no room below.
    flyout.style.positionAnchor = "none";
    flyout.style.insetInlineStart = "auto";
    flyout.style.left = `${Math.round(triggerRect.right + gap)}px`;
    flyout.style.top = `${margin}px`;

    const flyoutRect = flyout.getBoundingClientRect();
    const fitsBelow = triggerRect.top + flyoutRect.height <= window.innerHeight - margin;
    const preferredTop = fitsBelow
        ? triggerRect.top
        : triggerRect.top - flyoutRect.height - gap;
    const maxTop = Math.max(margin, window.innerHeight - flyoutRect.height - margin);
    const top = Math.min(Math.max(margin, preferredTop), maxTop);

    const preferredLeft = triggerRect.right + gap;
    const maxLeft = Math.max(margin, window.innerWidth - flyoutRect.width - margin);
    const left = preferredLeft + flyoutRect.width <= window.innerWidth - margin
        ? preferredLeft
        : Math.min(maxLeft, Math.max(margin, triggerRect.left - flyoutRect.width - gap));

    flyout.style.left = `${Math.round(left)}px`;
    flyout.style.top = `${Math.round(top)}px`;
}

function placeVisibleRailFlyouts() {
    document.querySelectorAll(".sui-rail-flyout").forEach(placeRailFlyout);
}

function schedulePlacement() {
    requestAnimationFrame(placeVisibleRailFlyouts);
}

function observeRailFlyout(flyout) {
    if (railFlyoutObservers.has(flyout)) {
        return;
    }

    const observer = new ResizeObserver(() => placeRailFlyout(flyout));
    observer.observe(flyout);
    railFlyoutObservers.set(flyout, observer);
}

const bodyObserver = new MutationObserver(mutations => {
    for (const mutation of mutations) {
        if (mutation.type === "childList") {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType !== Node.ELEMENT_NODE) return;
                node.matches?.(".sui-rail-flyout") && observeRailFlyout(node);
                node.querySelectorAll?.(".sui-rail-flyout").forEach(observeRailFlyout);
            });
        }
    }

    if (mutations.some(mutation => {
        if (mutation.type !== "attributes") {
            return false;
        }

        const target = mutation.target;
        return target.matches?.(".sui-rail-flyout")
            || target.closest?.(".sui-rail-flyout") !== null;
    })) {
        schedulePlacement();
    }
});

document.querySelectorAll(".sui-rail-flyout").forEach(observeRailFlyout);
bodyObserver.observe(document.body, {
    attributes: true,
    attributeFilter: ["class", "hidden"],
    childList: true,
    subtree: true,
});

window.addEventListener("resize", schedulePlacement, { passive: true });
window.addEventListener("scroll", schedulePlacement, { passive: true, capture: true });
