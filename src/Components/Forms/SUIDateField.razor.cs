using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Culture-aware date picker: a button trigger opens a calendar grid in the top layer, navigable with the arrow keys, Home/End and PageUp/PageDown (Shift for years); the value is a <see cref="DateOnly"/>.</summary>
public partial class SUIDateField
{
    [CascadingParameter] private Microsoft.AspNetCore.Components.Forms.EditContext? FormContext { get; set; }
    private readonly SUIFieldBinding<DateOnly?> _field = new();
    private string? EffectiveErrorText => ErrorText ?? _field.Error;

    /// <summary>Selected date, or null when empty.</summary>
    [Parameter]
    public DateOnly? Value { get; set; }

    /// <summary>Raised when a day is picked, Today is pressed or the value is cleared; enables <c>@bind-Value</c>.</summary>
    [Parameter]
    public EventCallback<DateOnly?> ValueChanged { get; set; }

    /// <summary>Field expression used to bind validation messages from the surrounding <c>EditContext</c>.</summary>
    [Parameter]
    public Expression<Func<DateOnly?>>? ValueExpression { get; set; }

    /// <summary>Visible label; also names the trigger and the calendar dialog for assistive technology.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Text shown while no date is selected. Defaults to <c>dd/mm/aaaa</c> for Portuguese cultures and <c>mm/dd/yyyy</c> otherwise.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Helper text rendered under the field and linked through <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>Explicit error message; takes precedence over validation messages from the form.</summary>
    [Parameter]
    public string? ErrorText { get; set; }

    /// <summary>Forces the invalid state (<c>aria-invalid</c>) without an error message.</summary>
    [Parameter]
    public bool Invalid { get; set; }

    /// <summary>Marks the trigger <c>aria-required</c> and disables the Clear action.</summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>Disables the trigger and every day, and closes an open calendar.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Earliest selectable date; earlier days are disabled and month navigation stops before them.</summary>
    [Parameter]
    public DateOnly? Min { get; set; }

    /// <summary>Latest selectable date; later days are disabled and month navigation stops after them.</summary>
    [Parameter]
    public DateOnly? Max { get; set; }

    /// <summary>Culture for date formatting, weekday names, first day of week and text direction. Defaults to <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>Id of the trigger button; generated when omitted.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>When set, renders a hidden input with the ISO date (<c>yyyy-MM-dd</c>) for plain form posts.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>Additional CSS class for the root element.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Accessible name of the trigger when no <see cref="Label"/> is given. Defaults to a localized "Choose date".</summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>Extra element ids prepended to the trigger's <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>Unmatched attributes forwarded to the trigger button.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    private readonly string _generatedId = $"sui-date-field-{Guid.NewGuid():N}";
    private ElementReference _rootElement;
    private ElementReference _triggerElement;
    private ElementReference _popoverElement;
    private IJSObjectReference? _module;
    private DotNetObjectReference<SUIDateField>? _dotNetReference;
    private DateOnly _displayMonth;
    private DateOnly _focusedDate;
    private DateOnly? _lastValue;
    private string? _lastCulture;
    private bool _initialized;
    private bool _connected;
    private bool _open;
    private bool _interopOpen;
    private bool _focusPending;
    private bool _restoreFocusPending;

    private CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentUICulture;
    private bool IsPortuguese => EffectiveCulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase);
    private DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private DateOnly FocusedDate => _focusedDate;
    private string EffectiveId => string.IsNullOrWhiteSpace(Id) ? _generatedId : Id.Trim();
    private string LabelId => $"{EffectiveId}-label";
    private string HelperId => $"{EffectiveId}-helper";
    private string ErrorId => $"{EffectiveId}-error";
    private string PopoverId => $"{EffectiveId}-calendar";
    private string MonthId => $"{EffectiveId}-month";
    private string? TriggerLabelledBy => string.IsNullOrWhiteSpace(Label) ? null : LabelId;
    private string? TriggerAriaLabel => string.IsNullOrWhiteSpace(Label)
        ? AriaLabel ?? ChooseDateText
        : null;
    private bool HasError => Invalid || !string.IsNullOrWhiteSpace(EffectiveErrorText);
    private string? DescribedBy
    {
        get
        {
            var value = string.Join(' ', new[]
            {
                AriaDescribedBy,
                string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
                string.IsNullOrWhiteSpace(EffectiveErrorText) ? null : ErrorId,
            }.Where(item => !string.IsNullOrWhiteSpace(item)));
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    private string Classname
        => SUIClassBuilder.Default("sui-field sui-date-field")
            .AddClass(Class)
            .AddClass(_open ? "sui-date-field--open" : null)
            .Build();
    private string PopoverClass => _open
        ? "sui-select__menu sui-select__menu--open sui-date-field__popover sui-date-field__popover--open"
        : "sui-select__menu sui-date-field__popover";
    private string FormValue => Value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
    private string EffectivePlaceholder => !string.IsNullOrWhiteSpace(Placeholder)
        ? Placeholder
        : IsPortuguese ? "dd/mm/aaaa" : "mm/dd/yyyy";
    private string MonthLabel => _displayMonth.ToString("Y", EffectiveCulture);
    private string ChooseDateText => IsPortuguese ? "Escolher data" : "Choose date";
    private string CalendarAriaLabel => string.IsNullOrWhiteSpace(Label)
        ? ChooseDateText
        : IsPortuguese ? $"Escolher data para {Label}" : $"Choose date for {Label}";
    private string PreviousMonthText => IsPortuguese ? "Mês anterior" : "Previous month";
    private string NextMonthText => IsPortuguese ? "Próximo mês" : "Next month";
    private string TodayText => IsPortuguese ? "Hoje" : "Today";
    private string ClearText => IsPortuguese ? "Limpar" : "Clear";

    private IReadOnlyList<WeekdayLabel> Weekdays
    {
        get
        {
            var format = EffectiveCulture.DateTimeFormat;
            var first = (int)format.FirstDayOfWeek;
            return Enumerable.Range(0, 7)
                .Select(offset => (DayOfWeek)((first + offset) % 7))
                .Select(day => new WeekdayLabel(
                    format.AbbreviatedDayNames[(int)day].TrimEnd('.'),
                    format.DayNames[(int)day]))
                .ToArray();
        }
    }

    /// <summary>Wires form validation and resets the focused day and visible month when the value or culture changes while the calendar is closed.</summary>
    protected override void OnParametersSet()
    {
        _field.Configure(FormContext, ValueExpression, () => _ = InvokeAsync(StateHasChanged));
        var cultureName = EffectiveCulture.Name;
        if (!_initialized || (!_open && Value != _lastValue) || _lastCulture != cultureName)
        {
            _focusedDate = Clamp(Value ?? Today);
            _displayMonth = StartOfMonth(_focusedDate);
            _initialized = true;
        }

        if (Disabled)
        {
            _open = false;
        }

        _lastValue = Value;
        _lastCulture = cultureName;
    }

    /// <summary>Loads the browser module once, then opens or closes the top-layer popover and moves focus to the focused day.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _module ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Sufficit.Blazor.UI/Components/Forms/SUIDateField.razor.js");
        if (!_connected)
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync(
                "connectDateField", _rootElement, _triggerElement, _popoverElement, _dotNetReference);
            _connected = true;
        }

        if (_open)
        {
            await _module.InvokeVoidAsync("openDateField", _triggerElement, _popoverElement);
            _interopOpen = true;
            if (_focusPending)
            {
                _focusPending = false;
                await _module.InvokeVoidAsync("focusDate", _popoverElement, IsoDate(_focusedDate));
            }
        }
        else if (_interopOpen)
        {
            await _module.InvokeVoidAsync(
                "closeDateField", _popoverElement, _triggerElement, _restoreFocusPending);
            _interopOpen = false;
            _restoreFocusPending = false;
        }
    }

    private void Toggle()
    {
        if (Disabled)
        {
            return;
        }

        _open = !_open;
        if (_open)
        {
            _focusedDate = Clamp(Value ?? Today);
            _displayMonth = StartOfMonth(_focusedDate);
            _focusPending = true;
        }
    }

    private void OnTriggerKeyDown(KeyboardEventArgs args)
    {
        if (Disabled)
        {
            return;
        }

        if (args.Key is "Enter" or " " or "ArrowDown")
        {
            if (!_open)
            {
                Toggle();
            }
        }
        else if (args.Key == "Escape")
        {
            Close(restoreFocus: true);
        }
    }

    private void OnPopoverKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            Close(restoreFocus: true);
        }
    }

    private async Task OnDayKeyDown(KeyboardEventArgs args, DateOnly date)
    {
        switch (args.Key)
        {
            case "Enter":
            case " ":
                await SelectDateAsync(date);
                return;
            case "ArrowLeft":
                Focus(AddDays(date, EffectiveCulture.TextInfo.IsRightToLeft ? 1 : -1));
                return;
            case "ArrowRight":
                Focus(AddDays(date, EffectiveCulture.TextInfo.IsRightToLeft ? -1 : 1));
                return;
            case "ArrowUp":
                Focus(AddDays(date, -7));
                return;
            case "ArrowDown":
                Focus(AddDays(date, 7));
                return;
            case "Home":
                Focus(AddDays(date, -WeekdayOffset(date)));
                return;
            case "End":
                Focus(AddDays(date, 6 - WeekdayOffset(date)));
                return;
            case "PageUp":
                Focus(AddMonths(date, args.ShiftKey ? -12 : -1));
                return;
            case "PageDown":
                Focus(AddMonths(date, args.ShiftKey ? 12 : 1));
                return;
        }
    }

    private void MoveMonth(int delta)
    {
        if (!CanMoveMonth(delta))
        {
            return;
        }

        Focus(AddMonths(_focusedDate, delta));
    }

    private void Focus(DateOnly date)
    {
        _focusedDate = Clamp(date);
        _displayMonth = StartOfMonth(_focusedDate);
        _focusPending = true;
    }

    private async Task SelectDateAsync(DateOnly date)
    {
        if (IsDateDisabled(date))
        {
            return;
        }

        await ValueChanged.InvokeAsync(date);
        _field.Notify();
        Close(restoreFocus: true);
    }

    private Task SelectTodayAsync() => SelectDateAsync(Today);

    private async Task ClearAsync()
    {
        if (Required)
        {
            return;
        }

        await ValueChanged.InvokeAsync(null);
        _field.Notify();
        Close(restoreFocus: true);
    }

    private void Close(bool restoreFocus)
    {
        _open = false;
        _restoreFocusPending = restoreFocus;
    }

    /// <summary>Called from the browser on an outside pointer-down or when another date field opens; closes without restoring focus.</summary>
    [JSInvokable]
    public Task CloseFromJs()
    {
        if (!_open)
        {
            return Task.CompletedTask;
        }

        _open = false;
        _restoreFocusPending = false;
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>Releases the form binding, the browser listeners and the .NET reference.</summary>
    public async ValueTask DisposeAsync()
    {
        _field.Dispose();
        if (_module is not null)
        {
            try
            {
                if (_connected)
                {
                    await _module.InvokeVoidAsync("disconnectDateField", _rootElement, _popoverElement);
                }
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Browser-owned listeners disappear with the disconnected circuit.
            }
        }

        _dotNetReference?.Dispose();
    }
}
