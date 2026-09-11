using Bunit;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for the overlay family: the tooltip anchor (a
/// span carrying data-sui-tooltip-* for the shared browser helper) and the
/// decision dialog body (message + three outcomes routed to the dialog
/// reference cascade).
/// </summary>
public sealed class RenderContractOverlaysTests
{
    [Fact]
    public void Tooltip_RendersAnchorWithTextPlacementAndForwardsAttributes()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUITooltip>(parameters => parameters
            .Add(component => component.Text, "  Copiar  ")
            .Add(component => component.Placement, SUITooltipPlacement.Bottom)
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "tooltip")
            .AddChildContent("<button>Copiar</button>"));

        var root = cut.Find("span.sui-tooltip-anchor");
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("tooltip", root.GetAttribute("data-test"));
        Assert.Equal("Copiar", root.GetAttribute("data-sui-tooltip"));
        Assert.Equal("bottom", root.GetAttribute("data-sui-tooltip-placement"));
        Assert.Equal("true", root.GetAttribute("data-sui-tooltip-arrow"));
        Assert.Equal("95%", root.GetAttribute("data-sui-tooltip-opacity"));
        Assert.Equal("Copiar", root.QuerySelector("button")!.TextContent);
    }

    [Fact]
    public void Tooltip_DisabledDropsTheTextSoTheHelperStaysSilent()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUITooltip>(parameters => parameters
            .Add(component => component.Text, "Copiar")
            .Add(component => component.Disabled, true)
            .Add(component => component.Offset, 500)
            .AddChildContent("alvo"));

        var root = cut.Find(".sui-tooltip-anchor");
        Assert.False(root.HasAttribute("data-sui-tooltip"));
        Assert.Equal("80", root.GetAttribute("data-sui-tooltip-offset"));
    }

    [Fact]
    public void DecisionDialog_RendersMessageDescriptionAndThreeActions()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDecisionDialog>(parameters => parameters
            .Add(component => component.Message, "Publicar agora?")
            .Add(component => component.Description, "Isso notifica os clientes.")
            .Add(component => component.PrimaryText, "Publicar")
            .Add(component => component.SecondaryText, "Agendar")
            .Add(component => component.CancelText, "Voltar"));

        Assert.Equal("Publicar agora?", cut.Find(".sui-dialog__body .sui-dialog__message").TextContent);
        Assert.Equal("Isso notifica os clientes.", cut.Find(".sui-dialog__description").TextContent);

        var buttons = cut.FindAll(".sui-dialog__actions button");
        Assert.Equal(3, buttons.Count);
        Assert.Equal("Voltar", buttons[0].TextContent.Trim());
        Assert.Equal("Agendar", buttons[1].TextContent.Trim());
        Assert.Equal("Publicar", buttons[2].TextContent.Trim());
        Assert.True(buttons[0].ClassList.Contains("sui-btn--text"));
        Assert.True(buttons[1].ClassList.Contains("sui-btn--outlined"));
        Assert.True(buttons[2].ClassList.Contains("sui-btn--filled"));
    }

    [Fact]
    public async Task DecisionDialog_CompletesTheCascadedReferenceWithTheChosenOutcome()
    {
        using var context = new BunitContext();
        var primary = new SUIDialogReference();
        var secondary = new SUIDialogReference();
        var cancel = new SUIDialogReference();

        Render(context, primary).FindAll(".sui-dialog__actions button")[2].Click();
        Render(context, secondary).FindAll(".sui-dialog__actions button")[1].Click();
        Render(context, cancel).FindAll(".sui-dialog__actions button")[0].Click();

        Assert.Equal(true, await primary.Result);
        Assert.Equal(false, await secondary.Result);
        Assert.Null(await cancel.Result);
    }

    [Fact]
    public void DecisionDialog_OmitsDescriptionWhenBlank()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDecisionDialog>(parameters => parameters
            .Add(component => component.Message, "Continuar?"));

        Assert.Empty(cut.FindAll(".sui-dialog__description"));
        Assert.Equal("Sim", cut.FindAll(".sui-dialog__actions button")[2].TextContent.Trim());
    }

    private static IRenderedComponent<SUIDecisionDialog> Render(BunitContext context, SUIDialogReference reference)
        => context.Render<SUIDecisionDialog>(parameters => parameters
            .AddCascadingValue(reference)
            .Add(component => component.Message, "Decidir"));
}
