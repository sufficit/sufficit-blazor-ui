// Positions the sliding pill indicator under the active tab. The Razor side
// calls update(root) after every render; initialize keeps it glued when the
// layout shifts (resize, webfont swap). Tab switches animate; first paint,
// resizes and font swaps reposition instantly.

const position = (root, instant) => {
  const track = root.querySelector('[role="tablist"]');
  if (!track) return;
  const indicator = track.querySelector('.sui-sliding-tabs__indicator');
  const active = track.querySelector('[role="tab"].is-active');
  if (!indicator || !active) return;

  if (instant) indicator.style.transition = 'none';
  indicator.style.left = `${active.offsetLeft}px`;
  indicator.style.width = `${active.offsetWidth}px`;
  if (!track.classList.contains('is-positioned')) {
    // Commit the initial jump before enabling the transition, so the pill
    // never slides in from the left edge on first paint.
    void indicator.offsetWidth;
    track.classList.add('is-positioned');
  } else if (instant) {
    void indicator.offsetWidth;
    indicator.style.transition = '';
  }
};

export function initialize(root) {
  position(root, true);

  const observer = new ResizeObserver(() => position(root, true));
  observer.observe(root);

  if (typeof document !== 'undefined' && document.fonts && document.fonts.ready)
    document.fonts.ready.then(() => position(root, true)).catch(() => {});

  return { dispose: () => observer.disconnect() };
}

export function update(root) {
  position(root, false);
}
