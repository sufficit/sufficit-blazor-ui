using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for <see cref="SUISwitchButton"/> (a full-width
/// toggle button whose variant mirrors the checked state) and
/// <see cref="SUILink"/> (an anchor forwarding unmatched attributes).
/// </summary>
public sealed class RenderContractFormsActionsTests
{
    [Fact]
    public void SwitchButton_UncheckedRendersOutlinedPrimaryButton()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISwitchButton>(parameters => parameters
            .AddChildContent("Ativo"));

        var button = cut.Find("button");
        Assert.True(button.ClassList.Contains("sui-btn--outlined"));
        Assert.True(button.ClassList.Contains("sui-btn--color-primary"));
        Assert.True(button.ClassList.Contains("sui-btn--full-width"));
        Assert.Contains("Ativo", button.TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".sui-field__error"));
    }

    [Fact]
    public void SwitchButton_CheckedRendersFilledAndClickTogglesBackThroughTheCallback()
    {
        using var context = new BunitContext();
        var reported = new List<bool>();
        var cut = context.Render<SUISwitchButton>(parameters => parameters
            .Add(component => component.Checked, true)
            .Add(component => component.CheckedChanged, EventCallback.Factory.Create<bool>(this, reported.Add))
            .AddChildContent("Ativo"));

        Assert.True(cut.Find("button").ClassList.Contains("sui-btn--filled"));

        cut.Find("button").Click();

        Assert.Equal([false], reported);
        Assert.True(cut.Find("button").ClassList.Contains("sui-btn--outlined"));
    }

    [Fact]
    public void Link_RendersAnchorWithHrefTargetRelAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUILink>(parameters => parameters
            .Add(component => component.Href, "https://sufficit.com.br")
            .Add(component => component.Target, "_blank")
            .Add(component => component.Rel, "noopener")
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "link")
            .AddUnmatched("aria-label", "Site da Sufficit")
            .AddChildContent("Sufficit"));

        var root = cut.Find("a.sui-link");
        Assert.Equal("https://sufficit.com.br", root.GetAttribute("href"));
        Assert.Equal("_blank", root.GetAttribute("target"));
        Assert.Equal("noopener", root.GetAttribute("rel"));
        Assert.Equal("link", root.GetAttribute("data-test"));
        Assert.Equal("Site da Sufficit", root.GetAttribute("aria-label"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("Sufficit", root.TextContent.Trim());
    }

    [Fact]
    public void Link_OmitsOptionalAttributesWhenUnset()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUILink>(parameters => parameters
            .Add(component => component.Href, "/conta"));

        var root = cut.Find("a.sui-link");
        Assert.Equal("/conta", root.GetAttribute("href"));
        Assert.False(root.HasAttribute("target"));
        Assert.False(root.HasAttribute("rel"));
    }
}
