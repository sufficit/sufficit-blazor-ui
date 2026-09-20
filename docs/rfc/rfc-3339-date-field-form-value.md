# RFC 3339 — date representation in SUIDateField

**Status:** ✅ compliant for the date-only subset the library uses — machine-readable dates are always `yyyy-MM-dd` in the invariant culture; human-readable formats are deliberately culture-aware.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`

## What the standard requires

Only the requirements that touch this project:

- Date/time strings exchanged between systems use `YYYY-MM-DD` (plus time/offset where applicable) (§5.6, `date-fullyear "-" date-month "-" date-mday`).

## Where the project complies

- Machine value, invariant, unambiguous — `src/Components/Forms/SUIDateField.Calendar.cs:48` (`IsoDate(date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)`).
- Form post value uses the same ISO form — `src/Components/Forms/SUIDateField.razor.cs:147` (`FormValue => Value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)`), rendered into the hidden proxy input `src/Components/Forms/SUIDateField.razor:38`.
- Calendar cells carry the ISO date as data — `data-sui-date="@IsoDate(date)"` `src/Components/Forms/SUIDateField.razor:82`, and focus is restored through it (`focusDate` with the ISO string, `src/Components/Forms/SUIDateField.razor.cs:218`).
- Display formatting is culture-first (`"d"`/`"D"` with `EffectiveCulture`) `src/Components/Forms/SUIDateField.Calendar.cs:46`–`:47` — correct separation of machine format (ISO) vs human format (locale).

## Gaps

None for the subset used. The library intentionally handles dates only (no time component anywhere).

Searches performed for claimed absences: `TimeOnly`, `toISOString`, `DateTimeOffset`, `yyyy-` in `*.cs`, `*.js`, `*.razor` under `src/` — the only `yyyy-` occurrences are the ISO formatters listed above; no time-of-day serialization exists.

## Intentional divergences

- `aria-label` on day cells uses the long culture format (`FormatLongDate`, `SUIDateField.Calendar.cs:47`), not ISO: screen-reader users should hear their locale's date, while the machine value stays ISO.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- RFC 3339 — <https://www.rfc-editor.org/rfc/rfc3339>
- Related: [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md)
