# Native stat value content

Objective: preserve rich formatted values while migrating shared consumer stat cards to native SUIStat; no widget dependency or raw app-specific card recreation.

1. Completed — add native ValueContent with the same loading/value/label semantics; update executable example and focused behavior tests.
2. Completed — validate library, unit suite, standalone catalog examples and package gates.
3. In progress — record evidence, convert this completed plan to an activity, commit/push main. No NuGet publication is claimed from a main push.

Reference: existing SUIStat/SUI card tokens and consumer SUXStatCard.ValueContent. Preserve plain text and Unit fallback. Loading must hide rich content as it hides the textual value. Explicit rich content has no numeric tween to infer.

Validation: 1,230 unit tests passed; 84 standalone examples compile; full solution
Release warnaserror and package root/path-base minimal consumers passed. Rich values
skip numeric interpolation of the hidden fallback text. No dependency on MudBlazor.
No complete browser-suite rerun is claimed for this additive slot; the consuming
shared-card fixture will exercise it in desktop/mobile and both themes.
