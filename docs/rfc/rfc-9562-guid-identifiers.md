# RFC 9562 — UUIDs as identifiers

**Status:** ✅ compliant — UUIDs are generated for uniqueness only (never parsed from untrusted wire input except one conversion path), formatted canonically, and never serialized cross-system by this library.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`

## What the standard requires

Only the requirements that touch this project:

- UUIDs must be generated with sufficient unpredictability when used as unguessable identifiers (§5.4 / random-UUID method, RFC 9562 §6.2 — obsoletes RFC 4122).
- Canonical text representation: 8-4-4-4-12 hex, lowercase (§4.3), or the equivalent compact form when the consumer format demands it.

## Where the project complies

- Generation via the platform RNG — `src/Services/SUISnackbarService.cs:20` (snackbar entry id), `src/Services/ISuiDialogService.cs:35` (dialog request id default), `src/Components/Navigation/SUINavigationContext.cs:11` (`MenuId`). `Guid.NewGuid()` is backed by the OS CSPRNG on .NET (inferred from runtime behavior, not tested in this review — no cryptographic claim is made beyond uniqueness).
- Compact lowercase hex for DOM ids — `sui-select-{Guid.NewGuid():N}` `src/Components/Forms/SUISelect.razor.cs:87`, `sui-autocomplete-…` `src/Components/Forms/SUIAutocomplete.razor.cs:125`, `sui-date-field-…` `src/Components/Forms/SUIDateField.razor.cs:93`. The `"N"` format is the canonical hex without hyphens; the ids stay inside one document, where the standard's hyphenated form is not required.
- Parsing with graceful failure — `Guid.Parse(raw.ToString()!)` for `Guid`-typed select values `src/Components/Forms/SUISelect.razor.cs:379`, with `FormatException` caught at `:384`–`:394` returning `false` instead of throwing to the caller.

## Gaps

None. Identifiers never cross a process boundary in a context where byte-order (§4) or version bits matter: they key dictionaries, DOM ids and service queues inside one circuit.

Searches performed for claimed absences: `Guid.Parse`, `Guid.TryParse`, `uuid` in `*.cs`, `*.js` under `src/` (single parse site, listed above); no UUID bytes/endianness handling exists.

## Intentional divergences

- DOM ids use the `N` (hyphen-less) format rather than the canonical 8-4-4-4-12 string: shorter ids inside `aria-controls`/`aria-activedescendant` references, and HTML id syntax has no opinion on hyphens.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- RFC 9562 — <https://www.rfc-editor.org/rfc/rfc9562>
- Related: [rfc-3339-date-field-form-value.md](rfc-3339-date-field-form-value.md)
