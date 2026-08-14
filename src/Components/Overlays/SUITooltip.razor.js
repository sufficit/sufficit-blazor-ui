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

function parseNumber(value, fallback, minimum, maximum) {
    const parsed = Number.parseFloat(value);
    return Number.isFinite(parsed)
        ? Math.min(maximum, Math.max(minimum, parsed))
        : fallback;
}

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

function candidates(preferred) {
    switch (preferred) {
        case 'left': return ['left', 'right', 'top', 'bottom'];
        case 'top': return ['top', 'bottom', 'right', 'left'];
        case 'bottom': return ['bottom', 'top', 'right', 'left'];
        case 'auto': return ['right', 'left', 'top', 'bottom'];
        default: return ['right', 'left', 'top', 'bottom'];
    }
}

function coordinates(side, targetRect, tooltipRect, gap) {
    switch (side) {
        case 'left':
            return {
                left: targetRect.left - tooltipRect.width - gap,
                top: targetRect.top + ((targetRect.height - tooltipRect.height) / 2),
            };
        case 'top':
            return {
                left: targetRect.left + ((targetRect.width - tooltipRect.width) / 2),
                top: targetRect.top - tooltipRect.height - gap,
            };
        case 'bottom':
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

function overflow(position, tooltipRect, margin) {
    return Math.max(0, margin - position.left)
        + Math.max(0, position.left + tooltipRect.width + margin - window.innerWidth)
        + Math.max(0, margin - position.top)
        + Math.max(0, position.top + tooltipRect.height + margin - window.innerHeight);
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
    const placements = candidates(preferred).map(side => ({
        side,
        position: coordinates(side, targetRect, tooltipRect, gap),
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
    if (!label) {
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
        tooltip.textContent = label;
        tooltip.setAttribute('aria-hidden', 'false');
        addDescription(activeDescriptionTarget, tooltip.id);
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

export function connectTooltip(anchor) {
    if (!anchor || anchorControllers.has(anchor)) {
        return;
    }

    const controller = {
        pointerEnter: () => showTooltip(anchor),
        pointerLeave: () => {
            if (!anchor.contains(document.activeElement)) {
                hideTooltip();
            }
        },
        focusIn: () => showTooltip(anchor, true),
        focusOut: event => {
            if (!anchor.contains(event.relatedTarget)) {
                hideTooltip(true);
            }
        },
    };
    anchor.addEventListener('pointerenter', controller.pointerEnter);
    anchor.addEventListener('pointerleave', controller.pointerLeave);
    anchor.addEventListener('focusin', controller.focusIn);
    anchor.addEventListener('focusout', controller.focusOut);
    anchors.add(anchor);
    anchorControllers.set(anchor, controller);
}

export function disconnectTooltip(anchor) {
    const controller = anchorControllers.get(anchor);
    if (!controller) {
        return;
    }

    anchor.removeEventListener('pointerenter', controller.pointerEnter);
    anchor.removeEventListener('pointerleave', controller.pointerLeave);
    anchor.removeEventListener('focusin', controller.focusIn);
    anchor.removeEventListener('focusout', controller.focusOut);
    anchorControllers.delete(anchor);
    anchors.delete(anchor);

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
