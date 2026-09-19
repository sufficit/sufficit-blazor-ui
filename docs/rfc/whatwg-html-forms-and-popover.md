# WHATWG HTML — form participation and the Popover API

**Status:** 🟡 partial — native form controls and the Popover API are used correctly with graceful fallbacks, but form participation (name/value submission) is only first-class on three of the form components.
**Last reviewed:** 2026-09-19T16:56Z · commit `9cbc7ea`
**Project role:** rendering library — emits HTML form controls and consumes browser APIs; never handles form data server-side (hosts do).

## What the standard requires

Only the requirements that touch this project:

- Form submission: controls participate in submission through `name`/`value` pairs on form-associated elements (HTML §4.10 "Forms"); `button type` semantics (`submit`/`reset`/`button`).
- Popover attribute: `popover="manual"` promotes an element to the top layer; `showPopover()`/`hidePopover()` and the `:popover-open` selector (HTML §6.13 "The popover attribute").
- Progressive enhancement: features degrade when unsupported.

## Where the project complies

- Native `button` with explicit type — `src/Components/Actions/SUIButton.razor:30` (`type="@SUIClassBuilder.Slug(EffectiveButtonType)"`), default `SUIButtonType.Button` at `:166`; icon button native `<button>` `src/Components/Actions/SUIIconButton.razor:26`, `:34`.
- Native form controls that submit:
  - checkbox — `src/Components/Forms/SUICheckbox.razor:6`–`:13` (`id`, `name="@Name"`, `type=checkbox`, `value="@FormValue"`, `checked`), `Name` parameter at `:65`;
  - choice card — `src/Components/Forms/SUIChoiceCard.razor:12` (`name="@Name"`), `Name` parameter at `:101`;
  - date field — hidden proxy input `src/Components/Forms/SUIDateField.razor:38` (`<input type="hidden" name="@Name" value="@FormValue" disabled="@Disabled">`), `Name` parameter `src/Components/Forms/SUIDateField.razor.cs:75`, `FormValue` ISO at `:147`.
- Popover API, top layer:
  - `popover="manual"` on the Select menu `src/Components/Forms/SUISelect.razor:55`, the Autocomplete list `src/Components/Forms/SUIAutocomplete.razor:63`, the DateField calendar `src/Components/Forms/SUIDateField.razor:49`;
  - feature detection before use — `src/Components/Forms/SUISelect.razor.js:96` (`typeof menu.showPopover !== 'function'` → early return, menu still opens as an inline positioned element via its `--open` class), `showPopover()` wrapped in `try/catch` with a warning `:101`–`:107`, `hidePopover()` guarded `:147`–`:148`;
  - `:popover-open` probe guarded by `try/catch` `src/Components/Forms/SUISelect.razor.js:6`–`:10` (same guard in `src/Components/Forms/SUIDateField.razor.js:6`–`:10`);
  - viewport-aware placement while in the top layer `src/Components/Forms/SUISelect.razor.js:12`–`:44` (and `src/Components/Forms/SUIDateField.razor.js:29`–`:58`), RTL-aware `:30`–`:35`.
- Sample host pipeline honors standard middleware order for SSR forms — `samples/Sufficit.Blazor.UI.Catalog/Program.cs:28`–`:33` (`UseRouting` → `UseStatusCodePagesWithReExecute` → `UseAntiforgery` → `MapStaticAssets`/`MapRazorComponents`).

## Gaps

| Priority | Gap | Concrete impact | Recommendation |
|---|---|---|---|
| 🔵 | `SUITextField` (`src/Components/Forms/SUITextField.razor:31`–`:44`) and `SUINumericField` (`src/Components/Forms/SUINumericField.razor:14`–`:27`) have no `Name` parameter; `name` reaches the input only via unmatched-attribute forwarding (`SUITextField.razor:153`, `SUINumericField.razor:162`) | Inconsistent authoring: three components take `Name` first-class, two others need `name="…" @attributes` knowledge; plain-HTML posts from those fields depend on the consumer remembering the escape hatch | Add a `Name` parameter mirroring `SUIDateField`/`SUICheckbox`/`SUIChoiceCard`, or document the forwarding pattern for every form component |
| 🔵 | `SUISelect` and `SUIAutocomplete` render no proxy input, so their selected value is invisible to plain HTML form posts | Server-side form binding (`Request.Form`) cannot see the selection without JS/Blazor interactivity | Follow the `SUIDateField` hidden-input pattern when `Name` is set |

Searches performed for claimed absences: `type="hidden"` in `*.razor` under `src/Components` (only `SUIDateField.razor:38`); `public string? Name` in `src/Components/Forms` (only `SUIDateField.razor.cs:75`, `SUIChoiceCard.razor:101`, `SUICheckbox.razor:65`).

## Intentional divergences

- Modality is implemented manually, not with `<dialog>`/`showModal()`: `SUIDialogHost` renders `div role=dialog` with its own focus trap (`src/Components/Overlays/SUIDialogHost.razor.js:72`–`:114`) so dialog content stays in Blazor's render tree and the service API can queue dialogs; the browser dialog element would fight the component lifecycle.
- When the Popover API is missing, menus fall back to inline (non-top-layer) rendering with the same class-based open state (see `SUISelect.razor.js:96`–`:107` and the `--open` classes applied from C#, `src/Components/Forms/SUIDateField.razor.cs:145`–`:146`) — deliberate progressive enhancement, not a bug.
- `SUICopyToClipboard`'s wrapper span intentionally forwards click only; submission semantics are the child's (see the accessibility document).

## Revision history

| Reviewed (UTC) | Commit | Summary of changes |
|---|---|---|
| 2026-09-19T16:56Z | `9cbc7ea` | Initial analysis (regenerate mode) |

## References

- WHATWG HTML Standard — <https://html.spec.whatwg.org/multipage/>
- The popover attribute — <https://html.spec.whatwg.org/multipage/popover.html>
- Related: [w3c-wcag22-wai-aria-component-accessibility.md](w3c-wcag22-wai-aria-component-accessibility.md), [rfc-3339-date-field-form-value.md](rfc-3339-date-field-form-value.md)
