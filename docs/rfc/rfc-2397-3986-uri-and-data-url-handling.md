# RFC 3986 + RFC 2397 — URI handling and data: URLs

**Status:** ✅ compliant — the library is a passive URI emitter (consumer-supplied `href`/`target`/`rel`) with the same safe new-tab `rel` default on anchor-buttons and `SUILink`; the only `data:` URL in the repository is an intentional empty favicon.
**Last reviewed:** 2026-09-19T18:01Z · working tree over `3e152ec` (fix pass for the 16:56Z findings)

## What the standard requires

Only the requirements that touch this project:

- URIs, when constructed, must be well-formed and percent-encoded where required (RFC 3986 §2).
- `data:` URLs follow the `data:[<mediatype>][;base64],<data>` scheme (RFC 2397 §3); an empty payload (`data:,`) is the canonical way to reference "nothing".

## Where the project complies

- `data:` URL usage is minimal and intentional — empty favicon to suppress the browser's default 404/console error: `samples/Sufficit.Blazor.UI.Catalog/Components/App.razor:13` (rationale in the comment at `:11`–`:12`) and `samples/Sufficit.Blazor.UI.Showcase/wwwroot/index.html:9`. No base64 payloads, no inline media.
- Anchor-buttons default `rel` for new tabs — external/new-tab anchors get `noopener noreferrer` without consumer opt-in: `src/Components/Actions/SUIButton.razor:92`–`:94`, `src/Components/Actions/SUIIconButton.razor:90`.
- `SUILink` defaults `rel` like the anchor-buttons — `rel="@EffectiveRel"` `src/Components/Actions/SUILink.razor:3`, `noopener noreferrer` when `Target` is set and not `_self` and `Rel` is unset (`:28`–`:31`); an explicit `Rel` always wins. Covered by `tests/Sufficit.Blazor.UI.Tests/StandardsDefaultsTests.cs` (`LinkDefaultsRelLikeTheAnchorButtons`).
- `SUILink` passes `Href`/`Target` through untouched (`src/Components/Actions/SUILink.razor:3`); the library never rewrites or re-encodes URIs.
- The only URLs the library itself constructs are static asset paths for JS module imports, e.g. `./_content/Sufficit.Blazor.UI/Components/Actions/SUICopyToClipboard.razor.js` `src/Components/Actions/SUICopyToClipboard.razor:96` — constant strings, no user input.

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| — | None open. The 16:56Z `SUILink` `rel` gap is resolved (see above). | — | — |

Searches performed for claimed absences: `System.Uri`, `UriBuilder`, `EscapeUriString`, `EscapeDataString`, `UrlEncoder`, `QueryString` in `*.cs`, `*.js` under `src/` (none); `encodeURIComponent`/`decodeURIComponent` in `src/Components/**/*.js` (none); `data:` in `*.cs`, `*.js`, `*.razor`, `*.html` under `src/`+`samples/` (only the two favicons above).

## Intentional divergences

- `SUILink` never validates or rewrites `Href` by design: an internal router link and an external URL are equally valid, and Blazor/the browser own resolution (RFC 3986 §5).

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |
| 2026-09-19T18:01Z | `3e152ec` + fix | `SUILink` new-tab `rel` default; status → compliant |

## References

- RFC 3986 — <https://www.rfc-editor.org/rfc/rfc3986>
- RFC 2397 — <https://www.rfc-editor.org/rfc/rfc2397>
- Related: [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md)
