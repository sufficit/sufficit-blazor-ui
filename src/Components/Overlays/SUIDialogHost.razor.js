const dialogStates = new WeakMap();
let lastFocusedElement = null;
let connectedHosts = 0;

function trackFocus(event) {
    if (event.target instanceof HTMLElement && !event.target.closest('.sui-dialog')) {
        lastFocusedElement = event.target;
    }
}

export function connectDialogHost() {
    connectedHosts += 1;
    if (connectedHosts === 1) {
        document.addEventListener('focusin', trackFocus, true);
    }
}

export function disconnectDialogHost() {
    connectedHosts = Math.max(0, connectedHosts - 1);
    if (connectedHosts === 0) {
        document.removeEventListener('focusin', trackFocus, true);
        lastFocusedElement = null;
    }
}

function focusableElements(dialog) {
    return [...dialog.querySelectorAll(
        'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])')]
        .filter(element => !element.hidden && element.getAttribute('aria-hidden') !== 'true');
}

function elementPath(element) {
    const path = [];
    let current = element;
    while (current && current !== document.body) {
        const parent = current.parentElement;
        if (!parent) {
            return null;
        }

        path.unshift([...parent.children].indexOf(current));
        current = parent;
    }
    return current === document.body ? path : null;
}

function resolveElementPath(path) {
    let current = document.body;
    for (const index of path || []) {
        current = current?.children[index];
        if (!current) {
            return null;
        }
    }
    return current instanceof HTMLElement ? current : null;
}

export function openDialog(dialog, dotNetReference) {
    if (!dialog?.isConnected) {
        return;
    }

    closeDialog(dialog, false);
    const activeElement = document.activeElement instanceof HTMLElement
        ? document.activeElement
        : null;
    const previousFocus = activeElement && activeElement !== document.body
        ? activeElement
        : lastFocusedElement;

    const onKeyDown = event => {
        if (event.key === 'Escape') {
            event.preventDefault();
            event.stopPropagation();
            void dotNetReference.invokeMethodAsync('DismissFromKeyboardAsync');
            return;
        }

        if (event.key !== 'Tab') {
            return;
        }

        const focusables = focusableElements(dialog);
        if (focusables.length === 0) {
            event.preventDefault();
            dialog.focus();
            return;
        }

        const first = focusables[0];
        const last = focusables[focusables.length - 1];
        const active = document.activeElement;
        if (event.shiftKey && (active === first || !dialog.contains(active))) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && active === last) {
            event.preventDefault();
            first.focus();
        }
    };

    document.addEventListener('keydown', onKeyDown, true);
    dialogStates.set(dialog, {
        previousFocus,
        previousFocusPath: elementPath(previousFocus),
        onKeyDown,
    });
    queueMicrotask(() => {
        if (!dialog.isConnected) {
            return;
        }

        const first = focusableElements(dialog)[0];
        (first || dialog).focus();
    });
}

export function closeDialog(dialog, restoreFocus = true) {
    const state = dialogStates.get(dialog);
    if (!state) {
        return;
    }

    document.removeEventListener('keydown', state.onKeyDown, true);
    dialogStates.delete(dialog);
    if (restoreFocus) {
        const restore = () => {
            const active = document.activeElement;
            if (active && active !== document.body && !dialog.contains(active)) {
                return;
            }

            const target = state.previousFocus?.isConnected
                ? state.previousFocus
                : resolveElementPath(state.previousFocusPath);
            if (target && !target.matches(':disabled')) {
                target.focus();
            }
        };

        restore();
        requestAnimationFrame(() => requestAnimationFrame(restore));
        window.setTimeout(restore, 100);
    }
}
