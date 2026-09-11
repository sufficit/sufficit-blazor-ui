using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for <see cref="SUINavGroup"/>: the standard
/// collapsible group (a labelled nav with a disclosure button) and the rail
/// flyout mode (icon trigger + menu popover) it switches to under the
/// SufficitRailMode cascade.
/// </summary>
public sealed class RenderContractNavigationTests
{
    [Fact]
    public void NavGroup_RendersLabelledNavWithDisclosureButtonAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUINavGroup>(parameters => parameters
            .Add(component => component.Title, "Telefonia")
            .Add(component => component.SubTitle, "Ramais e troncos")
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "group")
            .AddChildContent("<a class=\"sui-nav-link\" href=\"/ramais\">Ramais</a>"));

        var root = cut.Find("nav.sui-nav-group");
        Assert.Equal("Telefonia", root.GetAttribute("aria-label"));
        Assert.Equal("group", root.GetAttribute("data-test"));
        Assert.True(root.ClassList.Contains("sui-nav-group--root"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.False(root.ClassList.Contains("is-expanded"));

        var toggle = cut.Find("button.sui-nav-group__toggle");
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));
        Assert.Equal("Telefonia", cut.Find(".sui-nav-link__title").TextContent);
        Assert.Equal("Ramais e troncos", cut.Find(".sui-nav-link__subtitle").TextContent);

        var collapse = cut.Find(".sui-collapse");
        Assert.Equal(toggle.GetAttribute("aria-controls"), collapse.Id);
        Assert.Equal("true", collapse.GetAttribute("aria-hidden"));
        Assert.Equal("Ramais", collapse.QuerySelector(".sui-nav--nested a")!.TextContent);
    }

    [Fact]
    public void NavGroup_ToggleExpandsAndRevealsTheCollapse()
    {
        using var context = new BunitContext();
        var expanded = new List<bool>();
        var cut = context.Render<SUINavGroup>(parameters => parameters
            .Add(component => component.Title, "Relatórios")
            .Add(component => component.ExpandedChanged, value => expanded.Add(value)));

        cut.Find("button.sui-nav-group__toggle").Click();

        Assert.Equal([true], expanded);
        Assert.Equal("true", cut.Find("button.sui-nav-group__toggle").GetAttribute("aria-expanded"));
        Assert.True(cut.Find(".sui-collapse").ClassList.Contains("is-expanded"));
        Assert.Equal("false", cut.Find(".sui-collapse").GetAttribute("aria-hidden"));
        Assert.True(cut.Find("nav").ClassList.Contains("is-expanded"));
    }

    [Fact]
    public void NavGroup_DisabledPropagatesToTheButtonAndAriaDisabled()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUINavGroup>(parameters => parameters
            .Add(component => component.Title, "Financeiro")
            .Add(component => component.Disabled, true)
            .Add(component => component.HideExpandIcon, true));

        Assert.True(cut.Find("nav").ClassList.Contains("sui-nav-group--disabled"));
        // Blazor renders a true bool attribute as present-and-empty, never as "True".
        Assert.True(cut.Find("nav").HasAttribute("aria-disabled"));
        Assert.True(cut.Find("button").HasAttribute("disabled"));
        Assert.Equal("-1", cut.Find("button").GetAttribute("tabindex"));
        Assert.Empty(cut.FindAll(".sui-nav-link__expand"));
    }

    [Fact]
    public void NavGroup_RailModeRendersMenuTriggerAndPopover()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUINavGroup>(parameters => parameters
            .AddCascadingValue("SufficitRailMode", true)
            .Add(component => component.Title, "Telefonia")
            .AddChildContent("<a href=\"/ramais\">Ramais</a>"));

        var root = cut.Find(".sui-nav-group.sui-rail-group");
        Assert.True(root.ClassList.Contains("sui-nav-group--root"));
        Assert.Empty(cut.FindAll("nav"));

        var trigger = cut.Find("button.sui-rail-trigger");
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("Telefonia", trigger.GetAttribute("aria-label"));

        var flyout = cut.Find(".sui-rail-flyout");
        Assert.Equal("menu", flyout.GetAttribute("role"));
        Assert.Equal("Telefonia", flyout.GetAttribute("aria-label"));
        Assert.True(flyout.HasAttribute("hidden"));
        Assert.Equal("Telefonia", cut.Find(".sui-rail-flyout-title").TextContent);

        trigger.Click();

        Assert.Equal("true", cut.Find("button.sui-rail-trigger").GetAttribute("aria-expanded"));
        Assert.False(cut.Find(".sui-rail-flyout").HasAttribute("hidden"));
    }
}
