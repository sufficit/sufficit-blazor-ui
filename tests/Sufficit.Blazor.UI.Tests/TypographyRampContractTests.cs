using System.Text.RegularExpressions;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// F3 of the 2026-10-04 evaluation: <see cref="SUITypo"/> carries two families
/// by decision (docs/DESIGN-TYPOGRAPHY-RAMP.md). These contracts keep that
/// decision from dissolving silently: every member renders, every semantic role
/// is themeable end to end, nothing turned obsolete before the next major, and
/// the document keeps naming the roles it governs.
/// </summary>
public sealed class TypographyRampContractTests
{
    private static readonly string[] SemanticRoles = ["display", "headline", "title", "body", "label", "mono"];

    private static string Foundations => File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));

    private static string TextStyles => File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-shared-text.css"));

    private static string Capitalize(string role) => char.ToUpperInvariant(role[0]) + role[1..];

    [Fact]
    public void EveryTypoMember_HasARenderedTextRule()
    {
        // SUIText renders class sui-text--{member}; a member without its rule
        // would silently fall back to the bare .sui-text style.
        var css = TextStyles;
        var missing = Enum.GetNames<SUITypo>()
            .Where(name => !Regex.IsMatch(css, $@"\.sui-text--{Regex.Escape(name)}\b"))
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void SemanticRoles_AreThemeableEndToEnd()
    {
        var writer = File.ReadAllText(Path.Combine(RepositoryLayout.Src, "Themes", "SUIThemeCssWriter.cs"));
        var typography = typeof(SUITypography);
        var offenders = new List<string>();

        foreach (var role in SemanticRoles)
        {
            Assert.Contains(role, Enum.GetNames<SUITypo>());

            foreach (var (token, property) in new[]
                     {
                         ($"--sui-fs-{role}", $"Fs{Capitalize(role)}"),
                         ($"--sui-lh-{role}", $"LineHeight{Capitalize(role)}"),
                     })
            {
                if (!Foundations.Contains($"{token}:", StringComparison.Ordinal))
                    offenders.Add($"{token}: missing default in sui-foundations.css");
                if (typography.GetProperty(property) is null)
                    offenders.Add($"{token}: SUITypography.{property} does not exist");
                if (!writer.Contains($"\"{token}\"", StringComparison.Ordinal))
                    offenders.Add($"{token}: SUIThemeCssWriter does not publish it");
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void NoTypoMember_IsObsoleteBeforeTheNextMajor()
    {
        // The Material family stays supported until the breaking window; marking
        // a member [Obsolete] earlier turns the decision into enforcement.
        var obsolete = typeof(SUITypo).GetFields()
            .Where(field => field.IsStatic)
            .Where(field => field.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Length > 0)
            .Select(field => field.Name)
            .ToArray();

        Assert.Empty(obsolete);
    }

    [Fact]
    public void SemanticRoles_DoNotRenderAHeadingUnderAuto()
    {
        // Documented in the decision: only h1-h6 map to native headings, so a
        // semantic role used as a page title must set Tag explicitly.
        var source = File.ReadAllText(Path.Combine(RepositoryLayout.Src, "Components", "DataDisplay", "SUIText.razor"));
        var autoMap = Regex.Match(source, @"SUITextTag\.Auto\s*=>\s*Typo\s+switch\s*\{(?<body>.*?)\},\s*_\s*=>", RegexOptions.Singleline);

        Assert.True(autoMap.Success, "SUIText no longer maps Auto through a Typo switch; update the typography doc.");
        foreach (var role in SemanticRoles)
            Assert.DoesNotContain($"SUITypo.{role} ", autoMap.Groups["body"].Value);
    }

    [Fact]
    public void DecisionDocument_NamesEverySemanticRole()
    {
        var document = File.ReadAllText(Path.Combine(RepositoryLayout.Root, "docs", "DESIGN-TYPOGRAPHY-RAMP.md"));

        var missing = SemanticRoles
            .Where(role => !document.Contains($"`{role}`", StringComparison.Ordinal))
            .ToArray();

        Assert.Empty(missing);
    }
}
