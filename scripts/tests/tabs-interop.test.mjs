import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
const source = await readFile(new URL('../../src/Components/Navigation/SUITabs.razor.js', import.meta.url), 'utf8');
const { initialize, reveal } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
class Element {
  isConnected = true;
  listeners = new Map();
  addEventListener(name, handler) { this.listeners.set(name, handler); }
  removeEventListener(name, handler) { assert.equal(this.listeners.get(name), handler); this.listeners.delete(name); }
  getAttribute(name) { return this.attributes?.[name] ?? null; }
  querySelector() { return this.child ?? null; }
  getBoundingClientRect() { return this.box; }
}
globalThis.HTMLElement = Element;
test('missing or detached roots are safe during navigation', () => {
  const detached = new Element(); detached.isConnected = false;
  for (const root of [null, undefined, detached]) {
    assert.equal(initialize(root), null);
    assert.doesNotThrow(() => reveal(root));
  }
});
test('keyboard prevention and listener disposal remain functional', () => {
  const root = new Element(), tab = new Element();
  tab.attributes = { role: 'tab' }; tab.parentElement = root;
  const interop = initialize(root);
  let prevented = false;
  root.listeners.get('keydown')({ target: tab, key: 'ArrowRight', preventDefault() { prevented = true; } });
  assert.equal(prevented, true);
  interop.dispose(); assert.equal(root.listeners.size, 0);
});
test('selected horizontal tab is revealed without moving vertical tabs', () => {
  const root = new Element(), list = new Element(), tab = new Element();
  root.child = list; list.child = tab; list.scrollLeft = 0;
  list.box = { left: 0, right: 100 }; tab.box = { left: 90, right: 130 };
  reveal(root); assert.equal(list.scrollLeft, 30);
  list.attributes = { 'aria-orientation': 'vertical' };
  reveal(root); assert.equal(list.scrollLeft, 30);
});
