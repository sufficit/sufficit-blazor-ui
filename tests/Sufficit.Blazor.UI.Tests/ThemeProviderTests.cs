using Bunit;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ThemeProviderTests
{
    [Fact]
    public void DarkTheme_EmitsGlobalSyntacticallyScopedTokens()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIThemeProvider>(parameters => parameters
            .Add(component => component.Theme, TestTheme.Dark)
            .AddChildContent("conteúdo"));

        var css = cut.Find("style").TextContent;

        Assert.StartsWith(":root{", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary:#fb923c;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary-action:#fb923c;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary-action-contrast:#111827;", css, StringComparison.Ordinal);
        Assert.Contains("color-scheme:dark;}", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitPrimaryAction_SeparatesFilledControlsFromAccentTreatments()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIThemeProvider>(parameters => parameters
            .Add(component => component.Theme, ExplicitActionTheme.Instance));

        var css = cut.Find("style").TextContent;

        Assert.Contains("--sui-color-primary:#fb923c;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary-contrast:#111827;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary-action:#b7440e;", css, StringComparison.Ordinal);
        Assert.Contains("--sui-color-primary-action-contrast:#fff7ed;", css, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeParameterChange_UpdatesCascadeAndCssTokens()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIThemeProvider>(parameters => parameters
            .Add(component => component.Theme, TestTheme.Light));

        cut.Render(parameters => parameters
            .Add(component => component.Theme, TestTheme.Dark));

        var css = cut.Find("style").TextContent;
        Assert.Contains("--sui-color-primary:#fb923c;", css, StringComparison.Ordinal);
        Assert.Equal("dark", cut.Find(".sui-root").GetAttribute("data-sui-theme"));
    }

    private sealed class TestTheme(bool dark) : ISUITheme
    {
        public static TestTheme Light { get; } = new(false);
        public static TestTheme Dark { get; } = new(true);

        public SUIPalette Palette { get; } = dark
            ? SUIPalette.Default with { Primary = "#fb923c", PrimaryContrast = "#111827" }
            : SUIPalette.Default;
        public SUITypography Typography { get; } = SUITypography.Default;
        public Sufficit.Blazor.UI.Themes.SUILayout Layout { get; }
            = Sufficit.Blazor.UI.Themes.SUILayout.Default;
        public bool IsDark { get; } = dark;
    }

    private sealed class ExplicitActionTheme : ISUITheme
    {
        public static ExplicitActionTheme Instance { get; } = new();

        public SUIPalette Palette { get; } = SUIPalette.Default with
        {
            Primary = "#fb923c",
            PrimaryContrast = "#111827",
            PrimaryAction = "#b7440e",
            PrimaryActionContrast = "#fff7ed",
        };
        public SUITypography Typography { get; } = SUITypography.Default;
        public Sufficit.Blazor.UI.Themes.SUILayout Layout { get; }
            = Sufficit.Blazor.UI.Themes.SUILayout.Default;
        public bool IsDark => true;
    }
}
