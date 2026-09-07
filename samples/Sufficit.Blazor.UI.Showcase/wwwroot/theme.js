(() => {
  const media = matchMedia('(prefers-color-scheme: dark)');
  const query = new URL(location.href).searchParams;
  const preview = query.has('preview');
  let settings = {};
  try { settings = JSON.parse(localStorage.getItem('sui-showcase-theme') || '{}'); } catch { }
  const bounded = (value, fallback, min, max) => Number.isFinite(Number(value)) ? Math.max(min, Math.min(max, Math.round(Number(value)))) : fallback;
  const normalize = value => ({
    mode: ['light', 'dark', 'system'].includes(value?.mode) ? value.mode : 'system',
    brand: ['amber', 'blue', 'red'].includes(value?.brand) ? value.brand : 'amber',
    density: value?.density === 'compact' ? 'compact' : 'comfortable',
    font: ['system', 'serif', 'mono'].includes(value?.font) ? value.font : 'system',
    fontSize: bounded(value?.fontSize ?? 16, 16, 14, 20),
    spaceUnit: bounded(value?.spaceUnit ?? 4, 4, 2, 6),
    radius: bounded(value?.radius ?? 8, 8, 0, 16),
    success: /^#[0-9a-f]{6}$/i.test(value?.success) ? value.success : null,
    error: /^#[0-9a-f]{6}$/i.test(value?.error) ? value.error : null,
  });
  if (preview) {
    settings = Object.fromEntries(query.entries());
    settings.mode = query.get('preview') === 'dark' ? 'dark' : 'light';
  }
  settings = normalize(settings);
  const snapshot = () => ({...settings, dark: settings.mode === 'dark' || settings.mode === 'system' && media.matches});
  const paint = () => document.documentElement.dataset.suiTheme = snapshot().dark ? 'dark' : 'light';
  paint();
  let receiver;
  const changed = () => { paint(); receiver?.invokeMethodAsync('SystemThemeChanged', snapshot()); };
  media.addEventListener('change', changed);
  window.suiShowcase = {
    connect: ref => { receiver = ref; performance.mark('sui-interactive'); return snapshot(); },
    disconnect: () => { receiver = undefined; },
    set: value => {
      settings = normalize(value); paint();
      if (!preview) { try { localStorage.setItem('sui-showcase-theme', JSON.stringify(settings)); } catch { } }
      return snapshot();
    },
  };
})();
