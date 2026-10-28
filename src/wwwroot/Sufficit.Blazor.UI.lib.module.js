// Blazor JS initializer for Sufficit.Blazor.UI, loaded automatically by
// blazor.web.js / blazor.webassembly.js / blazor.server.js.
//
// SUI components never compute an inline style attribute: a page whose
// Content-Security-Policy has style-src-attr 'none' (or no 'unsafe-inline')
// ignores it, both in prerendered HTML and when Blazor sets it later. Computed
// values travel in data-sui-style instead ("--sui-a:1fr 2fr;--sui-b:12rem") and
// are applied here through CSSOM, which that directive does not govern. One
// document-level observer covers prerender, static SSR, enhanced navigation and
// interactive renders alike, so no component needs its own JS for this.
//
// Only --sui-* custom properties are honored, so markup injected into the page
// cannot use the attribute to restyle arbitrary properties.

const ATTRIBUTE = 'data-sui-style';
const NAME = /^--sui-[a-z0-9-]+$/;
const UNSAFE = /[;{}<>\\@!]|\/\*|\*\/|expression\(|javascript:/i;
const applied = new WeakMap();
let observer = null;

function parse(text) {
    const declarations = new Map();
    for (const part of (text || '').split(';')) {
        const colon = part.indexOf(':');
        if (colon < 1) continue;
        const name = part.slice(0, colon).trim();
        const value = part.slice(colon + 1).trim();
        if (!NAME.test(name) || !value || value.length > 2100 || UNSAFE.test(value)) continue;
        // Only image properties may fetch, and the browser still applies img-src.
        if (/url\(/i.test(value) && !name.endsWith('-image')) continue;
        declarations.set(name, value);
    }
    return declarations;
}

/** Applies (and un-applies) the declarations of one element's data-sui-style. */
export function applySuiStyle(element) {
    const next = parse(element.getAttribute(ATTRIBUTE));
    const previous = applied.get(element);
    if (previous) {
        for (const name of previous.keys()) {
            if (!next.has(name)) element.style.removeProperty(name);
        }
    }
    for (const [name, value] of next) element.style.setProperty(name, value);
    if (next.size) applied.set(element, next); else applied.delete(element);
}

// Rewriting the style attribute (a consumer Style parameter changing, where CSP
// allows it) resets the whole inline declaration block. Put back only what is
// missing, so this never feeds its own mutations.
function repair(element) {
    const declarations = applied.get(element);
    if (!declarations) return;
    for (const [name, value] of declarations) {
        if (!element.style.getPropertyValue(name)) element.style.setProperty(name, value);
    }
}

function scan(node) {
    if (node.nodeType !== 1) return;
    if (node.hasAttribute(ATTRIBUTE)) applySuiStyle(node);
    for (const element of node.querySelectorAll(`[${ATTRIBUTE}]`)) applySuiStyle(element);
}

function handle(records) {
    for (const record of records) {
        if (record.type === 'childList') record.addedNodes.forEach(scan);
        else if (record.attributeName === ATTRIBUTE) applySuiStyle(record.target);
        else repair(record.target);
    }
}

/** Starts applying data-sui-style across the document. Idempotent. */
export function startSuiStyle() {
    if (observer || typeof document === 'undefined' || typeof MutationObserver === 'undefined') return;
    observer = new MutationObserver(handle);
    observer.observe(document.documentElement, {
        subtree: true,
        childList: true,
        attributes: true,
        attributeFilter: [ATTRIBUTE, 'style'],
    });
    scan(document.documentElement);
}

// Blazor Web App initializers, then the classic ones (standalone WebAssembly,
// blazor.server.js). Starting before Blazor applies the prerendered values as
// early as the module loads.
export function beforeWebStart() { startSuiStyle(); }
export function afterWebStarted() { startSuiStyle(); }
export function beforeStart() { startSuiStyle(); }
export function afterStarted() { startSuiStyle(); }
