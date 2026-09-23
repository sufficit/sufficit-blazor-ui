using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
///     Single-line or multiline text input bound to <typeparamref name="T"/>.
///     Numeric input types (<c>number</c>, <c>range</c>) are parsed and rendered
///     with the invariant culture, because the browser only speaks the
///     dot-decimal form: rendering 0.40m as "0,40" makes it drop the value, and
///     parsing "0.40" as pt-BR turns forty cents into forty reais.
/// </summary>
public partial class SUITextField<T>
{
    [CascadingParameter] private Microsoft.AspNetCore.Components.Forms.EditContext? FormContext { get; set; }
    private readonly SUIFieldBinding<T?> _field = new();
    private string? EffectiveErrorText => ErrorText ?? _field.Error;

    /// <summary>Current value, or the type default when empty.</summary>
    [Parameter]
    public T? Value { get; set; }

    /// <summary>Raised when the user commits a value; enables <c>@bind-Value</c>.</summary>
    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }

    /// <summary>Field expression used to bind validation messages from the surrounding <c>EditContext</c>.</summary>
    [Parameter]
    public Expression<Func<T?>>? ValueExpression { get; set; }

    /// <summary>Visible label, rendered above the control and bound to it for assistive technology.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Hint shown inside the empty control.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Static helper text rendered below the control.</summary>
    [Parameter]
    public string? HelperText { get; set; }

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

    /// <summary>Additional css classes appended to the field's root element.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Any other attributes are splatted onto the underlying input or textarea.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    private ElementReference _inputElement;
    private readonly string _generatedId = $"sui-text-field-{Guid.NewGuid():N}";
    private string EffectiveId => string.IsNullOrWhiteSpace(Id) ? _generatedId : Id;
    private int EffectiveRows => Math.Clamp(Rows, 2, 20);
    private string HelperId => $"{EffectiveId}-helper";
    private string ErrorId => $"{EffectiveId}-error";
    private bool HasError => Invalid || !string.IsNullOrWhiteSpace(EffectiveErrorText);
    private bool CanClear
        => Clearable && !Disabled && !string.IsNullOrEmpty(Value?.ToString());
    private string? InputPaddingStyle
        => CanClear && !string.IsNullOrWhiteSpace(AdornmentIcon)
            ? "padding-inline-end:5rem"
            : CanClear || !string.IsNullOrWhiteSpace(AdornmentIcon)
                ? "padding-inline-end:3rem"
                : null;
    private string AdornmentStyle
        => $"position:absolute;inset-block-start:50%;inset-inline-end:{(CanClear ? "2.75rem" : "var(--sui-space-3)")};color:var(--sui-text-secondary);pointer-events:none;transform:translateY(-50%)";

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

    private static string? FormatNumeric(T? value)
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
                string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
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
    protected override void OnParametersSet() => _field.Configure(FormContext, ValueExpression, () => _ = InvokeAsync(StateHasChanged));

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

        if (!BindConverter.TryConvertTo<T>(raw, CultureInfo.CurrentCulture, out var converted))
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
    private static bool TryParseNumeric(string? text, out T? parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var culture = CultureInfo.InvariantCulture;
        var style = NumberStyles.Float;
        var type = typeof(T);

        if (type == typeof(decimal)) { if (decimal.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(double)) { if (double.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(float)) { if (float.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(int)) { if (int.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(long)) { if (long.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(short)) { if (short.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(byte)) { if (byte.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(sbyte)) { if (sbyte.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(uint)) { if (uint.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(ulong)) { if (ulong.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }
        if (type == typeof(ushort)) { if (ushort.TryParse(text, style, culture, out var v)) { parsed = (T)(object)v; return true; } return false; }

        // Unknown numeric binding: fall back to the framework converter with the
        // invariant culture rather than inventing a conversion.
        return BindConverter.TryConvertTo<T>(text, culture, out parsed);
    }
}
