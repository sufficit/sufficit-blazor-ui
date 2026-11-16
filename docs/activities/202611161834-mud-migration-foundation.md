# SUI contracts for the application migration

This activity records the validated library foundation for the ongoing application-wide
MudBlazor removal tracked in sufficit-blazor/docs/PLAN-MUDBLAZOR-REMOVAL.md.
It does not close that application plan or claim all consumers are migrated.

- Added SUIForm/SUIValidatedField: annotations, typed rules, cancellation, per-form state,
  context replacement, cleanup and duplicate-submit protection.
- Added SUIDataTable: immutable query/page contracts, asynchronous authorized owner loader,
  stale-response rejection, scope clearing, retry/error states and controlled pagination.
- Added SUIPopup: interactive nonmodal native top layer, viewport positioning using shared
  geometry, light dismissal, Escape in search inputs, focus return and lifecycle cleanup.
- Added autonomous navigation/communication icon constants. WhatsApp SVG is from Simple
  Icons under CC0; THIRD-PARTY-NOTICES.md is included in the package. No Mud runtime dependency.
- Snackbar feedback moves to the top while a pending-changes bar is visible, keeping its
  Save/Cancel controls accessible. Other snackbar placement remains at the bottom.

Validation: Release solution builds with warnings as errors; 1,215 component tests passed.
All 84 copied standalone catalog examples compile. Package contents and minimal net10.0
consumer applications validate at root and a path base. Chromium complete suite reached
129 passing cases and exposed a snackbar/footer collision; its seven affected interaction
and accessibility cases passed after correction. Updated popup search-input keyboard cases
also passed. Performance uses the measured asset ceilings: 15,604 B Brotli across optional
JS modules (16,128 B ceiling); global CSS 62,503 B raw / 11,382 B gzip / 9,928 B Brotli.
No NuGet release tag or production deployment is implied by local validation.
