let active;
const bindings = new Map();

export function close(panelId) {
    bindings.get(panelId)?.dispose(false);
}

export function focus(panel, selector) {
    panel?.querySelector(selector)?.focus({ preventScroll: true });
}

export function open(panel, anchorId, x, y, dotnet, generation) {
    if (!panel?.isConnected || !panel.id) return;
    const id = panel.id;
    // Keep the native popover open when a single consumer changes its target.
    const wasOpen = panel.matches(':popover-open');
    const previousBinding = bindings.get(id);
    if (active && active !== panel) bindings.get(active.id)?.dispose(true);
    previousBinding?.dispose(false, wasOpen);

    const anchor = anchorId ? document.getElementById(anchorId) : null;
    const previousFocus = anchor ?? previousBinding?.previousFocus ?? document.activeElement;
    const point = Number.isFinite(x) && Number.isFinite(y);
    const viewport = window.visualViewport;
    const events = new AbortController();
    const options = { signal: events.signal };
    const items = () => [...panel.querySelectorAll('[role="menuitem"], summary')]
        .filter(item => !item.matches(':disabled, [aria-disabled="true"]') && item.getClientRects().length > 0);

    function position() {
        const rect = anchor?.getBoundingClientRect();
        const left = viewport?.offsetLeft ?? 0;
        const top = viewport?.offsetTop ?? 0;
        const width = viewport?.width ?? window.innerWidth;
        const height = viewport?.height ?? window.innerHeight;
        const margin = Math.min(8, width / 4, height / 4);
        panel.style.maxWidth = `${width - margin * 2}px`;
        panel.style.maxHeight = `${height - margin * 2}px`;
        const bounds = panel.getBoundingClientRect();
        const clamp = (value, start, end) => Math.max(start, Math.min(value, end));
        panel.style.left = `${clamp(point ? x : rect?.left ?? left, left + margin, left + width - bounds.width - margin)}px`;
        panel.style.top = `${clamp(point ? y : rect?.bottom ?? top, top + margin, top + height - bounds.height - margin)}px`;
    }

    function keydown(event) {
        if (event.key === 'Escape') {
            event.preventDefault();
            if (!panel.hasAttribute('data-sui-defer-escape')) {
                event.stopPropagation();
                dispose(true);
            }
            return;
        }
        if (event.key === 'Tab' && !panel.hasAttribute('data-sui-defer-escape')) {
            dispose(true);
            return;
        }
        if (event.target.matches('input, textarea, select, [contenteditable="true"]')) return;
        const buttons = items();
        if (!buttons.length) return;
        const index = buttons.indexOf(document.activeElement);
        let next;
        switch (event.key) {
            case 'ArrowDown': next = (index + 1) % buttons.length; break;
            case 'ArrowUp': next = index <= 0 ? buttons.length - 1 : index - 1; break;
            case 'Home': next = 0; break;
            case 'End': next = buttons.length - 1; break;
            default: return;
        }
        event.preventDefault();
        buttons[next].focus({ preventScroll: true });
    }

    const observer = new ResizeObserver(position);
    function dispose(notify, keepOpen = false) {
        if (events.signal.aborted) return;
        const returnFocus = document.activeElement === document.body || panel.contains(document.activeElement);
        events.abort();
        observer.disconnect();
        bindings.delete(id);
        if (active === panel) active = null;
        if (!keepOpen && panel.matches(':popover-open')) panel.hidePopover();
        if (!keepOpen && returnFocus && previousFocus?.isConnected) previousFocus.focus({ preventScroll: true });
        if (notify) dotnet.invokeMethodAsync('NotifyDismissed', generation).catch(() => {});
    }

    bindings.set(id, { dispose, previousFocus });
    active = panel;
    panel.addEventListener('keydown', keydown, options);
    // WebKit can light-dismiss an auto popover on the same right-button release
    // that follows its opening contextmenu event. Only a new gesture dismisses it.
    document.addEventListener('pointerdown', event => {
        if (!event.composedPath().includes(panel) && !anchor?.contains(event.target)) dispose(true);
    }, { ...options, capture: true });
    panel.addEventListener('toggle', () => {
        if (!panel.matches(':popover-open')) dispose(true);
    }, options);
    window.addEventListener('resize', position, options);
    viewport?.addEventListener('resize', position, options);
    viewport?.addEventListener('scroll', position, options);
    if (!wasOpen) panel.showPopover();
    position();
    observer.observe(panel);
    (items()[0] ?? panel).focus({ preventScroll: true });
}
