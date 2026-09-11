using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for the layout primitives: root element, base class
/// and the modifier classes derived from their few parameters. None of these
/// capture unmatched attributes, so no forwarding assertion applies.
/// </summary>
public sealed class RenderContractLayoutTests
{
    [Fact]
    public void Grid_RendersRootWithGutterByDefault()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIGrid>(parameters => parameters
            .Add(component => component.Class, "probe")
            .AddChildContent("<span>cell</span>"));

        var root = cut.Find("div.sui-grid");
        Assert.True(root.ClassList.Contains("sui-grid--gutter"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("cell", root.QuerySelector("span")!.TextContent);
    }

    [Fact]
    public void Grid_DropsGutterWhenSpacingIsOff()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIGrid>(parameters => parameters
            .Add(component => component.Spacing, false));

        Assert.False(cut.Find(".sui-grid").ClassList.Contains("sui-grid--gutter"));
    }

    [Fact]
    public void Container_DefaultsToLargeBucket()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIContainer>(parameters => parameters
            .AddChildContent("conteúdo"));

        var root = cut.Find("div.sui-container");
        Assert.True(root.ClassList.Contains("sui-container--lg"));
        Assert.Equal("conteúdo", root.TextContent.Trim());
    }

    [Fact]
    public void Container_MapsMaxWidthToModifierClass()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIContainer>(parameters => parameters
            .Add(component => component.MaxWidth, "SM")
            .Add(component => component.Class, "probe"));

        var root = cut.Find(".sui-container");
        Assert.True(root.ClassList.Contains("sui-container--sm"));
        Assert.True(root.ClassList.Contains("probe"));
    }

    [Fact]
    public void Spacer_IsHiddenFromAssistiveTechnology()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISpacer>();

        var root = cut.Find("div.sui-spacer");
        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.Empty(root.ChildNodes);
    }

    [Fact]
    public void AppBar_RendersHeaderWithDefaultElevation()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIAppBar>(parameters => parameters
            .AddChildContent("<span>brand</span>"));

        var root = cut.Find("header.sui-appbar");
        Assert.True(root.ClassList.Contains("sui-appbar--e1"));
        Assert.False(root.ClassList.Contains("sui-appbar--dense"));
        Assert.Equal("brand", root.QuerySelector("span")!.TextContent);
    }

    [Fact]
    public void AppBar_AppliesDenseColorAndClampedElevation()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIAppBar>(parameters => parameters
            .Add(component => component.Dense, true)
            .Add(component => component.Color, "Primary")
            .Add(component => component.Elevation, 9)
            .Add(component => component.Class, "probe")
            .Add(component => component.Style, "top:0"));

        var root = cut.Find("header.sui-appbar");
        Assert.True(root.ClassList.Contains("sui-appbar--dense"));
        Assert.True(root.ClassList.Contains("sui-appbar--primary"));
        Assert.True(root.ClassList.Contains("sui-appbar--e1"));
        Assert.True(root.ClassList.Contains("probe"));
    }
}
