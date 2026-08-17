using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

public partial class SUIDateField
{
    [Parameter]
    public DateOnly? Value { get; set; }

    [Parameter]
    public EventCallback<DateOnly?> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<DateOnly?>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [Parameter]
    public bool Invalid { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public DateOnly? Min { get; set; }

    [Parameter]
    public DateOnly? Max { get; set; }

    [Parameter]
    public CultureInfo? Culture { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

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
    private bool HasError => Invalid || !string.IsNullOrWhiteSpace(ErrorText);
    private string? DescribedBy
    {
        get
        {
            var value = string.Join(' ', new[]
            {
                AriaDescribedBy,
                string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
                string.IsNullOrWhiteSpace(ErrorText) ? null : ErrorId,
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

    protected override void OnParametersSet()
    {
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
        Close(restoreFocus: true);
    }

    private void Close(bool restoreFocus)
    {
        _open = false;
        _restoreFocusPending = restoreFocus;
    }

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

    private DateOnly CalendarDate(int offset)
    {
        var firstDay = (int)EffectiveCulture.DateTimeFormat.FirstDayOfWeek;
        var leading = ((int)_displayMonth.DayOfWeek - firstDay + 7) % 7;
        return AddDays(_displayMonth, offset - leading);
    }

    private bool CanMoveMonth(int delta)
    {
        try
        {
            var month = _displayMonth.AddMonths(delta);
            var monthEnd = new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
            return (Min is null || monthEnd >= Min) && (Max is null || month <= Max);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private DateOnly Clamp(DateOnly date)
    {
        if (Min is { } min && date < min)
        {
            return min;
        }
        if (Max is { } max && date > max)
        {
            return max;
        }
        return date;
    }

    private bool IsDateDisabled(DateOnly date)
        => Disabled || Min is { } min && date < min || Max is { } max && date > max;
    private int WeekdayOffset(DateOnly date)
        => ((int)date.DayOfWeek - (int)EffectiveCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7;
    private string FormatDate(DateOnly date) => date.ToString("d", EffectiveCulture);
    private string FormatLongDate(DateOnly date) => date.ToString("D", EffectiveCulture);
    private static string IsoDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    private static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);
    private static DateOnly AddDays(DateOnly date, int days)
    {
        try
        {
            return date.AddDays(days);
        }
        catch (ArgumentOutOfRangeException)
        {
            return days < 0 ? DateOnly.MinValue : DateOnly.MaxValue;
        }
    }

    private static DateOnly AddMonths(DateOnly date, int months)
    {
        try
        {
            var month = StartOfMonth(date).AddMonths(months);
            return new DateOnly(month.Year, month.Month, Math.Min(date.Day, DateTime.DaysInMonth(month.Year, month.Month)));
        }
        catch (ArgumentOutOfRangeException)
        {
            return months < 0 ? DateOnly.MinValue : DateOnly.MaxValue;
        }
    }

    private string DayClass(DateOnly date)
        => SUIClassBuilder.Default("sui-btn sui-btn--icon sui-btn--sm sui-btn--text sui-btn--color-default sui-date-field__day")
            .AddClass(date.Month != _displayMonth.Month || date.Year != _displayMonth.Year
                ? "sui-date-field__day--outside" : null)
            .AddClass(Value == date ? "sui-date-field__day--selected" : null)
            .AddClass(IsToday(date) ? "sui-date-field__day--today" : null)
            .Build();
    private bool IsToday(DateOnly date) => date == Today;

    public async ValueTask DisposeAsync()
    {
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

    private readonly record struct WeekdayLabel(string Short, string Full);
}
