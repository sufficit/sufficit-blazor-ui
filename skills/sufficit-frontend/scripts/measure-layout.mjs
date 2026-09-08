export default function measureLayout({ selectors = [] } = {}) {
  if (!Array.isArray(selectors) || selectors.some(s => typeof s !== 'string'))
    throw new TypeError('selectors must be an array of CSS selector strings');
  const elements = selectors.map(selector => {
    let matches;
    try { matches = document.querySelectorAll(selector); }
    catch { return { selector, status: 'invalid-selector' }; }
    if (!matches.length) return { selector, status: 'missing', count: 0 };
    const element = matches[0];
    const rect = element.getBoundingClientRect();
    const css = getComputedStyle(element);
    // Ancestors such as closed details or display:none can suppress the box.
    const visible = element.getClientRects().length > 0 && rect.width > 0 && rect.height > 0
      && css.visibility !== 'hidden' && css.visibility !== 'collapse';
    return {
      selector, status: visible ? 'rendered' : 'no-visible-box', count: matches.length,
      rect: { top: rect.top, right: rect.right, bottom: rect.bottom, left: rect.left,
        width: rect.width, height: rect.height },
      style: { display: css.display, gap: css.gap, padding: css.padding,
        margin: css.margin, fontSize: css.fontSize, lineHeight: css.lineHeight,
        color: css.color, backgroundColor: css.backgroundColor },
      horizontalOverflow: element.scrollWidth > element.clientWidth
    };
  });
  return {
    viewport: { width: innerWidth, height: innerHeight, devicePixelRatio },
    pageHorizontalOverflow: document.documentElement.scrollWidth > innerWidth,
    elements,
    verticalGaps: elements.slice(1).map((next, i) => ({
      from: elements[i].selector, to: next.selector,
      pixels: elements[i].status === 'rendered' && next.status === 'rendered'
        ? next.rect.top - elements[i].rect.bottom : null
    }))
  };
}
