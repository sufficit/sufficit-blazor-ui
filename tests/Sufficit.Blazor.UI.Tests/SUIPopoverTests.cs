using Bunit;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SUIPopoverTests
{
    [Fact]
    public void Renders_AnchorContent_AndHiddenPanel()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIPopover.razor.js").Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIPopover>(parameters => parameters
            .Add(p => p.ChildContent, "<b>gauge</b>")
            .Add(p => p.Content, "<div class=\"meter\">42%</div>"));

        var anchor = cut.Find(".sui-popover-anchor");
        Assert.Contains("gauge", anchor.InnerHtml);
        Assert.Contains("sui-popover-anchor", anchor.ClassList);

        var panel = cut.Find("[role='tooltip']");
        Assert.Contains("sui-popover", panel.ClassList);
        Assert.DoesNotContain("sui-popover--open", panel.ClassList);
        Assert.Equal("true", panel.GetAttribute("aria-hidden"));
        Assert.Contains("meter", panel.InnerHtml);
        Assert.Equal(anchor.GetAttribute("aria-describedby"), panel.Id);
    }

    [Fact]
    public void Placement_AndMaxWidth_FlowOntoThePanel()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIPopover.razor.js").Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIPopover>(parameters => parameters
            .Add(p => p.ChildContent, "anchor")
            .Add(p => p.Content, "content")
            .Add(p => p.Placement, SUITooltipPlacement.Left)
            .Add(p => p.MaxWidth, 420));

        var panel = cut.Find("[role='tooltip']");
        Assert.Equal("left", panel.GetAttribute("data-sui-popover-placement"));
        Assert.Contains("max-width:420px", panel.GetAttribute("style"));
    }

    [Fact]
    public void Disabled_DropsThePanel_AndTheDescribedBy()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIPopover.razor.js").Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIPopover>(parameters => parameters
            .Add(p => p.ChildContent, "anchor")
            .Add(p => p.Content, "content")
            .Add(p => p.Disabled, true));

        Assert.Null(cut.Find(".sui-popover-anchor").GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("[role='tooltip']"));
    }

    [Fact]
    public void PanelClass_IsAppliedToTheFloatingSurface()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIPopover.razor.js").Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIPopover>(parameters => parameters
            .Add(p => p.ChildContent, "anchor")
            .Add(p => p.Content, "content")
            .Add(p => p.PanelClass, "sf-meter-stack"));

        Assert.Contains("sf-meter-stack", cut.Find("[role='tooltip']").ClassList);
    }
}
