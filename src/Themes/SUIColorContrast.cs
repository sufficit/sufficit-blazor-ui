using System.Globalization;

namespace Sufficit.Blazor.UI.Themes;

/// <summary>WCAG contrast for opaque six-digit sRGB colors. CSS expressions require browser resolution first.</summary>
public static class SUIColorContrast
{
    /// <summary>
    /// Computes the WCAG 2 contrast ratio (1 to 21) between two <c>#rrggbb</c> colors.
    /// Returns false, with <paramref name="ratio"/> set to 0, when either value is not a six-digit hex color.
    /// </summary>
    public static bool TryGetRatio(string? foreground, string? background, out double ratio)
    {
        ratio = 0;
        if (!TryLuminance(foreground, out var first) || !TryLuminance(background, out var second)) return false;
        ratio = (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
        return true;
    }

    /// <summary>Whether <paramref name="value"/> is an opaque six-digit hex color (<c>#rrggbb</c>); shorthand, alpha and CSS functions are rejected.</summary>
    public static bool IsHexColor(string? value)
        => value is { Length: 7 } && value[0] == '#'
            && int.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

    private static bool TryLuminance(string? value, out double luminance)
    {
        luminance = 0;
        if (!IsHexColor(value)) return false;
        var rgb = int.Parse(value!.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        static double Linear(int component)
        {
            var channel = component / 255d;
            return channel <= .04045 ? channel / 12.92 : Math.Pow((channel + .055) / 1.055, 2.4);
        }
        luminance = .2126 * Linear(rgb >> 16) + .7152 * Linear((rgb >> 8) & 255) + .0722 * Linear(rgb & 255);
        return true;
    }
}
