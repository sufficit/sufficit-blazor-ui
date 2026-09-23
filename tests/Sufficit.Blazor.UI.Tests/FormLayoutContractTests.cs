using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class FormLayoutContractTests
{
    [Fact]
    public void Section_AppliesSafeVerticalRhythmBetweenContentBlocks()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Visibilidade")
            .AddChildContent((RenderFragment)(builder =>
            {
                builder.OpenElement(0, "fieldset");
                builder.AddContent(1, "Opções");
                builder.CloseElement();
                builder.OpenElement(2, "aside");
                builder.AddContent(3, "Orientação");
                builder.CloseElement();
            })));

        var content = cut.Find(".sui-card__content");
        var stack = content.QuerySelector(":scope > .sui-stack");

        Assert.NotNull(stack);
        Assert.Contains("gap: var(--sui-space-4)", stack.GetAttribute("style"));
        Assert.Equal(2, stack.Children.Length);
    }

    [Fact]
    public void Section_ClampsUnsafeContentSpacing()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Visibilidade")
            .Add(component => component.ContentSpacing, 99)
            .AddChildContent("Conteúdo"));

        var stack = cut.Find(".sui-card__content > .sui-stack");

        Assert.Contains("gap: var(--sui-space-6)", stack.GetAttribute("style"));
    }

    [Fact]
    public void Section_LeavesTheDefaultRhythmToTheStylesheet()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Filtros")
            .AddChildContent("Conteúdo"));

        var root = cut.Find(".sui-section");

        // No inline override: stacked sections must be separated by the
        // stylesheet default, without every page asking for a margin.
        Assert.DoesNotContain("sui-section--attached", root.ClassList);
        Assert.True(string.IsNullOrEmpty(root.GetAttribute("style")));
    }

    [Theory]
    [InlineData(2, "--sui-section-gap:var(--sui-space-2);")]
    [InlineData(0, "--sui-section-gap:0;")]
    public void Section_OverridesTheGapOnRequest(int gap, string expected)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Filtros")
            .Add(component => component.Gap, gap)
            .Add(component => component.Style, "--consumer-probe:1;")
            .AddChildContent("Conteúdo"));

        var style = cut.Find(".sui-section").GetAttribute("style");

        Assert.Contains(expected, style);
        Assert.Contains("--consumer-probe:1;", style);
    }

    [Fact]
    public void Section_AttachedGluesItselfToThePreviousSection()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Transferências")
            .Add(component => component.Attached, true)
            .AddChildContent("Conteúdo"));

        var root = cut.Find(".sui-section");

        Assert.Contains("sui-section--attached", root.ClassList);
        // Attached means glued: the gap is zero by definition, so the stylesheet
        // only has to flatten the seam.
        Assert.Contains("--sui-section-gap:0;", root.GetAttribute("style"));
    }

    [Fact]
    public void SectionStylesheet_SeparatesStackedSectionsAndFlattensAttachedSeams()
    {
        var css = Compact(File.ReadAllText(
            Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css")));

        // Default rhythm, overridable per section through the variable.
        Assert.Contains(
            ".sui-section+.sui-section{margin-block-start:var(--sui-section-gap,var(--sui-space-4))}",
            css);

        // The seam: bottom corners of the previous card and top corners of the
        // attached one go flat, and the shared border is drawn only once.
        Assert.Contains(".sui-section:has(+.sui-section--attached)>.sui-card{", css);
        Assert.Contains(".sui-section+.sui-section--attached>.sui-card{", css);
        Assert.Contains("border-block-start:0", css);

        // A stack around the sections already spaces them through its gap;
        // keeping the margin as well would double the breathing room.
        Assert.Contains(".sui-stack>.sui-section+.sui-section{margin-block-start:0}", css);
    }

    private static string Compact(string css)
        => string.Concat(css.Where(character => !char.IsWhiteSpace(character)));

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
