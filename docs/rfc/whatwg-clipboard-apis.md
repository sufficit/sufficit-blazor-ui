# WHATWG Clipboard API — SUICopyToClipboard

**Status:** 🟡 partial — the async Clipboard API is the primary path with a guarded legacy fallback and surfaced errors; the fallback relies on deprecated `document.execCommand`.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`
**Project role:** clipboard writer only (copy to clipboard); no paste/read path exists.

## What the standard requires

Only the requirements that touch this project:

- Use `navigator.clipboard.writeText()` for writing text (Clipboard API §"Writing to the clipboard").
- The async clipboard API is only exposed in secure contexts; non-secure pages need a fallback or a clear failure.
- Permission/feature-policy failures must be handled, not swallowed.

## Where the project complies

- Primary path gated on availability and secure context — `src/Components/Actions/SUICopyToClipboard.razor.js:2`–`:3` (`if (navigator.clipboard && window.isSecureContext) await navigator.clipboard.writeText(text)`).
- Legacy fallback for non-secure contexts — `src/Components/Actions/SUICopyToClipboard.razor.js:7`–`:21` (offscreen `textarea`, `select()`, `document.execCommand("copy")` with a thrown error on `false`, cleanup in `finally` via `area.remove()`).
- Failures surfaced, not swallowed — `src/Components/Actions/SUICopyToClipboard.razor:104`–`:111` (`catch (Exception ex)` → error toast; exception message trimmed to the first line and 160 chars so interop stack traces never reach the UI).
- Module lifecycle — lazily imported per instance (`import` call at `src/Components/Actions/SUICopyToClipboard.razor:96`), disposed with the circuit-gone case handled at `:124` (`JSDisconnectedException`).

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| 🔵 | The fallback path depends on deprecated `document.execCommand("copy")` (`src/Components/Actions/SUICopyToClipboard.razor.js:15`) | Browsers may remove `execCommand`; on plain HTTP the copy button degrades to an error toast when that happens | Keep the fallback but track removal timelines; alternatively accept secure-context-only copying and turn the fallback into an explicit "use HTTPS" message |

Searches performed for claimed absences: `navigator.clipboard` in `*.js` under `src/` (single occurrence, `SUICopyToClipboard.razor.js`); no `readText`/`read()` anywhere in `src/` (read path intentionally absent).

## Intentional divergences

- None beyond the documented fallback above.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- WHATWG Clipboard API — <https://www.w3.org/TR/clipboard-apis/>
- MDN: Clipboard API secure-context note — <https://developer.mozilla.org/docs/Web/API/Clipboard_API>
- Related: [w3c-csp3-nonce-theme-provider.md](w3c-csp3-nonce-theme-provider.md)
