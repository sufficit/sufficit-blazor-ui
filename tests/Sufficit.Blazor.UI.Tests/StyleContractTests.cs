using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Contract for the authored stylesheets. The library ships CSS into consumer
/// applications, so anything unprefixed, forced with <c>!important</c> or
/// stacked with a magic z-index becomes their problem, not ours.
/// </summary>
public sealed class StyleContractTests
{
    /// <summary>Classes owned by the framework or shared as state hooks.</summary>
    private static readonly string[] AllowedBareClasses =
    [
        "active",       // Blazor NavLink active class
        "theme-dark",   // legacy dark-mode alias
    ];

    private static readonly Regex ClassSelector = new(@"(?<![\w-])\.([a-zA-Z_][\w-]*)", RegexOptions.Compiled);
    // Only real custom properties: a var() read or a declaration. A bare
    // "--foo" match would also hit every BEM modifier (.sui-btn--primary).
    private static readonly Regex CustomPropertyRead =
        new(@"var\(\s*(--[a-zA-Z_][\w-]*)", RegexOptions.Compiled);

    private static readonly Regex CustomPropertyDeclaration =
        new(@"^\s*(--[a-zA-Z_][\w-]*)\s*:", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex ZIndexLiteral = new(@"z-index:\s*(-?\d+)", RegexOptions.Compiled);

    public static TheoryData<string> Stylesheets()
    {
        var data = new TheoryData<string>();
        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.css"))
        {
            if (RepositoryLayout.Relative(file).StartsWith("src/wwwroot/", StringComparison.Ordinal))
                continue; // generated bundle

            data.Add(RepositoryLayout.Relative(file));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Stylesheets))]
    public void ClassSelectors_AreNamespacedOrKnownStateHooks(string relativePath)
    {
        var offenders = new List<string>();

        foreach (var line in ReadDeclarationLines(relativePath))
        {
            foreach (Match match in ClassSelector.Matches(line))
            {
                var name = match.Groups[1].Value;
                if (name.StartsWith("sui-", StringComparison.Ordinal)
                    || name.StartsWith("is-", StringComparison.Ordinal)
                    || name.StartsWith("has-", StringComparison.Ordinal)
                    || AllowedBareClasses.Contains(name, StringComparer.Ordinal))
                {
                    continue;
                }

                offenders.Add($".{name} in: {line.Trim()}");
            }
        }

        Assert.True(offenders.Count == 0,
            $"{relativePath} leaks unprefixed classes into consumers:{Environment.NewLine}"
            + string.Join(Environment.NewLine, offenders.Distinct()));
    }

    [Theory]
    [MemberData(nameof(Stylesheets))]
    public void CustomProperties_UseAnOwnedPrefix(string relativePath)
    {
        var text = File.ReadAllText(Path.Combine(RepositoryLayout.Root, relativePath));

        var offenders = CustomPropertyRead.Matches(text)
            .Concat(CustomPropertyDeclaration.Matches(text))
            .Select(match => match.Groups[1].Value)
            .Where(name => !name.StartsWith("--sui-", StringComparison.Ordinal)
                && !name.StartsWith("--_", StringComparison.Ordinal)
                && !name.StartsWith("--sufficit-", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.True(offenders.Length == 0,
            $"{relativePath} reads or declares foreign design tokens: {string.Join(", ", offenders)}");
    }

    [Theory]
    [MemberData(nameof(Stylesheets))]
    public void ZIndex_ComesFromTheTokenScale(string relativePath)
    {
        var offenders = new List<string>();

        foreach (var line in File.ReadAllLines(Path.Combine(RepositoryLayout.Root, relativePath)))
        {
            var match = ZIndexLiteral.Match(line);
            if (!match.Success)
                continue;

            // 0..2 are local stacking contexts, not app-wide layering.
            if (int.TryParse(match.Groups[1].Value, out var value) && Math.Abs(value) <= 2)
                continue;

            offenders.Add(line.Trim());
        }

        Assert.True(offenders.Count == 0,
            $"{relativePath} must layer through --sui-z-* tokens:{Environment.NewLine}"
            + string.Join(Environment.NewLine, offenders));
    }

    [Theory]
    [MemberData(nameof(Stylesheets))]
    public void Stylesheet_DoesNotForceRulesWithImportant(string relativePath)
    {
        var offenders = File.ReadAllLines(Path.Combine(RepositoryLayout.Root, relativePath))
            .Where(line => line.Contains("!important", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .ToArray();

        Assert.True(offenders.Length <= 1,
            $"{relativePath} relies on !important more than the single documented exception:{Environment.NewLine}"
            + string.Join(Environment.NewLine, offenders));
    }

    [Theory]
    [MemberData(nameof(Stylesheets))]
    public void RemovingTheOutline_AlwaysComesWithAFocusVisibleStyle(string relativePath)
    {
        var lines = File.ReadAllLines(Path.Combine(RepositoryLayout.Root, relativePath));
        var text = string.Join('\n', lines);
        var suppressions = new List<string>();

        for (var index = 0; index < lines.Length; index++)
        {
            if (!Regex.IsMatch(lines[index], @"outline:\s*(none|0)"))
                continue;

            // A selector scoped to tabindex="-1" is a programmatic focus target:
            // it is not in the tab order, so there is no keyboard ring to remove.
            var selector = lines.Take(index).LastOrDefault(line => line.Contains('{')) ?? string.Empty;
            if (selector.Contains("tabindex=\"-1\"", StringComparison.Ordinal))
                continue;

            suppressions.Add($"{relativePath}:{index + 1}: {selector.Trim()}");
        }

        if (suppressions.Count == 0)
            return;

        Assert.True(text.Contains(":focus-visible", StringComparison.Ordinal),
            $"Focus outline removed without a :focus-visible replacement:{Environment.NewLine}"
            + string.Join(Environment.NewLine, suppressions));
    }

    [Fact]
    public void Foundations_HonourReducedMotionAndForcedColors()
    {
        var authored = string.Concat(RepositoryLayout.Files(RepositoryLayout.Styles, "*.css")
            .Select(File.ReadAllText));

        Assert.Contains("prefers-reduced-motion", authored, StringComparison.Ordinal);
        Assert.Contains("forced-colors", authored, StringComparison.Ordinal);
    }

    [Fact]
    public void ColorModel_StaysOnHexAndColorMix()
    {
        var offenders = RepositoryLayout.Files(RepositoryLayout.Src, "*.css")
            .Where(file => File.ReadAllText(file).Contains("oklch(", StringComparison.OrdinalIgnoreCase))
            .Select(RepositoryLayout.Relative)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Sufficit CSS uses hex + color-mix(in srgb, ...), never OKLCH: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Avatar_owns_and_clips_its_image_geometry()
    {
        var stylesheet = File.ReadAllText(Path.Combine(
            RepositoryLayout.Styles, "sui-components.css"));
        var compact = Regex.Replace(stylesheet, @"\s+", string.Empty);

        Assert.Contains(".sui-avatar{", compact, StringComparison.Ordinal);
        Assert.Contains("overflow:hidden", compact, StringComparison.Ordinal);
        Assert.Contains("object-fit:cover", compact, StringComparison.Ordinal);
    }

    private static IEnumerable<string> ReadDeclarationLines(string relativePath)
        => File.ReadAllLines(Path.Combine(RepositoryLayout.Root, relativePath))
            .Where(line => !line.TrimStart().StartsWith("/*", StringComparison.Ordinal)
                && !line.TrimStart().StartsWith("*", StringComparison.Ordinal))
            .Where(line => line.Contains('{') || line.TrimEnd().EndsWith(",", StringComparison.Ordinal));
}
