# Shared SUI context menu

The library now offers a native top-layer context menu with viewport-point
and focusable-element anchors. The consumer supplies semantic actions and
owns their callbacks. Browser dismissal and Escape restore focus when
appropriate. Arrow keys and Home/End navigate enabled actions. Stable menu
IDs allow cleanup even after the component is removed.

The existing SUIMenu remains available for its button-triggered actions.
The executable catalog example covers mouse and keyboard entry. The
reviewed public API and asset budgets reflect the new component.

The component suite passed 1,280 tests. The context-menu and existing menu
lifecycle browser tests passed. All 86 catalog examples compiled, and the
prerelease package passed validation with root and path-base consumers.

WebKit can dismiss an auto popover on the right-button release that follows
its opening event. The context menu uses a manual popover and dismisses on
the next outside pointer gesture. Genius browser fixtures validated the
result with mouse, keyboard and touch in Chromium and WebKit. The consumer
uses the corrected context menu.

The release pipeline exposed an existing WebKit catalog test that could act
on prerendered markup. The field-actions fixture now uses the same
interactivity marker as the catalog, and the test waits for the live
circuit before exercising its original assertions. The local WebKit run
passed both widths with their original layout and interaction checks.
