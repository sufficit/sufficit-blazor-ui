# W3C CSP Level 3 — nonce support in SUIThemeProvider

**Status:** ✅ compliant for what a component library can do — the inline token `<style>` accepts a consumer-supplied nonce, and its content is structurally validated so theme values cannot escape into markup.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`
**Project role:** content producer inside a host page; the CSP itself is the host's policy (headers are outside this repository).

## What the standard requires

Only the requirements that touch this project:

- Inline `<style>` elements require a matching `nonce-…` source in `style-src` (CSP3 §"nonces"); nonce values must be fresh per response and unguessable — the host's responsibility.
- Content injected into an approved inline element must not become an injection vector of its own.

## Where the project complies

- Nonce forwarded verbatim to the inline element — `<style nonce="@Nonce">` `src/Themes/SUIThemeProvider.razor:9`; `Nonce` parameter with host-policy guidance in its doc comment `:23`–`:30`.
- Inline style content is generated, not templated — `ToCssVars` delegates to `SUIThemeCssWriter.Write` `src/Themes/SUIThemeProvider.razor:50`.
- Structural validation closes style/markup injection from theme values — `src/Themes/SUIThemeCssWriter.cs:70` (`IsSafeValue`: rejects `< > ; { } @ \` NUL`, control chars, comment delimiters, unbalanced quotes/parens, depth > 8), allow-listed CSS functions only `:55` (`url`, `image`, `image-set`, `element`, `expression` deliberately absent), per-token fallback instead of a broken block `:203` (`Append`).
- Behavior verified by unit tests — nonce copied `tests/Sufficit.Blazor.UI.Tests/ThemeProviderTests.cs:105`–`:112`; absent nonce leaves no attribute `:116`–`:122`.

## Style attributes (`style-src-attr`)

A policy with `style-src-attr 'none'` (or `style-src` without `'unsafe-inline'`) makes the browser ignore every `style` attribute — parser-inserted or set later through `setAttribute` by Blazor — while CSSOM writes (`element.style.setProperty`) remain allowed.

- No component renders a style attribute with its own values; the contract test `Components_DoNotShipStaticInlineStyles` (`tests/Sufficit.Blazor.UI.Tests/DesignTokenContractTests.cs`) allows only the consumer pass-through `@Style` and `SUITable.RowStyleFunc`.
- Bounded values are classes; free-form values travel in `data-sui-style` (`src/Utilities/SUIStyleBuilder.cs`: `--sui-*` names only, character/function allow-list) and are applied through CSSOM by the package JS initializer `src/wwwroot/Sufficit.Blazor.UI.lib.module.js` (re-validates names and values, so injected markup cannot style arbitrary properties).
- Verified in Chromium, Firefox and WebKit by `tests/Sufficit.Blazor.UI.BrowserTests/StrictCspBrowserTests.cs` (catalog served with the policy via `?csp=strict`; geometry measured; zero `securitypolicyviolation` events; positive control proves the policy is active).
- Consumer `Style`/`RowStyleFunc` values are ignored under such a policy — documented in `docs/components/README.md#csp-estrita`.

## Gaps

None at the library boundary. What remains is host configuration and is explicitly documented as such: without the nonce parameter, a strict `style-src` policy blocks the tokens and components render with fallback palette (stated in `SUIThemeProvider.razor:23`–`:29`; host headers not present in this repository — inferred from configuration, not tested).

Searches performed for claimed absences: `Content-Security-Policy` / `UseCSP` header configuration in `src/`, `samples/`, `.github/` (none — the library ships no policy, correctly).

## Intentional divergences

- Tokens are published via an inline `<style>` (nonce-supported) rather than a `<link>` to a generated resource: the palette can come from DI per application (and per tenant), which a static file cannot represent.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |
| 2026-10-28T16:30Z | (this change) | Style attributes: classes + `data-sui-style` via CSSOM, strict-CSP browser test |

## References

- W3C CSP Level 3 — <https://www.w3.org/TR/CSP3/>
- MDN: CSP nonces — <https://developer.mozilla.org/docs/Web/HTTP/Headers/Content-Security-Policy/Sources#nonce-source>
- Related: [whatwg-clipboard-apis.md](whatwg-clipboard-apis.md)
