export function initialize(root) {
  const preventNavigation = event => {
    const tab = event.target;
    if (!(tab instanceof HTMLElement) || tab.getAttribute('role') !== 'tab'
        || event.altKey || event.ctrlKey || event.metaKey) return;
    const vertical = tab.parentElement.getAttribute('aria-orientation') === 'vertical';
    const keys = vertical ? ['ArrowUp', 'ArrowDown', 'Home', 'End'] : ['ArrowLeft', 'ArrowRight', 'Home', 'End'];
    if (keys.includes(event.key)) event.preventDefault();
  };
  root.addEventListener('keydown', preventNavigation);
  return { dispose: () => root.removeEventListener('keydown', preventNavigation) };
}

export function reveal(root) {
  const list = root.querySelector('[role="tablist"]');
  const tab = list?.querySelector('[aria-selected="true"]');
  if (!tab || list.getAttribute('aria-orientation') === 'vertical') return;
  const box = list.getBoundingClientRect(), active = tab.getBoundingClientRect();
  if (active.left < box.left) list.scrollLeft += active.left - box.left;
  else if (active.right > box.right) list.scrollLeft += active.right - box.right;
}
