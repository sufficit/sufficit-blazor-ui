using System.Globalization;

namespace Sufficit.Blazor.UI.Themes;

/// <summary>WCAG contrast for opaque six-digit sRGB colors. CSS expressions require browser resolution first.</summary>
public static class SUIColorContrast
{
    public static bool TryGetRatio(string? foreground, string? background, out double ratio)
    {
        ratio = 0;
        if (!TryLuminance(foreground, out var first) || !TryLuminance(background, out var second)) return false;
        ratio = (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
        return true;
    }

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
