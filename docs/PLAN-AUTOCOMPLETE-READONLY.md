# Native autocomplete read-only contract

1. Completed — confirm the portability consumer needs focusable, copyable read-only selection. Two compiling behavior regressions fail on missing native readonly and an uncanceled search.
2. Completed — implement a typed ReadOnly contract, reject input/clear/selection, cancel searches and restore selected text on transition; update example/catalog/API baseline.
3. In progress — validate full unit suite, standalone examples, Release solution, package execution and consumer browser; record actual evidence and deliver main.

Reference: existing SUIAutocomplete cancellation/value contracts and ContextAutocomplete's current ReadOnly parameter. Disabled and read-only remain distinct; no mutation or search is triggered by observation.

Both behavior regressions now pass, together with the intentionally reviewed additive public API baseline.

Validation: 1,233 library tests passed; 84 standalone examples compile; Release
solution warnaserror and package root/path-base execution pass. Consumer browser
passes at 390/1280 in both themes: keyboard selection, required native-form and
imperative validation, search-error/empty distinction, pending-search cancellation
when switching read-only, preserved selected text and no clear/edit actions.
No complete library browser-suite rerun or NuGet publication is claimed.
