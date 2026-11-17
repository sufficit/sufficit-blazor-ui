# Native textarea restoration and nested-table ownership

Required by sufficit-blazor's active Mud removal/TTS browser checkpoint.

1. Completed — preserve the demonstrated failures in reusable examples/browser
   regressions: a user-edited multiline field keeps its dirty DOM value after a
   parent restores a new Value; a nested unstacked table inherits mobile grid cells.
2. Completed — fix the textarea controlled-value rendering and restrict mobile table
   rules to the table's own direct rows/cells; preserve initial SSR text, bindings,
   native readonly/disabled, standalone nested-table styling and existing tokens.
3. In progress — validate unit/example/Release gates, focused browser and the real TTS
   consumer, package execution, then reviewed main commit/push and activity conversion.

Reference lock: existing SUITextField.Value/ValueChanged and SUITable.StackOnMobile
contracts. No new public API, theme, CSS workaround or dependency is required.

Observed consumer before fix at 390px: textarea value = Texto digitado while
textContent/defaultValue = Texto do histórico. A nested details cell computes
`display:grid`, width 128.84px and height 755.69px instead of table-cell. The full
consumer recovery check times out; its failing screenshot is retained under /tmp.

Implemented controlled textarea value using its existing formatter, retaining SSR
child text. Mobile selectors now target only the owner table's direct header,
body, rows and cells; SUITd value rules follow the same ownership. Examples show
restore/clear and a nested table, with four focused Chromium cases passing at
390/1280. Library unit suite: 1,233 passed; 84 standalone examples compile; Release
solution warnaserror and generated CSS/catalog checks pass. Package execution and
real TTS consumer verification remain pending; no publication is claimed.

Package 2.26.1117.408 builds and passes the executable consumer checks at root
and /app. The real TTS consumer now passes 390/1280 light/dark, including edited
textarea restoration, expanded details, generation/mix/history and simulated
telephony publication. No full library browser-matrix rerun or NuGet publication
is claimed. Source delivery and final activity conversion are the remaining part
of checkpoint 3.
