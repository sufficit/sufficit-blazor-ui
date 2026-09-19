export function initialize(root) {
  const onKeyDown = e => {
    const row = e.target;
    if (!(row instanceof HTMLTableRowElement) || !row.classList.contains('sui-table__row--interactive')
        || e.altKey || e.ctrlKey || e.metaKey) return;
    const rows = [...row.parentElement.children].filter(r => r.tabIndex >= -1 && r.classList.contains('sui-table__row--interactive'));
    const i = rows.indexOf(row);
    const next = { ArrowDown: i + 1, ArrowUp: i - 1, Home: 0, End: rows.length - 1 }[e.key];
    if (e.key === 'Enter' || e.key === ' ') row.click();
    else if (next !== undefined) rows[Math.max(0, Math.min(next, rows.length - 1))].focus();
    else return;
    e.preventDefault();
  };
  root.addEventListener('keydown', onKeyDown);
  return { dispose: () => root.removeEventListener('keydown', onKeyDown) };
}
