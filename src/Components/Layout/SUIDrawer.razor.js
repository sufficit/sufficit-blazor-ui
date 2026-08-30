export function initialize(d, r, b, v) {
  const m = v === 'responsive' ? matchMedia(`(max-width:${b - .02}px)`) : null;
  let c = v === 'temporary' || !!m?.matches, o = false, p;
  const s = () => {
    c = v === 'temporary' || !!m?.matches;
    r.invokeMethodAsync('SetCompactStateAsync', c);
  };
  const k = e => {
    if (!o || !c) return;
    if (e.key === 'Escape') {
      e.preventDefault();
      r.invokeMethodAsync('CloseFromKeyboardAsync');
      return;
    }
    if (e.key !== 'Tab') return;
    const a = [...d.querySelectorAll('a[href],button:not(:disabled),[tabindex]:not([tabindex="-1"])')];
    const f = a[0] || d, l = a.at(-1) || d;
    if (e.shiftKey && (document.activeElement === f || !d.contains(document.activeElement))) {
      e.preventDefault(); l.focus();
    } else if (!e.shiftKey && document.activeElement === l) {
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
        p?.focus();
      }
    },
    dispose() {
      m?.removeEventListener('change', s);
      document.removeEventListener('keydown', k, true);
    },
  };
}
