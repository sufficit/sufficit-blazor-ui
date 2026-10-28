using Bunit;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// data-sui-style is how SUI ships computed values under a CSP that ignores
/// style attributes. The builder is the only writer, so its allow-list is the
/// injection boundary on the server side.
/// </summary>
public sealed class StyleBuilderTests
{
    [Theory]
    [InlineData("2fr 1fr")]
    [InlineData("minmax(0, 1fr) 12rem")]
    [InlineData("repeat(3, minmax(10rem, 1fr))")]
    [InlineData("[full-start] calc(100% - 2 * var(--sui-space-4)) [full-end]")]
    [InlineData("fit-content(20ch) 1fr")]
    [InlineData("0.68")]
    public void IsSafeValue_AcceptsGeometry(string value)
        => Assert.True(SUIStyleBuilder.IsSafeValue(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1fr; color: red")]
    [InlineData("1fr} body{display:none")]
    [InlineData("url(https://evil.example/x.png)")]
    [InlineData("expression(alert(1))")]
    [InlineData("1fr /* x */")]
    [InlineData("\"quoted\"")]
    [InlineData("1fr\n2fr")]
    [InlineData("min(1fr")]
    [InlineData("1fr !important")]
    public void IsSafeValue_RejectsInjection(string? value)
        => Assert.False(SUIStyleBuilder.IsSafeValue(value));

    [Theory]
    [InlineData("color", false)]
    [InlineData("--other-token", false)]
    [InlineData("--sui-", false)]
    [InlineData("--sui-Width", false)]
    [InlineData("--sui-form-grid-template", true)]
    public void IsSafeProperty_OnlyAcceptsSuiCustomProperties(string property, bool expected)
        => Assert.Equal(expected, SUIStyleBuilder.IsSafeProperty(property));

    [Fact]
    public void Build_JoinsValidDeclarationsAndSkipsTheRest()
    {
        var value = SUIStyleBuilder.Default()
            .Add("--sui-a", "1fr 2fr")
            .Add("--sui-b", "1fr; color:red")
            .Add("color", "red")
            .Add("--sui-c", "12rem", when: false)
            .Add("--sui-d", "12rem")
            .AddUrl("--sui-stat-image", "https://cdn.example/bg.webp")
            .AddUrl("--sui-stat-image", "javascript:alert(1)")
            .AddUrl("--sui-width", "/bg.webp")
            .Build();

        Assert.Equal("--sui-a:1fr 2fr;--sui-d:12rem;--sui-stat-image:url(\"https://cdn.example/bg.webp\")", value);
        Assert.Null(SUIStyleBuilder.Default().Build());
    }

    [Fact]
    public void Text_ColorBecomesAClassInsteadOfAStyle()
    {
        using var context = new BunitContext();
        var tone = context.Render<SUIText>(parameters => parameters
            .Add(component => component.Color, SUIColor.Error)
            .AddChildContent("Falhou"));
        var muted = context.Render<SUIText>(parameters => parameters
            .Add(component => component.Color, SUIColor.Default)
            .AddChildContent("Detalhe"));

        Assert.Contains("sui-color-error", tone.Find(".sui-text").ClassList);
        Assert.False(tone.Find(".sui-text").HasAttribute("style"));
        Assert.Contains("sui-text--secondary", muted.Find(".sui-text").ClassList);
    }

    [Fact]
    public void Stylesheet_KeepsTextTonesAboveTheTextBaseColor()
    {
        var css = File.ReadAllText(Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css"));
        Assert.Contains(".sui-text.sui-color-error{color:var(--sui-color-error)}", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Initializer_ShipsAndOnlyHonorsSuiCustomProperties()
    {
        var script = File.ReadAllText(Path.Combine(RepositoryLayout.WebRoot, "Sufficit.Blazor.UI.lib.module.js"));
        Assert.Contains("export function beforeWebStart", script, StringComparison.Ordinal);
        Assert.Contains("export function afterStarted", script, StringComparison.Ordinal);
        Assert.Contains("const NAME = /^--sui-[a-z0-9-]+$/;", script, StringComparison.Ordinal);
        Assert.Contains("setProperty", script, StringComparison.Ordinal);
        Assert.DoesNotContain("setAttribute('style'", script, StringComparison.Ordinal);
    }
}
