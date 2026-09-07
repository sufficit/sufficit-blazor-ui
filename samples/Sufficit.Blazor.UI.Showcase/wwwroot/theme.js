(() => {
  const media = matchMedia('(prefers-color-scheme: dark)');
  let mode = 'system', brand = 'amber', density = 'comfortable';
  try {
    const saved = JSON.parse(localStorage.getItem('sui-showcase-theme') || '{}');
    if (['light', 'dark', 'system'].includes(saved.mode)) mode = saved.mode;
    if (['amber', 'blue', 'red'].includes(saved.brand)) brand = saved.brand;
    if (['comfortable', 'compact'].includes(saved.density)) density = saved.density;
  } catch { /* Storage can be unavailable in private browsing. */ }
  const snapshot = () => ({mode, brand, density, dark: mode === 'dark' || mode === 'system' && media.matches});
  const paint = () => document.documentElement.dataset.suiTheme = snapshot().dark ? 'dark' : 'light';
  paint();
  let receiver;
  const changed = () => { paint(); receiver?.invokeMethodAsync('SystemThemeChanged', snapshot()); };
  media.addEventListener('change', changed);
  window.suiShowcase = {
    connect: ref => { receiver = ref; return snapshot(); },
    disconnect: () => { receiver = undefined; },
    set: (m, b, d) => {
      mode = m; brand = b; density = d; paint();
      try { localStorage.setItem('sui-showcase-theme', JSON.stringify({mode, brand, density})); } catch { }
      return snapshot();
    },
  };
})();
