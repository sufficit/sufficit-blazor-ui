using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Sufficit.Blazor.UI.Artwork;

/// <summary>
/// Path math for <see cref="SUIArtwork"/>. Pure and deterministic: the same
/// inputs always draw the same picture, so results are cached per shape.
/// </summary>
internal static class SUIArtworkGeometry
{
    public const int Width = 1600;
    public const int Height = 640;
    private const int Step = 16;

    private static readonly ConcurrentDictionary<(int Lines, int Peak, int Seed), string[]> Cache = new();

    /// <summary>Index of the accent lead line for a field of <paramref name="lines"/> lines.</summary>
    public static int LeadLine(int lines) => (int)Math.Round(lines * 0.575);

    /// <summary>
    /// SVG path data for each line of the signal-wave field.
    /// </summary>
    /// <param name="lines">Number of lines.</param>
    /// <param name="peak">Horizontal peak position, 0-1 of the width.</param>
    /// <param name="seed">Phase seed; 0 is the reference drawing.</param>
    public static IReadOnlyList<string> SignalWaves(int lines, double peak, int seed)
        => Cache.GetOrAdd((lines, (int)Math.Round(peak * 100), seed), key => Draw(key.Lines, key.Peak / 100.0, key.Seed));

    private static string[] Draw(int lines, double peak, int seed)
    {
        var peakX = peak * Width;
        var phase = seed * 0.7;
        var paths = new string[lines];
        var builder = new StringBuilder(2048);

        for (var i = 0; i < lines; i++)
        {
            builder.Clear();
            var baseline = 56 + i * (Height - 112.0) / (lines - 1);
            var centre = Math.Max(1 - Math.Abs(i - lines * 0.55) / (lines * 0.55), 0);
            var amplitude = 10 + 42 * centre;
            var bump = 28 + 60 * centre;

            for (var x = -Step; x <= Width + Step; x += Step)
            {
                var envelope = SmoothStep(peakX - 450, peakX + 10, x);
                var ripple = Math.Sin(x * 0.0062 + i * 0.31 + phase)
                    + 0.55 * Math.Sin(x * 0.0131 - i * 0.47 + phase * 1.3)
                    + 0.25 * Math.Sin(x * 0.027 + i * 0.83 - phase);
                var crest = Math.Exp(-Math.Pow((x - peakX) / 130, 2)) * bump;
                var y = baseline + envelope * (amplitude * ripple - crest);

                builder.Append(x == -Step ? 'M' : 'L')
                    .Append(x.ToString(CultureInfo.InvariantCulture))
                    .Append(' ')
                    .Append(y.ToString("0.#", CultureInfo.InvariantCulture));
            }

            paths[i] = builder.ToString();
        }

        return paths;
    }

    private static double SmoothStep(double edge0, double edge1, double x)
    {
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
