using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
///     Single-line or multiline text input bound to <typeparamref name="TValue"/>.
///     Numeric input types (<c>number</c>, <c>range</c>) are parsed and rendered
///     with the invariant culture, because the browser only speaks the
///     dot-decimal form: rendering 0.40m as "0,40" makes it drop the value, and
///     parsing "0.40" as pt-BR turns forty cents into forty reais.
/// </summary>
public partial class SUITextField<TValue>
{
    [CascadingParameter] private Microsoft.AspNetCore.Components.Forms.EditContext? FormContext { get; set; }
    private readonly SUIFieldBinding<TValue?> _field = new();
    private string? EffectiveErrorText => ErrorText ?? _field.Error;

    /// <summary>Current value, or the type default when empty.</summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>Raised when the user commits a value; enables <c>@bind-Value</c>.</summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>Field expression used to bind validation messages from the surrounding <c>EditContext</c>.</summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>Visible label, rendered above the control and bound to it for assistive technology.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Hint shown inside the empty control.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Static helper text rendered below the control.</summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>Appends a secondary <see cref="OptionalText"/> marker to the label instead of the caller editing the label text.</summary>
    [Parameter] public bool Optional { get; set; }

    /// <summary>Marker shown when <see cref="Optional"/> is set. Default "(opcional)".</summary>
    [Parameter] public string OptionalText { get; set; } = SUIFieldParts.DefaultOptionalText;

    /// <summary>Markup label (e.g. with inline <c>&lt;code&gt;</c>); takes precedence over <see cref="Label"/>.</summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>Markup helper content; takes precedence over <see cref="HelperText"/> and uses the same slot.</summary>
    [Parameter]
    public RenderFragment? HelperContent { get; set; }

    /// <summary>
    /// Non-blocking warning shown below the control in the warning tone and
    /// announced through <c>aria-describedby</c>. Unlike <see cref="ErrorText"/>
    /// it never sets <c>aria-invalid</c>.
    /// </summary>
    [Parameter]
    public string? WarningText { get; set; }

    /// <summary>
    /// Renders the value in the monospace font token (<c>--sui-font-mono</c>)
    /// with spellcheck off — for ids, tokens, URIs and JSON (with <see cref="Multiline"/>).
    /// </summary>
    [Parameter]
    public bool Monospace { get; set; }

    /// <summary>
    /// Values offered as native suggestions (an HTML <c>datalist</c>) while the
    /// user remains free to type any other text. Ignored when <see cref="Multiline"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Suggestions { get; set; }

    /// <summary>Explicit validation message; overrides any message from the <c>EditContext</c>.</summary>
    [Parameter]
    public string? ErrorText { get; set; }

    /// <summary>Marks the field as invalid regardless of validation state.</summary>
    [Parameter]
    public bool Invalid { get; set; }

    /// <summary>Explicit DOM id; one is generated when omitted.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>Extra element ids announced alongside this field's own helper and error texts.</summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>Form field name, so the value takes part in a plain HTML form post.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>HTML input type. Default "text".</summary>
    [Parameter]
    public string InputType { get; set; } = "text";

    /// <summary>Renders a resizable textarea while preserving the same field contract.</summary>
    [Parameter]
    public bool Multiline { get; set; }

    /// <summary>Initial number of visible rows when <see cref="Multiline"/> is enabled.</summary>
    [Parameter]
    public int Rows { get; set; } = 4;

    /// <summary>Maximum number of characters accepted by the control.</summary>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>Updates the bound value on every keystroke instead of on change.</summary>
    [Parameter]
    public bool Immediate { get; set; }

    /// <summary>Disables the control.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Optional SUI icon rendered inside the input at the inline end.</summary>
    [Parameter]
    public string? AdornmentIcon { get; set; }

    /// <summary>Shows an accessible clear action while the field contains a value.</summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>Accessible label and tooltip used by the clear action.</summary>
    [Parameter]
    public string ClearText { get; set; } = "Limpar campo";

    /// <summary>
    ///     Adds a show/hide toggle to a <c>password</c> input. Opt-in because the
    ///     toggle needs an interactive render mode: on a statically rendered
    ///     form the button would do nothing.
    /// </summary>
    [Parameter]
    public bool Revealable { get; set; }

    /// <summary>Accessible label and tooltip of the reveal toggle while the value is hidden.</summary>
    [Parameter]
    public string RevealText { get; set; } = "Mostrar valor";

    /// <summary>Accessible label and tooltip of the reveal toggle while the value is shown.</summary>
    [Parameter]
    public string HideText { get; set; } = "Ocultar valor";


    /// <summary>Legacy alias for <see cref="SUIComponentBase.AdditionalAttributes"/>; splatted onto the input or textarea.</summary>
    [Parameter, Obsolete("Use AdditionalAttributes instead; this alias will be removed in the next breaking release.")]
    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    private ElementReference _inputElement;
    private readonly string _generatedId = $"sui-text-field-{Guid.NewGuid():N}";
    private string EffectiveId => string.IsNullOrWhiteSpace(Id) ? _generatedId : Id;
    private int EffectiveRows => Math.Clamp(Rows, 2, 20);
    private string HelperId => $"{EffectiveId}-helper";
    private string ErrorId => $"{EffectiveId}-error";
    private string WarningId => $"{EffectiveId}-warning";
    private string SuggestionsId => $"{EffectiveId}-suggestions";
    private bool HasSuggestions => Suggestions?.Any() == true;
    private bool HasError => Invalid || !string.IsNullOrWhiteSpace(EffectiveErrorText);
    private bool _revealed;
    private bool CanClear
        => Clearable && !Disabled && !string.IsNullOrEmpty(Value?.ToString());
    private bool CanReveal
        => Revealable && !Multiline && IsPasswordInput;
    private bool IsPasswordInput
        => string.Equals(InputType, "password", StringComparison.OrdinalIgnoreCase);
    private string EffectiveInputType
        => CanReveal && _revealed ? "text" : InputType;
    private int TrailingCount
        => (CanReveal ? 1 : 0) + (CanClear ? 1 : 0) + (string.IsNullOrWhiteSpace(AdornmentIcon) ? 0 : 1);
    private string InputClassname
        => Monospace ? BaseInputClassname + " sui-field__input--mono" : BaseInputClassname;

    private string BaseInputClassname
        => TrailingCount switch
        {
            0 => "sui-field__input",
            1 => "sui-field__input sui-text-field__input--pad-trailing",
            2 => "sui-field__input sui-text-field__input--pad-clear",
            _ => "sui-field__input sui-text-field__input--pad-triple",
        };

    // The adornment sits left of every action button rendered at the end.
    private string AdornmentClassname
        => ((CanClear ? 1 : 0) + (CanReveal ? 1 : 0)) switch
        {
            0 => "sui-text-field__adornment",
            1 => "sui-text-field__adornment sui-text-field__adornment--with-clear",
            _ => "sui-text-field__adornment sui-text-field__adornment--with-actions",
        };

    private void ToggleReveal() => _revealed = !_revealed;

    /// <summary>
    ///     Number and range inputs speak the dot-decimal form only, whatever the
    ///     user culture is: a comma makes the browser drop the value silently.
    /// </summary>
    private bool IsNumericInput
        => string.Equals(InputType, "number", StringComparison.OrdinalIgnoreCase)
            || string.Equals(InputType, "range", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     The value the browser must see: numeric fields are rendered with the
    ///     invariant culture so 0.40m never reaches the DOM as "0,40".
    /// </summary>
    private string? InputValue => IsNumericInput ? FormatNumeric(Value) : Value?.ToString();

    private static string? FormatNumeric(TValue? value)
        => value switch
        {
            null => null,
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString("R", CultureInfo.InvariantCulture),
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

    private string? DescribedBy
        => string.Join(' ', new[]
            {
                AriaDescribedBy,
                HelperContent is null && string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
                string.IsNullOrWhiteSpace(WarningText) ? null : WarningId,
                string.IsNullOrWhiteSpace(EffectiveErrorText) ? null : ErrorId,
            }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private string Classname
        => SUIClassBuilder.Default("sui-field")
            .AddClass(Class)
            .Build();

    /// <summary>Releases the validation binding registered with the surrounding <c>EditContext</c>.</summary>
    public void Dispose() => _field.Dispose();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
#pragma warning disable CS0618 // deliberate bridge for the obsolete parameter
        MergeLegacyAttributes(UserAttributes);
#pragma warning restore CS0618
        _field.Configure(FormContext, ValueExpression, () => _ = InvokeAsync(StateHasChanged));
    }

    /// <summary>
    /// Moves keyboard focus to this field's control, in either mode.
    /// </summary>
    /// <remarks>
    /// An editor that appears in response to an action has to take focus when
    /// it appears, and only the caller knows when that was. The component
    /// already focuses its own input when the clear action runs; this is the
    /// same handle, made available to the page — as <c>SUIButton</c> does.
    /// </remarks>
    public ValueTask FocusAsync(bool preventScroll = false)
        => _inputElement.FocusAsync(preventScroll);

    private Task OnChanged(ChangeEventArgs e)
        => Immediate ? Task.CompletedTask : SetValue(e.Value);

    private Task OnInput(ChangeEventArgs e)
        => Immediate ? SetValue(e.Value) : Task.CompletedTask;

    private async Task ClearAsync()
    {
        await ValueChanged.InvokeAsync(default);
        _field.Notify();
        await _inputElement.FocusAsync(preventScroll: true);
    }

    private async Task SetValue(object? raw)
    {
        if (IsNumericInput)
        {
            if (!TryParseNumeric(raw?.ToString(), out var numeric))
            {
                _field.Notify("Informe um valor válido.");
                return;
            }
            await ValueChanged.InvokeAsync(numeric);
            _field.Notify();
            return;
        }

        if (!BindConverter.TryConvertTo<TValue>(raw, CultureInfo.CurrentCulture, out var converted))
        {
            _field.Notify("Informe um valor válido.");
            return;
        }
        await ValueChanged.InvokeAsync(converted);
        _field.Notify();
    }

    /// <summary>
    ///     Numeric inputs report dot-decimal text without thousands separators,
    ///     regardless of the user culture: parsing "0.40" as pt-BR turns forty
    ///     cents into forty reais, the exact bug this guards against.
    /// </summary>
    private static bool TryParseNumeric(string? text, out TValue? parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var culture = CultureInfo.InvariantCulture;
        var style = NumberStyles.Float;
        var type = typeof(TValue);

        if (type == typeof(decimal)) { if (decimal.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(double)) { if (double.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(float)) { if (float.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(int)) { if (int.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(long)) { if (long.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(short)) { if (short.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(byte)) { if (byte.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(sbyte)) { if (sbyte.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(uint)) { if (uint.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(ulong)) { if (ulong.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }
        if (type == typeof(ushort)) { if (ushort.TryParse(text, style, culture, out var v)) { parsed = (TValue)(object)v; return true; } return false; }

        // Unknown numeric binding: fall back to the framework converter with the
        // invariant culture rather than inventing a conversion.
        return BindConverter.TryConvertTo<TValue>(text, culture, out parsed);
    }
}
