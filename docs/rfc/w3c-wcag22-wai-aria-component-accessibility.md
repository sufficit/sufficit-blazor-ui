# W3C WCAG 2.2 (A/AA) + WAI-ARIA 1.2 — component accessibility

**Status:** 🟡 partial — the library ships a strong ARIA/keyboard implementation and CI-enforced WCAG 2.2 AA sweeps, but one data-table pattern (`tr[role=button]`) is outside the ARIA role taxonomy.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`
**Project role:** component library consumed by Blazor apps (SSR + interactive). Accessibility is the library's contractual surface, enforced in CI.

## What the standard requires

Only the requirements that touch a UI component library:

- WCAG 2.2 A/AA success criteria enforced by automated audit: `wcag2a`, `wcag2aa`, `wcag21aa`, `wcag22aa` (axe tags).
- 2.1.1/2.1.2 keyboard operability and no keyboard traps.
- 2.4.7 focus visible; 1.4.10 reflow at 320px and 200% zoom; 1.4.3/1.4.11 contrast (light/dark); 1.4.12 text spacing; 2.5.8 target size; 1.3.1 info & relationships (correct roles); 4.1.2 name/role/value; 2.3.3 animation from interactions (prefers-reduced-motion); 1.4.12 heading order (2.4.6/1.3.1 family).
- ARIA 1.2 patterns the library's widgets emulate: combobox (with listbox popup), dialog (modal and non-modal), tabs, tooltip, alert/status live regions, progressbar, grid.

## Where the project complies

- Automated WCAG 2.2 AA sweep — `tests/Sufficit.Blazor.UI.BrowserTests/AccessibilityBrowserTests.cs:20` (tags `wcag2a…wcag22aa`), `:199` (axe `RunOnly` by tag); matrix: mobile light/dark 375×812 and desktop dark 1440×900 (`:29`–`:31`), plus reduced-motion + dark color scheme (`:46`).
- Focus visibility beyond axe — `AccessibilityBrowserTests.cs:60` (`KeyboardFocus_IsAlwaysVisibleOnInteractiveElements`); text spacing `:116`–`:127`; heading order `:141`–`:149`.
- Reflow/zoom/RTL/targets — 200% zoom without horizontal overflow `tests/Sufficit.Blazor.UI.BrowserTests/CatalogBrowserTests.cs:646`–`:647`; RTL at 320px `:651`–`:655`; 44px touch targets `:682`–`:689`; forced-colors emulation asserted `:743`.
- Combobox pattern (Select) — `src/Components/Forms/SUISelect.razor:21`–`:29` (`role=combobox`, `aria-haspopup=listbox`, `aria-expanded`, `aria-controls`, `aria-activedescendant`), listbox/option `:52`–`:63`; keyboard `ArrowUp/Down/Home/End/Enter/Space/Escape` `src/Components/Forms/SUISelect.razor.js:69`, listener `:73`.
- Combobox pattern (Autocomplete) — `src/Components/Forms/SUIAutocomplete.razor:16`–`:25` (`role=combobox`, `aria-autocomplete=list`, invalid/errormessage), status live region `:61`, listbox/option/presentation `:65`–`:98`; keys `src/Components/Forms/SUIAutocomplete.razor.js:9`–`:10`.
- Dialog (non-modal calendar) — `src/Components/Forms/SUIDateField.razor:13`–`:32` (trigger `button type=button`, `aria-haspopup=dialog`, `aria-expanded`, `aria-controls`), `:41`–`:51` (`role=dialog`, `aria-modal=false`, `lang`/`dir` from effective culture), month announced `:57`, `role=grid`/`row`/`columnheader`/`gridcell`, `aria-selected`, `aria-current=date`, ISO `data-sui-date` `:65`–`:85`; keyboard: open on Enter/Space/ArrowDown, Escape, arrows, Home/End, PageUp/PageDown `src/Components/Forms/SUIDateField.razor.cs:246`–`:303`.
- Dialog (modal) + focus trap — `src/Components/Overlays/SUIDialogHost.razor:14`–`:17` (`role=dialog`, `aria-modal=true`, `aria-labelledby`, `tabindex=-1`); focus trap with Tab wrap and Escape `src/Components/Overlays/SUIDialogHost.razor.js:26` (focusableElements), `:72` (Escape), `:79`–`:98` (Tab cycling), `:102` (capture listener), initial focus `:113`–`:114`; browser-verified trap `CatalogBrowserTests.cs:530` (`Dialog_TrapsForwardAndBackwardTab`) and Escape `:357`.
- Tabs — `src/Components/Navigation/SUITabs.razor:8` (tablist), `:17`–`:22` (tab, `aria-selected`, `aria-controls`, roving `tabindex` 0/−1), `:32`–`:36` (tabpanel `tabindex=0`); arrow/Home/End keys `src/Components/Navigation/SUITabs.razor.js:7`.
- Tooltip pattern — portal element `role=tooltip` `src/Components/Overlays/SUITooltip.razor.js:37`, `aria-hidden` toggling `:38`/`:227`, `aria-describedby` add/remove on the anchor `:149`–`:165`, shown on `focusin` `:270`, content set via `textContent` (no `innerHTML`) `:226`.
- Live regions — assertive toast `src/Components/Feedback/SUIToast.razor:4` (`role=alert aria-live=assertive`), alert `src/Components/Feedback/SUIAlert.razor:3`, status `src/Components/Feedback/SUIStatusBanner.razor:7`, snackbar `src/Components/Feedback/SUISnackbarHost.razor:9`, polite pending-changes `src/Components/Feedback/SUIPendingChangesBar.razor:13`.
- Progressbars — `src/Components/Feedback/SUIProgressLinear.razor:4`, `src/Components/Feedback/SUIProgressCircular.razor:8` (`role=progressbar` + valuemin/valuenow-valuemax family).
- Reduced motion — `src/styles/sui-portals.css:108`, `src/styles/sui-shared-skeleton.css:23`, `src/styles/sui-shared-navigation-links-groups-collapse-and-desktop-rail-flyouts-3.css:24`, `src/styles/sui-checkbox.css:85` (all `@media (prefers-reduced-motion: reduce)`).

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| 🔶 | Interactive table rows are `<tr role="button" tabindex="0">` (`src/Components/DataDisplay/SUITable.razor:38`–`:41`; keyboard handled in `SUITable.razor.cs:140`) | `role=button` on a `tr` is not an ARIA-allowed role/element combination; screen readers may announce the row inconsistently, and 4.1.2 (name/role/value) relies on non-standard mapping | Render the action as a real button (e.g. first cell) or adopt the `role=grid` + row-activation pattern used by SUIDateField; keep `aria-label` for the action |
| 🔵 | `aria-sort` never emitted by the library; consumers must set it on the `<th>` themselves (`src/Components/DataDisplay/SUITableSortLabel.razor:6`) | Sortable tables announce state only when the consumer remembers the comment | Offer an optional `SortLabel`→`SUITh` integration (or a `SUISortHeader` that renders the `th` with `aria-sort`), so the default path is compliant |
| 🔵 | Icon-only path depends on consumer-provided `Title`/`AriaLabel` (`src/Components/Actions/SUIIconButton.razor:26`, `:34`) | Missing parameter yields an unnamed button (axe catches it in samples, not in consumer apps) | Make the accessible name `[EditorRequired]`-style enforced, or emit a default label from the icon name |

Searches performed for claimed absences: `aria-sort` in `*.razor`, `*.cs` under `src/`; `EditorRequired` on icon-button name params under `src/Components/Actions/`.

## Intentional divergences

- `SUICopyToClipboard` with `ChildContent` renders a click-only `<span>` (no role/tabindex) — keyboard access is deliberately the child's job, documented in `src/Components/Actions/SUICopyToClipboard.razor:10`–`:12`. The default path (own icon button, accessible name at `:88`–`:90`) is the compliant one.

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- WCAG 2.2 — <https://www.w3.org/TR/WCAG22/>
- WAI-ARIA 1.2 — <https://www.w3.org/TR/wai-aria-1.2/>
- ARIA Authoring Practices Guide — <https://www.w3.org/WAI/ARIA/apg/>
- Related: [whatwg-html-forms-and-popover.md](whatwg-html-forms-and-popover.md)
