using System.Globalization;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

public partial class SUIDateField
{
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

    private readonly record struct WeekdayLabel(string Short, string Full);
}
