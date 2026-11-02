using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Semantic chips composite text and fill through color-mix, which axe may skip. This computes what
/// the browser paints from the shipped rule, against the light and dark palettes in use, including a
/// consumer warning lighter than the library default.
/// </summary>
public class ChipContrastTests
{
    private const string LightSurface = "#ffffff";
    private const string LightSurface2 = "#f8fafc";
    private const string LightText = "#111827";
    private const string DarkSurface = "#0f172a";
    private const string DarkSurface2 = "#1e293b";
    private const string DarkText = "#f1f5f9";

    public static TheoryData<string, string, string, string> Palettes()
    {
        var data = new TheoryData<string, string, string, string>();
        foreach (var (tone, color) in new[]
                 {
                     ("warning", "#92400e"), ("warning", "#b45309"), ("error", "#b91c1c"),
                     ("success", "#166534"), ("info", "#0369a1"),
                 })
        {
            data.Add(tone, color, LightSurface, LightText);
            data.Add(tone, color, LightSurface2, LightText);
        }
        foreach (var (tone, color) in new[]
                 {
                     ("warning", "#fbbf24"), ("error", "#fca5a5"), ("success", "#4ade80"), ("info", "#7dd3fc"),
                 })
        {
            data.Add(tone, color, DarkSurface, DarkText);
            data.Add(tone, color, DarkSurface2, DarkText);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Palettes))]
    public void TintedChipTextReachesAa(string tone, string color, string surface, string text)
    {
        var css = File.ReadAllText(Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css"));
        var rule = Regex.Match(css, $@"\.sui-chip--{tone}\s*\{{[^}}]*\}}").Value;
        var fill = Percent(rule, $@"--_chip-bg:\s*color-mix\(in srgb,\s*var\(--sui-color-{tone}\)\s*(\d+)%,\s*transparent\)");
        var ink = Percent(rule, $@"--_chip-fg:\s*color-mix\(in srgb,\s*var\(--sui-color-{tone}\)\s*(\d+)%,\s*var\(--sui-text-primary\)\)");

        var background = Mix(Rgb(color), Rgb(surface), fill);
        var foreground = Mix(Rgb(color), Rgb(text), ink);

        Assert.True(Contrast(foreground, background) >= 4.5,
            $"{tone} {color} on {surface}: {Contrast(foreground, background):0.00}:1");
    }

    private static double Percent(string rule, string pattern)
    {
        var match = Regex.Match(rule, pattern);
        Assert.True(match.Success, $"Rule changed shape: {rule}");
        return double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) / 100;
    }

    private static double[] Rgb(string hex) => new[] { 1, 3, 5 }
        .Select(i => (double)Convert.ToInt32(hex.Substring(i, 2), 16)).ToArray();

    private static double[] Mix(double[] a, double[] b, double share)
        => a.Zip(b, (x, y) => x * share + y * (1 - share)).ToArray();

    private static double Luminance(double[] rgb)
    {
        var channels = rgb.Select(v => v / 255).Select(v => v <= .03928 ? v / 12.92 : Math.Pow((v + .055) / 1.055, 2.4)).ToArray();
        return .2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2];
    }

    private static double Contrast(double[] a, double[] b)
    {
        var (light, dark) = (Math.Max(Luminance(a), Luminance(b)), Math.Min(Luminance(a), Luminance(b)));
        return (light + .05) / (dark + .05);
    }
}
