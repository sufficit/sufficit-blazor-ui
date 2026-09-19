# Standards adherence review — index

**Last review:** 2026-09-19T16:56Z · commit `9cbc7ea` (mode: regenerate — first run in this repository; `AGENTS.md` in this folder is the process file and is not part of the review).

Reviewers ran serially in one agent (no sub-agents available in this environment); every claim was re-verified against the working tree in this run.

**Status legend:** ✅ compliant · 🟡 partial · 🔴 missing · ⚪ partially not applicable
**Priority legend:** 🔴 high (security, data loss, broken clients) · 🔶 medium (interoperability, reliability) · 🔵 low (polish, future-proofing)

## Highest-risk findings

No 🔴 findings. Ranked findings, highest first:

| # | Priority | Finding | Document |
|---|---|---|---|
| 1 | 🔶 | `SUITable` interactive rows use `tr[role=button]`, an ARIA-incompatible role/element combination announced inconsistently by screen readers | [w3c-wcag22-wai-aria-component-accessibility.md](w3c-wcag22-wai-aria-component-accessibility.md) |
| 2 | 🔵 | `SUITextField`/`SUINumericField` take no `Name`; plain-HTML form posts depend on attribute-forwarding knowledge | [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md) |
| 3 | 🔵 | `SUISelect`/`SUIAutocomplete` selected value is invisible to plain HTML form posts (no hidden proxy input) | [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md) |
| 4 | 🔵 | `aria-sort` is never emitted by the library; sortable tables depend on the consumer remembering a code comment | [w3c-wcag22-wai-aria-component-accessibility.md](w3c-wcag22-wai-aria-component-accessibility.md) |
| 5 | 🔵 | Clipboard fallback depends on deprecated `document.execCommand("copy")` | [whatwg-clipboard-apis.md](whatwg-clipboard-apis.md) |
| 6 | 🔵 | `SUILink` does not default `rel` for `Target="_blank"`, unlike `SUIButton`/`SUIIconButton` | [rfc-2397-3986-uri-and-data-url-handling.md](rfc-2397-3986-uri-and-data-url-handling.md) |

## Documents by domain

### Web platform — accessibility

| Document | Standard | Status |
|---|---|---|
| [w3c-wcag22-wai-aria-component-accessibility.md](w3c-wcag22-wai-aria-component-accessibility.md) | W3C WCAG 2.2 (A/AA) + WAI-ARIA 1.2 | 🟡 partial |

### Web platform — HTML and browser APIs

| Document | Standard | Status |
|---|---|---|
| [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md) | WHATWG HTML — form participation, Popover API | 🟡 partial |
| [whatwg-clipboard-apis.md](whatwg-clipboard-apis.md) | WHATWG Clipboard API | 🟡 partial |

### Web platform — security

| Document | Standard | Status |
|---|---|---|
| [w3c-csp3-nonce-theme-provider.md](w3c-csp3-nonce-theme-provider.md) | W3C CSP Level 3 (nonce) | ✅ compliant |

### Data formats and identifiers

| Document | Standard | Status |
|---|---|---|
| [rfc-9562-guid-identifiers.md](rfc-9562-guid-identifiers.md) | RFC 9562 (UUIDs) | ✅ compliant |
| [rfc-3339-date-field-form-value.md](rfc-3339-date-field-form-value.md) | RFC 3339 (date representation) | ✅ compliant |
| [rfc-2397-3986-uri-and-data-url-handling.md](rfc-2397-3986-uri-and-data-url-handling.md) | RFC 3986 (URIs) + RFC 2397 (data: URLs) | 🟡 partial |

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
- No other code defects were noticed during verification (the theme-CSS injection hardening in `SUIThemeCssWriter.cs` was re-read and holds).

## Intentional divergences (preserved across documents)

- `SUICopyToClipboard` with `ChildContent` renders a click-only `<span>` — keyboard access is the child's job (accessibility + clipboard docs).
- Modal dialogs use `div[role=dialog]` with a manual focus trap instead of `<dialog>`/`showModal()`, so dialog content stays in Blazor's render tree (HTML doc).
- Menus degrade to inline (non-top-layer) rendering when the Popover API is missing — deliberate progressive enhancement (HTML doc).
- DOM ids use the compact `Guid…:N` form rather than RFC 9562's hyphenated canonical string (UUID doc).
- Human-visible/announced dates are culture-aware while machine values stay ISO `yyyy-MM-dd` (RFC 3339 doc).
- `SUILink` never rewrites or re-encodes `Href` (RFC 3986 doc).
- Theme tokens are published via a nonce-capable inline `<style>` instead of a static file, because palettes can come from DI per tenant (CSP3 doc).
