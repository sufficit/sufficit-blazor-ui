# WHATWG Clipboard API — SUICopyToClipboard

**Status:** ✅ compliant — the async Clipboard API is the primary path; the legacy fallback for non-secure contexts is feature-detected and, once a browser drops `execCommand`, fails with an explicit "HTTPS required" reason instead of an opaque error.
**Last reviewed:** 2026-09-19T18:01Z · working tree over `3e152ec` (fix pass for the 16:56Z findings)
**Project role:** clipboard writer only (copy to clipboard); no paste/read path exists.

## What the standard requires

Only the requirements that touch this project:

- Use `navigator.clipboard.writeText()` for writing text (Clipboard API §"Writing to the clipboard").
- The async clipboard API is only exposed in secure contexts; non-secure pages need a fallback or a clear failure.
- Permission/feature-policy failures must be handled, not swallowed.

## Where the project complies

- Primary path gated on availability and secure context — `src/Components/Actions/SUICopyToClipboard.razor.js:2`–`:3` (`if (navigator.clipboard && window.isSecureContext) await navigator.clipboard.writeText(text)`).
- Legacy fallback for non-secure contexts, feature-detected — `src/Components/Actions/SUICopyToClipboard.razor.js:7`–`:9` throws "este navegador só copia em páginas HTTPS" when `document.execCommand` is gone; otherwise `:11`–`:24` (offscreen `textarea`, `select()`, `document.execCommand("copy")` with a thrown error on `false`, cleanup in `finally` via `area.remove()`). Rationale recorded beside the catch, `src/Components/Actions/SUICopyToClipboard.razor:106`–`:108`.
- Failures surfaced, not swallowed — `src/Components/Actions/SUICopyToClipboard.razor:104`–`:115` (`catch (Exception ex)` → error toast; exception message trimmed to the first line and 160 chars so interop stack traces never reach the UI).
- Module lifecycle — lazily imported per instance (`import` call at `src/Components/Actions/SUICopyToClipboard.razor:96`), disposed with the circuit-gone case handled at `:127` (`JSDisconnectedException`).

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| — | None open. The deprecated `execCommand` fallback is kept deliberately (plain-HTTP intranet hosts still use it) but is now feature-detected with an explicit HTTPS message — see *Intentional divergences*. | — | Re-check when a target browser announces `execCommand` removal. |

Searches performed for claimed absences: `navigator.clipboard` in `*.js` under `src/` (single occurrence, `SUICopyToClipboard.razor.js`); no `readText`/`read()` anywhere in `src/` (read path intentionally absent).

## Intentional divergences

- The `document.execCommand("copy")` fallback (deprecated) stays for non-secure contexts; its removal degrades to a clear "HTTPS required" toast, not to a silent or opaque failure.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |
| 2026-09-19T18:01Z | `3e152ec` + fix | `execCommand` feature detection with explicit HTTPS reason; status → compliant |

## References

- WHATWG Clipboard API — <https://www.w3.org/TR/clipboard-apis/>
- MDN: Clipboard API secure-context note — <https://developer.mozilla.org/docs/Web/API/Clipboard_API>
- Related: [w3c-csp3-nonce-theme-provider.md](w3c-csp3-nonce-theme-provider.md)
