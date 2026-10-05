using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Contracts introduced by the 2026-10-04 design consolidation: focus, motion,
/// nav sizing and control shape must come from the shared tokens, component
/// markup must not ship static inline styles (strict CSP breaks them), and
/// internal markup must use the canonical size class names instead of the
/// legacy --sm/--md/--lg aliases kept for consumers.
/// </summary>
public sealed class DesignTokenContractTests
{
    [Fact]
    public void FocusRing_UsesSharedTokensAcrossAuthoredStyles()
    {
        var foundations = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));
        Assert.Contains("--sui-focus-ring: 2px solid var(--sui-focus-color, var(--sui-color-primary));", foundations);
        Assert.Contains("--sui-focus-offset: 2px;", foundations);

        var styles = RepositoryLayout.Files(RepositoryLayout.Src, "*.css")
            .Where(path => !RepositoryLayout.Relative(path).StartsWith("src/wwwroot/", StringComparison.Ordinal))
            .ToArray();
        Assert.Contains(styles, path => File.ReadAllText(path).Contains("outline: var(--sui-focus-ring)", StringComparison.Ordinal)
            && !path.EndsWith("sui-foundations.css", StringComparison.Ordinal));

        var offenders = styles.SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => (path, line, index)))
            .Where(entry => Regex.IsMatch(entry.line, @"outline-offset:\s*(?:1px|3px)\s*;"))
            .Select(entry => $"{RepositoryLayout.Relative(entry.path)}:{entry.index + 1}")
            .ToArray();
        Assert.Empty(offenders);
    }

    [Fact]
    public void ReducedMotion_IsCentralizedInFoundations()
    {
        var offenders = RepositoryLayout.Files(RepositoryLayout.Src, "*.css")
            .Where(path => !RepositoryLayout.Relative(path).StartsWith("src/wwwroot/", StringComparison.Ordinal))
            .Where(path => !path.EndsWith("sui-foundations.css", StringComparison.Ordinal))
            .Where(path => File.ReadAllText(path).Contains("prefers-reduced-motion", StringComparison.Ordinal))
            .Select(RepositoryLayout.Relative)
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void MotionDurations_Easings_AndControlShape_ComeFromTokens()
    {
        var foundations = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));
        Assert.Contains("--sui-dur-fast: 120ms;", foundations);
        Assert.Contains("--sui-dur:      160ms;", foundations);
        Assert.Contains("--sui-ease:", foundations);
        Assert.Contains("--sui-ease-enter:", foundations);
        Assert.Contains("--sui-radius-control: calc(var(--sui-radius) * 1.25);", foundations);
        Assert.Contains("--sui-nav-item-h: 48px;", foundations);
        Assert.Contains("--sui-nav-item-h-nested: 40px;", foundations);

        var buttons = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-buttons.css"));
        Assert.Contains("border-radius: var(--sui-radius-control);", buttons);

        // Loops (spinners, shimmer) and the one documented signature movement
        // keep literal periods: they are progress or identity, not transitions.
        var allowed = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["src/styles/sui-foundations.css"] = ["2.4s", ".01ms"], // reduced-motion policy
            ["src/styles/sui-shared-skeleton.css"] = ["1.4s"],
            ["src/styles/sui-buttons.css"] = [".7s"],
            ["src/styles/sui-progress-circular.css"] = ["1.4s"],
            ["src/Components/Navigation/SUISlidingTabs.razor.css"] = ["380ms", "320ms"],
        };

        var offenders = new List<string>();
        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.css"))
        {
            var relative = RepositoryLayout.Relative(file);
            if (relative.StartsWith("src/wwwroot/", StringComparison.Ordinal))
                continue; // generated bundle

            allowed.TryGetValue(relative, out var permitted);
            foreach (var line in File.ReadAllLines(file))
            {
                var trimmed = line.Trim();
                if (!Regex.IsMatch(trimmed, @"^(transition|animation)(-[a-z]+)?:", RegexOptions.IgnoreCase))
                    continue;

                foreach (Match match in Regex.Matches(trimmed, @"(?<![\w-])(\d*\.?\d+)(ms|s)\b"))
                {
                    if (match.Value is "0s")
                        continue; // no-op visibility delay

                    if (permitted is not null && permitted.Contains(match.Value))
                        continue;

                    offenders.Add($"{relative}: {trimmed}");
                    break;
                }
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void DensityToken_ScalesControlSizing_InFoundationsAndTheme()
    {
        var foundations = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));

        // Density is a cascading token: one value tightens every control size
        // that derives from it, so an app (or subtree) gets coherent density
        // without per-component parameters.
        Assert.Contains("--sui-density-scale: 1;", foundations);
        Assert.Contains("--sui-control-h-md: calc(36px * var(--sui-density-scale));", foundations);
        Assert.Contains("--sui-control-px-md: calc(14px * var(--sui-density-scale));", foundations);

        // Navigation hit areas are accessibility floors, not density: they stay
        // literal even when everything else scales.
        Assert.Contains("--sui-nav-item-h: 48px;", foundations);
        Assert.DoesNotContain("--sui-nav-item-h: calc(", foundations);
        Assert.DoesNotContain("--sui-nav-item-h-nested: calc(", foundations);

        // The theme model must be able to publish the scale like any other
        // token, and its control-size defaults must derive from it too.
        Assert.Equal("1", Sufficit.Blazor.UI.Themes.SUILayout.Default.DensityScale);
        Assert.Equal("calc(36px * var(--sui-density-scale))",
            Sufficit.Blazor.UI.Themes.SUILayout.Default.ControlHMd);

        var css = Sufficit.Blazor.UI.Themes.SUIThemeCssWriter.Write(Sufficit.Blazor.UI.Themes.SUITheme.Light);
        Assert.Contains("--sui-density-scale:1;", css, StringComparison.Ordinal);
    }

    [Fact]
    public void StackingOrder_AndFieldFontSize_AreThemeableTokens()
    {
        // F4: z-index and --sui-fs-field existed only in the authored
        // foundations; consumers could not theme them. Now the model carries
        // them and the writer publishes them alongside every other token.
        var css = Sufficit.Blazor.UI.Themes.SUIThemeCssWriter.Write(Sufficit.Blazor.UI.Themes.SUITheme.Light);
        Assert.Contains("--sui-z-dropdown:1000;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-sticky:1100;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-drawer:1200;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-backdrop:1300;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-modal:1400;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-toast:1500;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-z-tooltip:1600;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-fs-field:.8125rem;", css, StringComparison.Ordinal);

        var foundations = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-foundations.css"));
        Assert.Contains("--sui-fs-field: .8125rem;", foundations);
        Assert.Contains("--sui-z-dropdown: 1000;", foundations);
    }

    [Fact]
    public void Components_DoNotShipStaticInlineStyles()
    {
        // Static inline styles break under a strict CSP (style-src without
        // 'unsafe-inline') and dodge every contract in this suite. Dynamic
        // Razor expressions (style="@...") stay allowed: they carry per-instance
        // values that no class can express.
        var offenders = new List<string>();
        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.razor"))
        {
            var relative = RepositoryLayout.Relative(file);
            foreach (Match match in Regex.Matches(File.ReadAllText(file), @"style\s*=\s*""(?<value>[^""]*)"""))
            {
                var value = match.Groups["value"].Value;
                if (value.Contains('@'))
                    continue;
                if (value.Length == 0)
                    continue; // style="" is a no-op placeholder

                offenders.Add($"{relative}: style=\"{value}\"");
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void InternalMarkup_UsesCanonicalSizeClasses_NotLegacyAliases()
    {
        // --small/--medium/--large match SUISize; the --sm/--md/--lg aliases
        // are legacy public surface kept for consumers only. Internal markup
        // must not grow new usages of the short forms.
        var pattern = @"sui-(?:icon|btn)--(?:sm|md|lg)\b";
        var offenders = RepositoryLayout.Files(RepositoryLayout.Src, "*.razor")
            .Concat(RepositoryLayout.Files(RepositoryLayout.Src, "*.cs"))
            .Where(path => !RepositoryLayout.Relative(path).StartsWith("src/obj/", StringComparison.Ordinal))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => (path, line, index)))
            .Where(entry => Regex.IsMatch(entry.line, pattern))
            .Select(entry => $"{RepositoryLayout.Relative(entry.path)}:{entry.index + 1}: {entry.line.Trim()}")
            .ToArray();

        Assert.Empty(offenders);
    }
}
