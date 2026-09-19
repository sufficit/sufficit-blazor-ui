# RFC 3986 + RFC 2397 — URI handling and data: URLs

**Status:** 🟡 partial — the library is a passive URI emitter (consumer-supplied `href`/`target`/`rel`) with safe defaults on anchor-buttons but not on `SUILink`; the only `data:` URL in the repository is an intentional empty favicon.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`

## What the standard requires

Only the requirements that touch this project:

- URIs, when constructed, must be well-formed and percent-encoded where required (RFC 3986 §2).
- `data:` URLs follow the `data:[<mediatype>][;base64],<data>` scheme (RFC 2397 §3); an empty payload (`data:,`) is the canonical way to reference "nothing".

## Where the project complies

- `data:` URL usage is minimal and intentional — empty favicon to suppress the browser's default 404/console error: `samples/Sufficit.Blazor.UI.Catalog/Components/App.razor:13` (rationale in the comment at `:11`–`:12`) and `samples/Sufficit.Blazor.UI.Showcase/wwwroot/index.html:9`. No base64 payloads, no inline media.
- Anchor-buttons default `rel` for new tabs — external/new-tab anchors get `noopener noreferrer` without consumer opt-in: `src/Components/Actions/SUIButton.razor:92`–`:94`, `src/Components/Actions/SUIIconButton.razor:90`.
- `SUILink` passes consumer values through untouched — `href="@Href" target="@Target" rel="@Rel"` `src/Components/Actions/SUILink.razor:3` (parameters `:8`–`:16`); the library never rewrites or re-encodes URIs.
- The only URLs the library itself constructs are static asset paths for JS module imports, e.g. `./_content/Sufficit.Blazor.UI/Components/Actions/SUICopyToClipboard.razor.js` `src/Components/Actions/SUICopyToClipboard.razor:96` — constant strings, no user input.

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| 🔵 | `SUILink` does not default `rel` when `Target` opens a new tab, unlike `SUIButton`/`SUIIconButton` (`src/Components/Actions/SUILink.razor:3` vs `SUIButton.razor:92`–`:94`) | A consumer setting `Target="_blank"` on `SUILink` silently loses the reverse-tabnabbing protection the button family gets for free | Mirror the anchor-button default (`noopener noreferrer` when `Target` is `_blank`/new-window and `Rel` is unset) |

Searches performed for claimed absences: `System.Uri`, `UriBuilder`, `EscapeUriString`, `EscapeDataString`, `UrlEncoder`, `QueryString` in `*.cs`, `*.js` under `src/` (none); `encodeURIComponent`/`decodeURIComponent` in `src/Components/**/*.js` (none); `data:` in `*.cs`, `*.js`, `*.razor`, `*.html` under `src/`+`samples/` (only the two favicons above).

## Intentional divergences

- `SUILink` never validates or rewrites `Href` by design: an internal router link and an external URL are equally valid, and Blazor/the browser own resolution (RFC 3986 §5).

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- RFC 3986 — <https://www.rfc-editor.org/rfc/rfc3986>
- RFC 2397 — <https://www.rfc-editor.org/rfc/rfc2397>
- Related: [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md)
