# Standards adherence review — index

**Last review:** 2026-09-19T19:37Z · `e1b1391` + doc (fix pass at 18:01Z resolved the six ranked findings of the 16:56Z review at `9cbc7ea`; at 19:37Z the icon-button name became a documented contract; `AGENTS.md` in this folder is the process file and is not part of the review).

Every changed claim was re-verified against the code in this run; unit tests (bUnit) and Chromium browser tests (axe WCAG 2.2 AA sweeps + grid keyboard) pass.

**Status legend:** ✅ compliant · 🟡 partial · 🔴 missing · ⚪ partially not applicable
**Priority legend:** 🔴 high (security, data loss, broken clients) · 🔶 medium (interoperability, reliability) · 🔵 low (polish, future-proofing)

## Highest-risk findings

No open findings. The icon-button accessible name (last open 🔵) became a documented contract at 19:37Z — see *Intentional divergences*.

Resolved in the 18:01Z fix pass (were #1–#6 at 16:56Z):

| Was | Finding | Resolution |
|---|---|---|
| 🔶 | `SUITable` interactive rows used `tr[role=button]` | `role=grid` with row focus, roving `tabindex`, arrow/Home/End/Enter/Space in a target-aware module, optional `RowAriaLabelFunc` |
| 🔵 | `SUITextField`/`SUINumericField` took no `Name` | First-class `Name` (also added to `SUISwitch`, with `FormValue`) |
| 🔵 | `SUISelect`/`SUIAutocomplete` selection invisible to plain posts | Hidden proxy input when `Name` is set; invariant token, `ToFormValueFunc` override |
| 🔵 | `aria-sort` never emitted | `SUITh` renders `aria-sort`, fed by the nested `SUITableSortLabel` or an explicit `SortDirection` |
| 🔵 | Clipboard fallback on deprecated `execCommand` | Kept for plain HTTP, feature-detected, explicit "HTTPS" reason when unavailable |
| 🔵 | `SUILink` had no default `rel` for new tabs | `noopener noreferrer` default when `Target` is set and `Rel` is not |

## Documents by domain

### Web platform — accessibility

| Document | Standard | Status |
|---|---|---|
| [w3c-wcag22-wai-aria-component-accessibility.md](w3c-wcag22-wai-aria-component-accessibility.md) | W3C WCAG 2.2 (A/AA) + WAI-ARIA 1.2 | ✅ compliant |

### Web platform — HTML and browser APIs

| Document | Standard | Status |
|---|---|---|
| [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md) | WHATWG HTML — form participation, Popover API | ✅ compliant |
| [whatwg-clipboard-apis.md](whatwg-clipboard-apis.md) | WHATWG Clipboard API | ✅ compliant |

### Web platform — security

| Document | Standard | Status |
|---|---|---|
| [w3c-csp3-nonce-theme-provider.md](w3c-csp3-nonce-theme-provider.md) | W3C CSP Level 3 (nonce) | ✅ compliant |

### Data formats and identifiers

| Document | Standard | Status |
|---|---|---|
| [rfc-9562-guid-identifiers.md](rfc-9562-guid-identifiers.md) | RFC 9562 (UUIDs) | ✅ compliant |
| [rfc-3339-date-field-form-value.md](rfc-3339-date-field-form-value.md) | RFC 3339 (date representation) | ✅ compliant |
| [rfc-2397-3986-uri-and-data-url-handling.md](rfc-2397-3986-uri-and-data-url-handling.md) | RFC 3986 (URIs) + RFC 2397 (data: URLs) | ✅ compliant |

## Evaluated without document

All searches below ran over `src/`, `samples/`, `tests/`, `scripts/`, `eng/`, `.github/` (`*.cs`, `*.js`, `*.razor`, `*.json`, `*.yml`, `*.mjs`), excluding `bin/`, `obj/`, `node_modules/`, `.worktrees/`, `artifacts/`.

- **HTTP semantics — RFC 9110, 9111, 9112, 9113, 6585, 9457, 8288, 5789, 7240, 9205, draft-ietf-httpapi-idempotency-key-header, draft-ietf-httpapi-ratelimit-headers, RFC 9745 + 8594.** The repository is a rendering library; it exposes no HTTP interface and produces no HTTP responses. Searches: `HttpClient|fetch(`, `MapGet|MapPost|controller|ApiController`, `problem ?details|www-authenticate|ratelimit|idempotency|x-request-id` — no matches in source (sample hosts only map Razor components: `samples/Sufficit.Blazor.UI.Catalog/Program.cs:33`).
- **OAuth and identity — RFC 6749, 6750, 7636, 8628, 8414, 9728, 8707, 7591, 7009, 7662, 9700, 7519, 8725, 9068, 9449, 8693, 7617, 6265, OpenID Connect Core/Discovery.** No authentication, token, session or cookie code exists in the library. Searches: `oauth|openid|oidc|clientsecret|jwtbearer|challenge`, `authorization|bearer|apikey`, `document.cookie|setcookie|httpcontext` — only the framework's own `Error.razor` template touches `HttpContext` (request id display, sample-only); the OAuth string in `samples/**/obj/*.json` is a transitive NuGet dependency graph, not code.
- **RFC 8259 / 7493 (JSON).** The library serializes nothing to JSON; component interop uses typed primitives and element references. Searches: `JsonSerializer|JsonDocument|System.Text.Json`, `JSON.` in `*.js` under `src/` — none. (The Showcase sample's `theme.js` uses `JSON.parse/stringify` for its own localStorage settings only — no interchange format with other systems.)
- **RFC 4648 (Base64).** No Base64 codec anywhere: searches `base64|btoa|atob` in `src/` — none.
- **Transport and network security — RFC 6797 (HSTS), 7239 (Forwarded), 8305, 6724, 6890/1918/4193 (IP), 6598, 8446 (TLS), 6455 (WebSocket), 9116 (security.txt), WHATWG Fetch (CORS), OWASP SSRF.** The repo contains no reverse-proxy config, no header middleware, no outbound HTTP, no WebSocket/SSE channel, no SSRF-capable surface, and no `security.txt`. Searches: `usehsts|forwardedheaders|addcors|usecors|httpsredirection`, `websocket|signalr|server-sent|event-stream`, `find -name security.txt`, `XMLHttpRequest|xhr` — none in source.
- **Streaming/APIs/observability — WHATWG SSE, JSON-RPC 2.0, Model Context Protocol, OpenAPI 3.1, W3C Trace Context, OpenTelemetry semantic conventions, Standard Webhooks.** No streaming endpoint, RPC layer, API description, trace propagation or webhook handling exists. Searches: `grpc|json-rpc|jsonrpc`, `traceparent|opentelemetry|activitysource|diagnosticlistener|correlation` — none in `src/` (the Catalog `Error.razor` reads `Activity.Current?.Id`, the framework's exception template, sample-only).

## Out-of-scope findings

- `src/Components/DataDisplay/SUIIconMarkup.cs` was an untracked file in the working tree during this review (not part of commit `9cbc7ea`) and was therefore excluded from the analysis. It was subsequently committed by another session as `84c3a07` (inline-SVG validation); it has not yet been reviewed against these standards — cover it in the next revision.
- The visual baseline test `Catalog_MatchesCommittedVisualBaselines` fails locally (catalog-light-desktop 1440×5380 expected, 1440×5398 rendered) on an unmodified `3e152ec` checkout as well — environment/fonts, not these changes; CI is the reference.
- No other code defects were noticed during verification (the theme-CSS injection hardening in `SUIThemeCssWriter.cs` was re-read and holds).

## Intentional divergences (preserved across documents)

- `SUIIconButton.Title` stays nullable: the name may come from `aria-label`/`aria-labelledby` or visible text; the obligation is documented inline on the parameter (accessibility doc).
- `SUICopyToClipboard` with `ChildContent` renders a click-only `<span>` — keyboard access is the child's job (accessibility + clipboard docs).
- Modal dialogs use `div[role=dialog]` with a manual focus trap instead of `<dialog>`/`showModal()`, so dialog content stays in Blazor's render tree (HTML doc).
- Menus degrade to inline (non-top-layer) rendering when the Popover API is missing — deliberate progressive enhancement (HTML doc).
- The clipboard fallback keeps deprecated `document.execCommand("copy")` for plain-HTTP hosts, feature-detected (clipboard doc).
- DOM ids use the compact `Guid…:N` form rather than RFC 9562's hyphenated canonical string (UUID doc).
- Human-visible/announced dates are culture-aware while machine values stay ISO `yyyy-MM-dd` (RFC 3339 doc).
- `SUILink` never rewrites or re-encodes `Href` (RFC 3986 doc).
- Theme tokens are published via a nonce-capable inline `<style>` instead of a static file, because palettes can come from DI per tenant (CSP3 doc).
