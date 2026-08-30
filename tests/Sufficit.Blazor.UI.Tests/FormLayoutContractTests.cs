using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class FormLayoutContractTests
{
    [Fact]
    public void ChoiceCard_OnlyReservesTracksForRenderedContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIChoiceCard<string>>(parameters => parameters
            .Add(component => component.Value, "pix")
            .Add(component => component.Title, "PIX")
            .Add(component => component.Description, "Confirmação imediata")
            .Add(component => component.ShowSelectionIndicator, true));

        var root = cut.Find(".sui-choice-card");
        Assert.DoesNotContain("sui-choice-card--has-leading", root.ClassList);
        Assert.Contains("sui-choice-card--has-description", root.ClassList);
        Assert.Contains("sui-choice-card--has-trailing", root.ClassList);
    }

    [Fact]
    public void ChoiceCard_DoesNotReserveAnEmptyTrailingTrack()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIChoiceCard<string>>(parameters => parameters
            .Add(component => component.Value, "manual")
            .Add(component => component.Title, "Processamento manual")
            .Add(component => component.ShowSelectionIndicator, false));

        var root = cut.Find(".sui-choice-card");
        Assert.DoesNotContain("sui-choice-card--has-leading", root.ClassList);
        Assert.DoesNotContain("sui-choice-card--has-description", root.ClassList);
        Assert.DoesNotContain("sui-choice-card--has-trailing", root.ClassList);
    }

    [Fact]
    public void ChoiceCard_DistinguishesCustomTrailingContentFromSelectionIndicator()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIChoiceCard<string>>(parameters => parameters
            .Add(component => component.Value, "marketplace")
            .Add(component => component.Title, "Usar uma oferta pronta")
            .Add(component => component.TrailingContent,
                (RenderFragment)(builder => builder.AddContent(0, "Nenhuma oferta compatível agora"))));

        var root = cut.Find(".sui-choice-card");
        Assert.Contains("sui-choice-card--has-trailing", root.ClassList);
        Assert.Contains("sui-choice-card--has-custom-trailing", root.ClassList);
        Assert.Contains(
            "--_choice-trailing-track:minmax(min-content,15rem)",
            root.GetAttribute("style"));
        Assert.Contains(
            "sui-choice-card__trailing--custom",
            cut.Find(".sui-choice-card__trailing").ClassList);
    }

    [Fact]
    public void FormGrid_EmitsAlignmentContractAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFormGrid>(parameters => parameters
            .Add(component => component.Columns, 3)
            .Add(component => component.Spacing, 5)
            .Add(component => component.LabelLines, 2)
            .Add(component => component.Class, "consumer-grid")
            .Add(component => component.Style, "--consumer-probe:1;")
            .AddUnmatched("aria-label", "Configuração regional")
            .AddChildContent((RenderFragment)(builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "data-sui-align-field", true);
                builder.AddContent(2, "Campo");
                builder.CloseElement();
            })));

        var root = cut.Find(".sui-form-grid");
        Assert.True(root.HasAttribute("data-sui-align-row"));
        Assert.Equal("Configuração regional", root.GetAttribute("aria-label"));
        Assert.Contains("sui-form-grid--stack-mobile", root.ClassList);
        Assert.Contains("consumer-grid", root.ClassList);
        Assert.Contains("--sui-form-grid-columns:3", root.GetAttribute("style"));
        Assert.Contains("--sui-form-grid-gap:var(--sui-space-5)", root.GetAttribute("style"));
        Assert.Contains("--sui-form-grid-label-lines:2", root.GetAttribute("style"));
        Assert.Contains("--consumer-probe:1", root.GetAttribute("style"));
        Assert.Single(root.Children);
    }

    [Fact]
    public void FormGrid_ClampsUnsafeLayoutValues()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFormGrid>(parameters => parameters
            .Add(component => component.Columns, 99)
            .Add(component => component.Spacing, -4)
            .Add(component => component.LabelLines, 0)
            .Add(component => component.StackOnMobile, false));

        var root = cut.Find(".sui-form-grid");
        Assert.DoesNotContain("sui-form-grid--stack-mobile", root.ClassList);
        Assert.Contains("--sui-form-grid-columns:4", root.GetAttribute("style"));
        Assert.Contains("--sui-form-grid-gap:var(--sui-space-0)", root.GetAttribute("style"));
        Assert.Contains("--sui-form-grid-label-lines:1", root.GetAttribute("style"));
    }
}
