# Native multiple-file selection

Required by the authorized music-on-hold migration in sufficit-blazor. Existing
SUIFileUpload only handles one file; preserve the consumer's multiple selection
without reintroducing Mud or copying a file input into the application.

1. Completed — define/prove native multiple selection while preserving existing
single Value/ValueChanged behavior, disabled guards, size rejection, explicit clear
and the rule that selection never opens streams or uploads.
2. Completed — implement Multiple, Files/FilesChanged and MaxFiles/count rejection,
replace each native selection rather than retaining stale browser-file references,
update example/catalog/API evidence and meaningful component/browser regressions.
3. Completed — validate unit/standalone examples/Release/package and the music-on-hold
consumer; deliver main and archive only after this scoped contract is proven.

Reference lock: existing SUIFileUpload and IBrowserFile ownership. The caller owns
buffering, MIME/container inspection, pending queues and all operational writes.
Multiple-file size validation should preserve accepted files and report rejection;
count overflow rejects the selection explicitly. No implicit append or file reads.

Four compiling tests fail on the old single-file handler after declaration of the
additive parameters. Native multiple selection now reports accepted files without
opening streams, replaces prior selection, rejects excessive count, preserves
allowed files when another is oversized, clears the native element explicitly and
ignores events while disabled. All eight single/multiple/media cases pass.

Reviewed public API adds only Multiple, Files, FilesChanged, MaxFiles and
TooManyFilesText; existing single-file members remain. All 1,238 unit cases and
the warnaserror solution pass. Both focused Chromium cases pass at 390/1280,
covering native selection, explicit clear, rejected count and a subsequent valid
selection. All 84 standalone examples compile. Executable local package 2.26.1117.504
passes at root and /app. The actual consumer browser passes at 390/1280 in light
and dark: keyboard native file chooser, count/container rejection, explicit upload,
partial failure/retry without replay, exact class ID and file/class lifecycle.
Screenshots inspected. App suite: 1,999 passed, 23 opt-in integrations skipped.
No full browser matrix, tagged NuGet release or production upload is claimed.

Scoped contract complete; implementation and this activity are delivered together
to main. The application's broader Mud removal remains active.
