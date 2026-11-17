# Native call-result contracts

Completed dependency scope for the authorized sufficit-blazor Mud migration.
All four checkpoints are complete: native labelled scrolling, disabled anchor
behavior, readable bounded filter chips, and gates/package/consumer validation.
The application-wide migration remains active.

SUITable adds opt-in ScrollRegion to expose its existing overflow wrapper as a
labelled keyboard-focusable region without forcing virtualization. Existing
ScrollLabel/default/interactive/virtualized behavior remains. The existing focus
ring also covers nonvirtualized focusable wrappers. A compiling assertion failed
with the former virtual-only computation, then passed after opt-in was honored.

SUIButton/SUIIconButton disabled/loading anchors now omit href, leave sequential
focus and retain explicit link/ARIA semantics; enabled targets are restored.
The icon family also prevents disabled default navigation without requiring an
OnClick callback. Two compiling old-behavior cases failed because href remained;
four disabled/loading cases pass. Chromium caught bool aria-disabled serializing
as an empty attribute, unlike bUnit; explicit true/false strings fix the browser
semantics. Keyboard activation/Tab skipping/enabled navigation are verified.

SUIChip adds opt-in Wrap: complete operands fit their parent, grow with text,
and retain existing sizes, edit/remove callbacks and nonwrapped defaults. Wrapped
actionable/removable chips reserve the existing 3px remove hit halo inside their
width. The calls consumer reproduced period-chip overflow (395px in a 342px grid
track) before the fix. Its owner now constrains the single grid track and opts
owned chips into Wrap; no calls-specific chip DOM/CSS override is used.

Reviewed public API adds only ScrollRegion and Wrap. No JavaScript or dependency
is added. Final CSS: 63,741 raw / 11,640 gzip / 10,139 Brotli, up 642 / 114 / 94 B.
Only raw crosses the previous ceiling. All three raw gates follow the documented
measured×1.03/256B rule (65,792); compressed ceilings remain 11,776 / 10,240.
The first redirected CSS build error was missed, so one consumer round served
old CSS and is excluded. Final generation/byte equality and budgets pass.

Final validation: warnaserror solution with zero warnings/errors; 1,246 unit tests;
84 standalone copied examples; catalog/CSS checks and 15 script tests. Six focused
Chromium cases pass at 390/1280 for scrolling, disabled links and wrapped labels/
independent keyboard actions. The complete browser project passes 148 / 1 optional
configured-catalog-path-base case skipped / 149 total. The initial broader run had
65 refused-connection errors because its separate catalog host was not configured;
that run is excluded. Fresh matching catalog/showcase hosts produced final results.

Fresh executable local package 2.26.1117.650 passes both root and /app. Earlier
local 550 proof predates Wrap and is not substituted via NuGet cache. The actual
consumer source/server/client/fixture builds pass against reviewed sibling bodies;
its suite passes 2,022 / 23 opt-in integrations skipped / 2,045 total. Native calls
browser passes 390/1280 light/dark: actual filter/URL updates, full long operands,
answerer order, keyboard scroll, noopener trace target, bounded recording callback,
loading/error/empty/refresh. Final mobile/light and desktop/dark inspected. Provider
queries/recording operations are not exercised by that local renderer fixture.

Source and activity are delivered together on main before the dependent consumer.
No tagged/NuGet release or production deployment is claimed. Consumer activity:
[native call results](../../../sufficit-blazor/docs/activities/202611170436-native-call-results.md).
