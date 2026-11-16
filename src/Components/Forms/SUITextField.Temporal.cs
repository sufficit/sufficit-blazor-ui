using System.Globalization;

namespace Sufficit.Blazor.UI.Components;

public partial class SUITextField<TValue>
{
    private static Type TemporalType => Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
    private bool IsCivilDateInput => string.Equals(InputType, "date", StringComparison.OrdinalIgnoreCase)
        && (TemporalType == typeof(DateOnly) || TemporalType == typeof(DateTime));
    private bool IsClockInput => string.Equals(InputType, "time", StringComparison.OrdinalIgnoreCase)
        && (TemporalType == typeof(TimeOnly) || TemporalType == typeof(TimeSpan));
    private bool IsTemporalInput => IsCivilDateInput || IsClockInput;

    // Native date/time values are ISO civil values, independent of display culture.
    // Never round-trip them through UTC or a current-culture numeric converter.
    private string? TemporalInputValue => Value switch
    {
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly time => FormatClock(time),
        TimeSpan time when time >= TimeSpan.Zero && time < TimeSpan.FromDays(1) => FormatClock(TimeOnly.FromTimeSpan(time)),
        _ => null
    };

    // HTML time supports milliseconds; model values are never mutated by rendering.
    private static string FormatClock(TimeOnly time)
        => time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

    private bool TryParseTemporal(string? text, out TValue? converted)
    {
        converted = default;
        if (string.IsNullOrWhiteSpace(text)) return default(TValue) is null;
        object value;
        if (IsCivilDateInput)
        {
            if (!DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return false;
            value = TemporalType == typeof(DateOnly) ? (object)date : date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        }
        else
        {
            if (!TimeOnly.TryParseExact(text, ["HH:mm", "HH:mm:ss", "HH:mm:ss.FFF"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)) return false;
            value = TemporalType == typeof(TimeOnly) ? (object)time : time.ToTimeSpan();
        }
        converted = (TValue)value;
        return true;
    }
}
