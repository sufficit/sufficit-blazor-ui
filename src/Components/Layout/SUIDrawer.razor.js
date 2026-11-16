export function initialize(d, r, b, v) {
  const m = v === 'responsive' ? matchMedia(`(max-width:${b - .02}px)`) : null;
  let c = v === 'temporary' || !!m?.matches, o = false, p;
  const s = () => {
    c = v === 'temporary' || !!m?.matches;
    r.invokeMethodAsync('SetCompactStateAsync', c);
  };
  const k = e => {
    if (!o || !c || e.defaultPrevented) return;
    // A dialog above the drawer owns its own focus and Escape lifecycle.
    if ([...document.querySelectorAll('dialog[open],[role="dialog"][aria-modal="true"]')]
      .some(el => el !== d && !el.contains(d) && el.getClientRects().length && getComputedStyle(el).visibility !== 'hidden')) return;
    if (e.key === 'Escape') {
      // Native interactive popovers dismiss first, without closing their owner.
      if (document.querySelector(':popover-open:not([role="tooltip"])')) return;
      e.preventDefault();
      r.invokeMethodAsync('CloseFromKeyboardAsync');
      return;
    }
    if (e.key !== 'Tab') return;
    const a = [...d.querySelectorAll('a[href],button,input:not([type="hidden"]),select,textarea,[tabindex]')]
      .filter(el => el.tabIndex >= 0 && !el.matches(':disabled') && !el.closest('[inert]') &&
        el.getClientRects().length && getComputedStyle(el).visibility !== 'hidden');
    const f = a[0] || d, l = a.at(-1) || d;
    const outside = document.activeElement === d || !d.contains(document.activeElement);
    if (e.shiftKey && (document.activeElement === f || outside)) {
      e.preventDefault(); l.focus();
    } else if (!e.shiftKey && (document.activeElement === l || outside)) {
      e.preventDefault(); f.focus();
    }
  };
  m?.addEventListener('change', s);
  document.addEventListener('keydown', k, true);
  s();
  return {
    setOpen(n, x) {
      const w = o && c;
      o = n; c = x;
      if (o && c && !w) {
        p = document.activeElement;
        queueMicrotask(() => (d.querySelector('[data-sui-drawer-close]') || d).focus());
      } else if ((!o || !c) && w) {
        if (p?.isConnected && !p.closest('[inert]')) p.focus();
      }
    },
    dispose() {
      m?.removeEventListener('change', s);
      document.removeEventListener('keydown', k, true);
    },
  };
}
