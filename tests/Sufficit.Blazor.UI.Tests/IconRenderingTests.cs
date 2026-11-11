using System.Text.RegularExpressions;
using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class IconRenderingTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingName_RendersTheFallbackCircle(string? name)
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, name!));

        var svg = rendered.Find("svg");
        var circle = Assert.Single(svg.QuerySelectorAll("circle"));
        Assert.Equal("none", svg.GetAttribute("fill"));
        Assert.Equal("12", circle.GetAttribute("cx"));
        Assert.Equal("12", circle.GetAttribute("cy"));
        Assert.Equal("9", circle.GetAttribute("r"));
    }

    [Fact]
    public void EyeOff_RendersItsDistinctVisibilityGlyph()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, "eye-off"));

        Assert.Contains("M3 3l18 18", rendered.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("cx=\"12\" cy=\"12\" r=\"9\"", rendered.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void LibraryIconConstant_RendersItsMarkupInsteadOfTheFallbackCircle()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, SUIIcons.Devices));

        var pathData = Regex.Match(SUIIcons.Devices, "d=\"([^\"]{8,})").Groups[1].Value;
        Assert.NotEmpty(pathData);
        Assert.Contains(pathData[..8], rendered.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("cx=\"12\" cy=\"12\" r=\"9\"", rendered.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownName_StillFallsBackToTheCircle()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, "storage"));

        Assert.Contains("cx=\"12\" cy=\"12\" r=\"9\"", rendered.Markup, StringComparison.Ordinal);
    }
}
