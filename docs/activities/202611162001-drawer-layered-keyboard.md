# Drawer keyboard and layered overlay lifecycle

Follow-up shared navigation work for the ongoing application-wide migration in
sufficit-blazor/docs/PLAN-MUDBLAZOR-REMOVAL.md. This delivery does not close that plan.

Two Chromium regression tests failed against the old drawer module: Tab omitted
editable fields from its loop, and Escape on a nested modal closed its owner too.
The corrected drawer includes visible, enabled inputs/selects/textareas and honors
nested modal/popover keyboard ownership. Disconnected/inert return targets are not
focused. An executable example exercises the temporary drawer, editable fields,
nested confirmation and interactive search popup.

Validation: all three focused Chromium cases passed (both original failures plus
popover dismissal/focus restoration); 1,228 library unit tests passed; complete
Release solution builds with warnings as errors; 84 copied standalone examples
compile; generated catalog and CSS gates pass. Existing CSS budget unchanged.
Final local package Sufficit.Blazor.UI.2.26.1116.2259 passes executable root/pathbase
consumer validation. No NuGet release or production deployment is claimed.
