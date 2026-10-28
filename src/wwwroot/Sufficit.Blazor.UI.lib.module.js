// Blazor JS initializer for Sufficit.Blazor.UI, loaded automatically by
// blazor.web.js / blazor.webassembly.js / blazor.server.js. The work lives in
// sui-style.js (stable name) so hosts without initializers can start it too.

import { startSuiStyle } from './sui-style.js';

export { applySuiStyle, startSuiStyle } from './sui-style.js';

export function beforeWebStart() { startSuiStyle(); }
export function afterWebStarted() { startSuiStyle(); }
export function beforeStart() { startSuiStyle(); }
export function afterStarted() { startSuiStyle(); }
