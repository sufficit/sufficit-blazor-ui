import { parseNumber, sidesFor, coordinates, overflow } from './SUIPopover.razor.js';

const anchors = new Set();
const anchorControllers = new WeakMap();
let activeAnchor = null;
let activeDescriptionTarget = null;
let tooltipElement = null;
let showTimer = 0;
let hideTimer = 0;
let viewportListenersConnected = false;

const styleProperties = {
    suiTooltipBackground: '--sui-tooltip-background',
    suiTooltipColor: '--sui-tooltip-color',
    suiTooltipBorder: '--sui-tooltip-border',
    suiTooltipOpacity: '--sui-tooltip-opacity',
    suiTooltipRadius: '--sui-tooltip-radius',
    suiTooltipPadding: '--sui-tooltip-padding',
    suiTooltipShadow: '--sui-tooltip-shadow',
    suiTooltipFontSize: '--sui-tooltip-font-size',
    suiTooltipFontWeight: '--sui-tooltip-font-weight',
};

function ensureTooltip() {
    if (tooltipElement?.isConnected) {
        return tooltipElement;
    }

    tooltipElement = document.createElement('div');
    tooltipElement.id = 'sui-tooltip-portal';
    tooltipElement.className = 'sui-tooltip';
    tooltipElement.setAttribute('role', 'tooltip');
    tooltipElement.setAttribute('aria-hidden', 'true');
    document.body.appendChild(tooltipElement);
    return tooltipElement;
}

function placeTooltip(anchor, tooltip) {
    if (!anchor?.isConnected || !tooltip?.isConnected) {
        return;
    }

    const margin = 10;
    const gap = parseNumber(anchor.dataset.suiTooltipOffset, 10, 0, 80);
    const targetRect = anchor.getBoundingClientRect();
    const tooltipRect = tooltip.getBoundingClientRect();
    const preferred = anchor.dataset.suiTooltipPlacement?.toLowerCase() || 'right';
    const placements = sidesFor(preferred, 'right').map(side => ({
        side,
        position: coordinates(side, targetRect, tooltipRect.width, tooltipRect.height, gap),
    }));
    const selected = placements.find(item => overflow(item.position, tooltipRect, margin) === 0)
        || placements.reduce((best, item) =>
            overflow(item.position, tooltipRect, margin) < overflow(best.position, tooltipRect, margin)
                ? item
                : best);
    const left = Math.min(
        Math.max(margin, selected.position.left),
        Math.max(margin, window.innerWidth - tooltipRect.width - margin));
    const top = Math.min(
        Math.max(margin, selected.position.top),
        Math.max(margin, window.innerHeight - tooltipRect.height - margin));

    tooltip.classList.remove(
        'sui-tooltip--right',
        'sui-tooltip--left',
        'sui-tooltip--top',
        'sui-tooltip--bottom');
    tooltip.classList.add(`sui-tooltip--${selected.side}`);
    tooltip.style.left = `${Math.round(left)}px`;
    tooltip.style.top = `${Math.round(top)}px`;
}

function applyOptions(anchor, tooltip) {
    tooltip.className = 'sui-tooltip';
    const additionalClasses = anchor.dataset.suiTooltipClass?.trim();
    if (additionalClasses) {
        tooltip.classList.add(...additionalClasses.split(/\s+/).filter(Boolean));
    }
    tooltip.classList.toggle('sui-tooltip--no-arrow', anchor.dataset.suiTooltipArrow === 'false');

    for (const [datasetName, propertyName] of Object.entries(styleProperties)) {
        const value = anchor.dataset[datasetName]?.trim();
        if (value) {
            tooltip.style.setProperty(propertyName, value);
        } else {
            tooltip.style.removeProperty(propertyName);
        }
    }

    const maxWidth = parseNumber(anchor.dataset.suiTooltipMaxWidth, 240, 80, 1200);
    tooltip.style.setProperty('--sui-tooltip-max-width', `${maxWidth}px`);
}

// Blazor renders rich content in a hidden source element.
const sourceOf = anchor => document.getElementById(anchor.dataset.suiTooltipContent || '');
const isPersistent = anchor => anchor.dataset.suiTooltipPersistent === 'true';

function descriptionTarget(anchor) {
    return anchor.matches('a, button, input, select, textarea, [tabindex]')
        ? anchor
        : anchor.querySelector('a, button, input, select, textarea, [tabindex]') || anchor;
}

function addDescription(target, id) {
    const ids = new Set((target.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean));
    ids.add(id);
    target.setAttribute('aria-describedby', [...ids].join(' '));
}

function removeDescription(target, id) {
    if (!target) {
        return;
    }

    const ids = (target.getAttribute('aria-describedby') || '')
        .split(/\s+/)
        .filter(value => value && value !== id);
    if (ids.length > 0) {
        target.setAttribute('aria-describedby', ids.join(' '));
    } else {
        target.removeAttribute('aria-describedby');
    }
}

function connectViewportListeners() {
    if (viewportListenersConnected) {
        return;
    }

    document.addEventListener('pointerdown', dismissImmediately, { passive: true });
    window.addEventListener('resize', dismissImmediately, { passive: true });
    window.addEventListener('scroll', dismissImmediately, { passive: true, capture: true });
    viewportListenersConnected = true;
}

function disconnectViewportListeners() {
    if (!viewportListenersConnected) {
        return;
    }

    document.removeEventListener('pointerdown', dismissImmediately);
    window.removeEventListener('resize', dismissImmediately);
    window.removeEventListener('scroll', dismissImmediately, true);
    viewportListenersConnected = false;
}

function conceal() {
    if (tooltipElement) {
        removeDescription(activeDescriptionTarget, tooltipElement.id);
        tooltipElement.classList.remove('is-visible');
        tooltipElement.setAttribute('aria-hidden', 'true');
    }
    activeAnchor = null;
    activeDescriptionTarget = null;
    disconnectViewportListeners();
}

function dismissImmediately() {
    hideTooltip(true);
}

function showTooltip(anchor, immediate = false) {
    const label = anchor?.dataset?.suiTooltip?.trim();
    const source = anchor && sourceOf(anchor);
    if (!label && !source) {
        return;
    }

    window.clearTimeout(hideTimer);
    window.clearTimeout(showTimer);
    const reveal = () => {
        if (!anchor.isConnected || !anchors.has(anchor)) {
            return;
        }

        if (activeAnchor && activeAnchor !== anchor) {
            conceal();
        }
        const tooltip = ensureTooltip();
        activeAnchor = anchor;
        activeDescriptionTarget = descriptionTarget(anchor);
        applyOptions(anchor, tooltip);
        if (source) tooltip.replaceChildren(...source.cloneNode(true).childNodes);
        else tooltip.textContent = label;
        tooltip.setAttribute('aria-hidden', 'false');
        // Persistent descriptions already reference the source.
        if (!isPersistent(anchor)) addDescription(activeDescriptionTarget, tooltip.id);
        placeTooltip(anchor, tooltip);
        connectViewportListeners();
        requestAnimationFrame(() => {
            if (activeAnchor === anchor) {
                tooltip.classList.add('is-visible');
            }
        });
    };

    const delay = parseNumber(anchor.dataset.suiTooltipShowDelay, 220, 0, 5000);
    showTimer = window.setTimeout(reveal, immediate ? 0 : delay);
}

function hideTooltip(immediate = false) {
    window.clearTimeout(showTimer);
    window.clearTimeout(hideTimer);
    const delay = parseNumber(activeAnchor?.dataset?.suiTooltipHideDelay, 70, 0, 5000);
    hideTimer = window.setTimeout(conceal, immediate ? 0 : delay);
}

// Long press previews touch tooltips; release must not activate the control.
const longPressDelay = 450;
const longPressLinger = 1500;

export function connectTooltip(anchor) {
    if (!anchor || anchorControllers.has(anchor)) {
        return;
    }

    let longPressTimer = 0;
    let lingerTimer = 0;
    let pressing = false;
    let held = false;
    const endPress = () => {
        window.clearTimeout(longPressTimer);
        pressing = false;
        if (held) {
            window.clearTimeout(lingerTimer);
            lingerTimer = window.setTimeout(() => hideTooltip(true), longPressLinger);
        }
    };
    const controller = {
        pointerEnter: event => {
            if (event.pointerType !== 'touch') showTooltip(anchor);
        },
        pointerLeave: event => {
            if (event.pointerType !== 'touch' && !anchor.contains(document.activeElement)) {
                hideTooltip();
            }
        },
        pointerDown: event => {
            if (event.pointerType !== 'touch') return;
            window.clearTimeout(longPressTimer);
            window.clearTimeout(lingerTimer);
            pressing = true;
            held = false;
            longPressTimer = window.setTimeout(() => {
                held = true;
                showTooltip(anchor, true);
            }, longPressDelay);
        },
        pointerEnd: endPress,
        click: event => {
            if (!held) return;
            held = false;
            event.preventDefault();
            event.stopPropagation();
        },
        contextMenu: event => {
            if (pressing || held) event.preventDefault();
        },
        // Open on keyboard focus, not the focus produced by a tap.
        focusIn: event => {
            if (event.target?.matches?.(':focus-visible')) showTooltip(anchor, true);
        },
        // Focus leaving another control must not close this tooltip.
        focusOut: event => {
            if (activeAnchor === anchor && !anchor.contains(event.relatedTarget)) {
                hideTooltip(true);
            }
        },
    };
    anchor.addEventListener('pointerenter', controller.pointerEnter);
    anchor.addEventListener('pointerleave', controller.pointerLeave);
    anchor.addEventListener('pointerdown', controller.pointerDown);
    anchor.addEventListener('pointerup', controller.pointerEnd);
    anchor.addEventListener('pointercancel', controller.pointerEnd);
    anchor.addEventListener('click', controller.click, true);
    anchor.addEventListener('contextmenu', controller.contextMenu);
    anchor.addEventListener('focusin', controller.focusIn);
    anchor.addEventListener('focusout', controller.focusOut);
    anchors.add(anchor);
    anchorControllers.set(anchor, controller);
    const source = sourceOf(anchor);
    if (source && isPersistent(anchor)) addDescription(descriptionTarget(anchor), source.id);
}

export function disconnectTooltip(anchor) {
    const controller = anchorControllers.get(anchor);
    if (!controller) {
        return;
    }

    anchor.removeEventListener('pointerenter', controller.pointerEnter);
    anchor.removeEventListener('pointerleave', controller.pointerLeave);
    anchor.removeEventListener('pointerdown', controller.pointerDown);
    anchor.removeEventListener('pointerup', controller.pointerEnd);
    anchor.removeEventListener('pointercancel', controller.pointerEnd);
    anchor.removeEventListener('click', controller.click, true);
    anchor.removeEventListener('contextmenu', controller.contextMenu);
    anchor.removeEventListener('focusin', controller.focusIn);
    anchor.removeEventListener('focusout', controller.focusOut);
    anchorControllers.delete(anchor);
    anchors.delete(anchor);
    if (anchor.dataset.suiTooltipContent) removeDescription(descriptionTarget(anchor), anchor.dataset.suiTooltipContent);

    if (activeAnchor === anchor) {
        window.clearTimeout(showTimer);
        window.clearTimeout(hideTimer);
        conceal();
    }
    if (anchors.size === 0) {
        tooltipElement?.remove();
        tooltipElement = null;
        disconnectViewportListeners();
    }
}
