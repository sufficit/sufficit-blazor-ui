const railFlyoutObservers = new WeakMap();
const selectMenuControllers = new WeakMap();
let activeSelectMenu = null;
let activeTooltipTarget = null;
let activeTooltipDescriptionTarget = null;
let tooltipElement = null;
let tooltipShowTimer = 0;
let tooltipHideTimer = 0;

const tooltipStyleProperties = {
    suiTooltipBackground: "--sui-tooltip-background",
    suiTooltipColor: "--sui-tooltip-color",
    suiTooltipBorder: "--sui-tooltip-border",
    suiTooltipOpacity: "--sui-tooltip-opacity",
    suiTooltipRadius: "--sui-tooltip-radius",
    suiTooltipPadding: "--sui-tooltip-padding",
    suiTooltipShadow: "--sui-tooltip-shadow",
    suiTooltipFontSize: "--sui-tooltip-font-size",
    suiTooltipFontWeight: "--sui-tooltip-font-weight",
};

function ensureTooltip() {
    if (tooltipElement?.isConnected) {
        return tooltipElement;
    }

    tooltipElement = document.createElement("div");
    tooltipElement.id = "sui-rail-tooltip";
    tooltipElement.className = "sui-tooltip";
    tooltipElement.setAttribute("role", "tooltip");
    tooltipElement.setAttribute("aria-hidden", "true");
    document.body.appendChild(tooltipElement);
    return tooltipElement;
}

function parseTooltipNumber(value, fallback, minimum, maximum) {
    const parsed = Number.parseFloat(value);
    return Number.isFinite(parsed)
        ? Math.min(maximum, Math.max(minimum, parsed))
        : fallback;
}

function tooltipCandidates(preferred) {
    switch (preferred) {
        case "left": return ["left", "right", "top", "bottom"];
        case "top": return ["top", "bottom", "right", "left"];
        case "bottom": return ["bottom", "top", "right", "left"];
        case "auto": return ["right", "left", "top", "bottom"];
        default: return ["right", "left", "top", "bottom"];
    }
}

function tooltipCoordinates(side, targetRect, tooltipRect, gap) {
    switch (side) {
        case "left":
            return {
                left: targetRect.left - tooltipRect.width - gap,
                top: targetRect.top + ((targetRect.height - tooltipRect.height) / 2),
            };
        case "top":
            return {
                left: targetRect.left + ((targetRect.width - tooltipRect.width) / 2),
                top: targetRect.top - tooltipRect.height - gap,
            };
        case "bottom":
            return {
                left: targetRect.left + ((targetRect.width - tooltipRect.width) / 2),
                top: targetRect.bottom + gap,
            };
        default:
            return {
                left: targetRect.right + gap,
                top: targetRect.top + ((targetRect.height - tooltipRect.height) / 2),
            };
    }
}

function tooltipOverflow(coordinates, tooltipRect, margin) {
    return Math.max(0, margin - coordinates.left)
        + Math.max(0, coordinates.left + tooltipRect.width + margin - window.innerWidth)
        + Math.max(0, margin - coordinates.top)
        + Math.max(0, coordinates.top + tooltipRect.height + margin - window.innerHeight);
}

function placeTooltip(target, tooltip) {
    if (!target?.isConnected || !tooltip?.isConnected) {
        return;
    }

    const margin = 10;
    const gap = parseTooltipNumber(target.dataset.suiTooltipOffset, 10, 0, 80);
    const targetRect = target.getBoundingClientRect();
    const tooltipRect = tooltip.getBoundingClientRect();
    const preferred = target.dataset.suiTooltipPlacement?.toLowerCase() || "right";
    const placements = tooltipCandidates(preferred)
        .map(side => ({
            side,
            coordinates: tooltipCoordinates(side, targetRect, tooltipRect, gap),
        }));
    const selected = placements.find(candidate =>
        tooltipOverflow(candidate.coordinates, tooltipRect, margin) === 0)
        || placements.reduce((best, candidate) =>
            tooltipOverflow(candidate.coordinates, tooltipRect, margin)
                < tooltipOverflow(best.coordinates, tooltipRect, margin)
                ? candidate
                : best);
    const preferredLeft = selected.coordinates.left;
    const left = Math.min(
        Math.max(margin, preferredLeft),
        Math.max(margin, window.innerWidth - tooltipRect.width - margin));
    const preferredTop = selected.coordinates.top;
    const top = Math.min(
        Math.max(margin, preferredTop),
        Math.max(margin, window.innerHeight - tooltipRect.height - margin));

    tooltip.classList.remove(
        "sui-tooltip--right",
        "sui-tooltip--left",
        "sui-tooltip--top",
        "sui-tooltip--bottom");
    tooltip.classList.add(`sui-tooltip--${selected.side}`);
    tooltip.style.left = `${Math.round(left)}px`;
    tooltip.style.top = `${Math.round(top)}px`;
}

function applyTooltipOptions(target, tooltip) {
    tooltip.className = "sui-tooltip";
    const additionalClasses = target.dataset.suiTooltipClass?.trim();
    if (additionalClasses) {
        tooltip.classList.add(...additionalClasses.split(/\s+/).filter(Boolean));
    }
    tooltip.classList.toggle("sui-tooltip--no-arrow", target.dataset.suiTooltipArrow === "false");

    for (const [datasetName, propertyName] of Object.entries(tooltipStyleProperties)) {
        const value = target.dataset[datasetName]?.trim();
        if (value) {
            tooltip.style.setProperty(propertyName, value);
        } else {
            tooltip.style.removeProperty(propertyName);
        }
    }

    const maxWidth = parseTooltipNumber(target.dataset.suiTooltipMaxWidth, 240, 80, 1200);
    tooltip.style.setProperty("--sui-tooltip-max-width", `${maxWidth}px`);
}

function tooltipDescriptionTarget(target) {
    return target.matches("a, button, input, select, textarea, [tabindex]")
        ? target
        : target.querySelector("a, button, input, select, textarea, [tabindex]") || target;
}

function showTooltip(target, immediate = false) {
    const label = target?.dataset?.suiTooltip?.trim();
    if (!label) {
        return;
    }

    window.clearTimeout(tooltipHideTimer);
    window.clearTimeout(tooltipShowTimer);
    const reveal = () => {
        if (!target.isConnected) {
            return;
        }

        const tooltip = ensureTooltip();
        activeTooltipDescriptionTarget?.removeAttribute("aria-describedby");
        activeTooltipTarget = target;
        activeTooltipDescriptionTarget = tooltipDescriptionTarget(target);
        applyTooltipOptions(target, tooltip);
        tooltip.textContent = label;
        tooltip.setAttribute("aria-hidden", "false");
        activeTooltipDescriptionTarget.setAttribute("aria-describedby", tooltip.id);
        placeTooltip(target, tooltip);
        requestAnimationFrame(() => tooltip.classList.add("is-visible"));
    };

    const showDelay = parseTooltipNumber(target.dataset.suiTooltipShowDelay, 220, 0, 5000);
    tooltipShowTimer = window.setTimeout(reveal, immediate ? 0 : showDelay);
}

function hideTooltip(immediate = false) {
    window.clearTimeout(tooltipShowTimer);
    window.clearTimeout(tooltipHideTimer);
    const conceal = () => {
        activeTooltipDescriptionTarget?.removeAttribute("aria-describedby");
        activeTooltipTarget = null;
        activeTooltipDescriptionTarget = null;
        tooltipElement?.classList.remove("is-visible");
        tooltipElement?.setAttribute("aria-hidden", "true");
    };
    const hideDelay = parseTooltipNumber(
        activeTooltipTarget?.dataset?.suiTooltipHideDelay,
        70,
        0,
        5000);
    tooltipHideTimer = window.setTimeout(conceal, immediate ? 0 : hideDelay);
}

function tooltipTargetFrom(node) {
    return node instanceof Element ? node.closest("[data-sui-tooltip]") : null;
}

document.addEventListener("pointerover", event => {
    const target = tooltipTargetFrom(event.target);
    if (!target || target.contains(event.relatedTarget)) {
        return;
    }
    showTooltip(target);
});

document.addEventListener("pointerout", event => {
    const target = tooltipTargetFrom(event.target);
    if (!target || target.contains(event.relatedTarget)) {
        return;
    }
    if (target.contains(document.activeElement)) {
        return;
    }
    hideTooltip();
});

document.addEventListener("focusin", event => {
    const target = tooltipTargetFrom(event.target);
    if (target) {
        showTooltip(target, true);
    }
});

document.addEventListener("focusout", event => {
    const target = tooltipTargetFrom(event.target);
    if (target && !target.contains(event.relatedTarget)) {
        hideTooltip(true);
    }
});

document.addEventListener("pointerdown", () => hideTooltip(true), { passive: true });
window.addEventListener("resize", () => hideTooltip(true), { passive: true });
window.addEventListener("scroll", () => hideTooltip(true), { passive: true, capture: true });

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

export function isRailInteractionActive(flyout) {
    if (!(flyout instanceof HTMLElement)) {
        return false;
    }

    const group = flyout.closest(".sui-rail-group");
    const activeElement = document.activeElement;
    return flyout.matches(":hover")
        || group?.matches(":hover") === true
        || (activeElement instanceof Element && group?.contains(activeElement) === true);
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
    const configuredGap = Number.parseFloat(
        getComputedStyle(flyout).getPropertyValue("--sui-rail-flyout-gap"));
    const horizontalGap = Number.isFinite(configuredGap) ? configuredGap : 20;
    const triggerRect = trigger.getBoundingClientRect();

    // The panel shares an edge with its trigger: top edges when there is room
    // below, bottom edges when it must grow upwards. Include the trigger height
    // in both measurements so the aligned edge remains visible and intentional.
    const availableWithTopAligned = Math.max(160, window.innerHeight - triggerRect.top - margin);
    const availableWithBottomAligned = Math.max(160, triggerRect.bottom - margin);
    const preferredHeight = Math.max(availableWithTopAligned, availableWithBottomAligned);
    flyout.style.setProperty("--sui-rail-flyout-max-height", `${Math.floor(preferredHeight)}px`);

    // The flyout lives inside the drawer (whose blur creates a fixed containing
    // block), so CSS anchor positioning cannot reliably flip at the viewport
    // edge. Temporarily use explicit coordinates, measure the panel, then place
    // it beside the trigger or above it when there is no room below.
    flyout.style.positionAnchor = "none";
    flyout.style.insetInlineStart = "auto";
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
