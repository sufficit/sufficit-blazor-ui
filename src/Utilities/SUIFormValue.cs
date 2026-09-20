using System;
using System.Globalization;

namespace Sufficit.Blazor.UI.Utilities;

/// <summary>
/// Turns a bound value into the token a plain HTML form submits. The token is
/// read by a server, not by a person, so it never follows the page culture:
/// a decimal that becomes "1,5" on a Portuguese page is a bug the receiver
/// cannot defend against.
/// </summary>
internal static class SUIFormValue
{
    public static string? Format(object? value)
        => value switch
        {
            null => null,
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset offset => offset.ToString("O", CultureInfo.InvariantCulture),
            bool flag => flag ? "true" : "false",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };

    public static string? Format<T>(T? value, Func<T, string>? custom)
        => value is null ? null : custom is not null ? custom(value) : Format((object)value);
}
