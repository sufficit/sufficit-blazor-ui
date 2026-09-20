using Bunit;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// The theme provider writes its token block into an inline style element as raw
/// markup, so every palette value is executed as CSS and, if it escapes the
/// element, as HTML. Consumers feed those values from per-tenant configuration,
/// so these are the tests that decide whether a brand colour column is data or a
/// scripting primitive.
/// </summary>
public sealed class ThemeInjectionTests
{
    public static TheoryData<string, string> EscapeAttempts() => new()
    {
        { "closes the style element", "#fff</style><script>alert(1)</script><style>" },
        { "closes only the tag", "#fff</style>" },
        { "opens a tag", "#fff<img src=x onerror=alert(1)>" },
        { "ends the declaration", "#fff;background:red" },
        { "ends the rule", "#fff}body{display:none" },
        { "opens an at-rule", "#fff} @import url('//attacker.test/x.css'); .x{color:red" },
        { "fetches a url", "url('//attacker.test/pixel.png')" },
        { "fetches a url inside a function", "rgb(0,0,0) url(//attacker.test/p.png)" },
        { "uses an image function", "image('//attacker.test/p.png')" },
        { "uses legacy expression", "expression(alert(1))" },
        { "hides behind a comment", "#fff/*;background:red;*/" },
        { "escapes a character", @"#fff\3b background:red" },
        { "smuggles a newline", "#fff\n;background:red" },
        { "smuggles a carriage return", "#fff\r;background:red" },
        { "leaves a parenthesis open", "rgb(0,0,0" },
        // Deliberately not a real family name: the fallback font stack contains
        // "Segoe UI", so such a payload would appear in the output as part of a
        // legitimate value and the assertion below could not tell them apart.
        { "leaves a quote open", "\"Unclosed Family" },
    };

    [Theory]
    [MemberData(nameof(EscapeAttempts))]
    public void HostilePaletteValue_NeverReachesTheRenderedStyle(string scenario, string payload)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIThemeProvider>(parameters => parameters
            .Add(component => component.Theme, HostileTheme.WithPrimary(payload)));

        var style = cut.Find("style");
        var css = style.TextContent;

        Assert.False(SUIThemeCssWriter.IsSafeValue(payload), scenario);
        Assert.DoesNotContain(payload, css, StringComparison.Ordinal);
        // The rejected token falls back to the library default rather than
        // disappearing, so one bad value cannot leave components unstyled.
        Assert.Contains($"--sui-color-primary:{SUIPalette.Default.Primary};", css, StringComparison.Ordinal);
        // The block still consists of exactly one rule: selector, one brace pair,
        // nothing after it. Anything else means a value escaped.
        Assert.StartsWith(SUIThemeCssWriter.TokenSelector + "{", css, StringComparison.Ordinal);
        Assert.EndsWith("}", css, StringComparison.Ordinal);
        Assert.Equal(1, css.Count(character => character == '{'));
        Assert.Equal(1, css.Count(character => character == '}'));
        // And the payload never becomes markup: the element has no children.
        Assert.Empty(style.Children);
    }

    [Theory]
    [MemberData(nameof(EscapeAttempts))]
    public void HostileValue_IsRejectedInEveryTokenCategory(string scenario, string payload)
    {
        // Colours are the obvious vector, but a font stack, a shadow and a
        // transition are written into the same block and were equally unchecked.
        using var context = new BunitContext();
        var cut = context.Render<SUIThemeProvider>(parameters => parameters
            .Add(component => component.Theme, HostileTheme.WithEveryCategory(payload)));

        var css = cut.Find("style").TextContent;

        Assert.DoesNotContain(payload, css, StringComparison.Ordinal);
        Assert.Equal(1, css.Count(character => character == '}'));
        Assert.Contains($"--sui-font:{SUITypography.Default.FontFamily};", css, StringComparison.Ordinal);
        Assert.Contains($"--sui-shadow-1:{SUILayout.Default.Shadow1};", css, StringComparison.Ordinal);
        Assert.Contains($"--sui-transition:{SUILayout.Default.Transition};", css, StringComparison.Ordinal);
        Assert.Contains($"--sui-radius:{SUILayout.Default.Radius};", css, StringComparison.Ordinal);
        Assert.Equal(scenario, scenario);
    }

    [Theory]
    // Everything the shipped themes and the real consumer themes actually use
    // has to keep working; a validator that rejects valid CSS would be replaced
    // by the first person who needs a gradient.
    [InlineData("#fff")]
    [InlineData("#2563eb")]
    [InlineData("#D62027")]
    [InlineData("rgba(15, 23, 42, .45)")]
    [InlineData("rgba(0,0,0,.6)")]
    [InlineData("transparent")]
    [InlineData("currentColor")]
    [InlineData("color-mix(in srgb, var(--sui-color-primary) 14%, transparent)")]
    [InlineData("var(--sui-font)")]
    [InlineData("oklch(62% 0.19 25)")]
    [InlineData("light-dark(#fff, #000)")]
    [InlineData("clamp(1.55rem, 2.2vw, 2.25rem)")]
    [InlineData("calc(100% - 2px)")]
    [InlineData("-.025em")]
    [InlineData("9999px")]
    [InlineData("1.45")]
    [InlineData("160ms cubic-bezier(.4, 0, .2, 1)")]
    [InlineData("0 1px 2px rgba(15,23,42,.06)")]
    [InlineData("0 1px 2px rgba(11,11,12,0.04), 0 1px 1px rgba(11,11,12,0.03)")]
    [InlineData("-apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, Helvetica, Arial, sans-serif")]
    [InlineData("Lato, Inter, system-ui, -apple-system, Segoe UI, sans-serif")]
    [InlineData("ui-monospace, SFMono-Regular, \"SF Mono\", Menlo, monospace")]
    public void LegitimateValues_AreAccepted(string value)
    {
        Assert.True(SUIThemeCssWriter.IsSafeValue(value), value);
    }

    [Fact]
    public void ShippedThemes_PassTheirOwnValidator()
    {
        // If a default ever fails validation the writer would silently fall back
        // to it anyway, so this is the test that keeps the guard honest.
        foreach (var theme in new ISUITheme[] { DefaultSUITheme.Instance, SUITheme.Light, SUITheme.Dark })
        {
            var css = SUIThemeCssWriter.Write(theme);
            Assert.Equal(1, css.Count(character => character == '{'));
            Assert.DoesNotContain("--sui-color-primary:;", css, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyValues_AreRejected(string? value)
    {
        Assert.False(SUIThemeCssWriter.IsSafeValue(value));
    }

    [Fact]
    public void OverlongValue_IsRejected()
    {
        Assert.False(SUIThemeCssWriter.IsSafeValue("#fff" + new string('a', 500)));
    }

    [Fact]
    public void NullTheme_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SUIThemeCssWriter.Write(null!));
    }

    private sealed class HostileTheme : ISUITheme
    {
        public required SUIPalette Palette { get; init; }
        public required SUITypography Typography { get; init; }
        public required SUILayout Layout { get; init; }
        public bool IsDark => false;

        public static HostileTheme WithPrimary(string payload) => new()
        {
            Palette = SUIPalette.Default with { Primary = payload },
            Typography = SUITypography.Default,
            Layout = SUILayout.Default,
        };

        public static HostileTheme WithEveryCategory(string payload) => new()
        {
            Palette = SUIPalette.Default with { Primary = payload, Overlay = payload },
            Typography = SUITypography.Default with { FontFamily = payload, FsBody = payload },
            Layout = SUILayout.Default with { Shadow1 = payload, Transition = payload, Radius = payload },
        };
    }
}
