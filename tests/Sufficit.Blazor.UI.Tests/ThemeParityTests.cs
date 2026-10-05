using System.Text.RegularExpressions;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// The dark palette exists twice on purpose: <c>SUITheme.Dark</c> feeds the
/// provider's injected tokens and the <c>[data-sui-theme="dark"]</c> block in
/// <c>sui-foundations.css</c> is the no-provider fallback. Two sources for the
/// same fact drift silently — this contract renders the preset through
/// <see cref="SUIThemeCssWriter"/> and compares, token by token, every literal
/// color both sides declare.
/// </summary>
public sealed class ThemeParityTests
{
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void AuthoredFallback_MatchesThePresetTokenByToken(string mode)
    {
        var theme = mode == "dark" ? SUITheme.Dark : SUITheme.Light;
        var written = ParseDeclarations(SUIThemeCssWriter.Write(theme));

        var foundations = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));
        // Strip comments first: the header prose mentions the dark selector,
        // which would otherwise match before the real block.
        foundations = Regex.Replace(foundations, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        var blockSelector = mode == "dark"
            ? @"\[data-sui-theme=""dark""\][^{]*\{"
            : @"^:root\s*\{";
        var match = new Regex(blockSelector, RegexOptions.Multiline).Match(foundations);
        Assert.True(match.Success, $"authored {mode} token block not found");
        var body = ExtractBlock(foundations, match.Index + match.Length);
        var authored = ParseDeclarations(body);

        var mismatches = new List<string>();
        foreach (var (token, value) in authored)
        {
            // Alias/computed values (var(), color-mix(), calc()) are resolved
            // at runtime and intentionally differ from the preset strings.
            if (!IsLiteralColor(value))
                continue;
            if (!written.TryGetValue(token, out var preset))
                continue; // token not published by the writer: out of scope here

            if (!string.Equals(Normalize(value), Normalize(preset), StringComparison.Ordinal))
                mismatches.Add($"{token}: authored {value} x preset {preset}");
        }

        Assert.Empty(mismatches);
    }

    private static Dictionary<string, string> ParseDeclarations(string css)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match declaration in new Regex(@"(--[\w-]+)\s*:\s*([^;{}]+);").Matches(css))
            result[declaration.Groups[1].Value] = declaration.Groups[2].Value.Trim();
        return result;
    }

    private static string ExtractBlock(string css, int from)
    {
        var depth = 1;
        var index = from;
        while (index < css.Length && depth > 0)
        {
            if (css[index] == '{') depth++;
            else if (css[index] == '}') depth--;
            index++;
        }
        return css[from..(index - 1)];
    }

    private static bool IsLiteralColor(string value) =>
        Regex.IsMatch(value, @"^#[0-9a-fA-F]{3,8}$") || Regex.IsMatch(value, @"^rgba?\(");

    private static string Normalize(string value)
    {
        var lowered = Regex.Replace(value.ToLowerInvariant(), @"\s+", string.Empty);
        // #fff and #ffffff are the same color; presets spell it long.
        return Regex.Replace(lowered, @"^#([0-9a-f])([0-9a-f])([0-9a-f])$",
            match => $"#{match.Groups[1].Value}{match.Groups[1].Value}{match.Groups[2].Value}{match.Groups[2].Value}{match.Groups[3].Value}{match.Groups[3].Value}");
    }
}
