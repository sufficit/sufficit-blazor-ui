using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// SUISection.ToneValue: a danger/warning zone is the same section with a tone
/// class, never an inline color (strict CSP), and without a tone the markup is
/// exactly what it was before the parameter existed.
/// </summary>
public sealed class SectionToneTests
{
    [Theory]
    [InlineData(SUITone.Danger, "sui-section--danger")]
    [InlineData(SUITone.Warning, "sui-section--warning")]
    public void Tone_AddsTheToneClassesWithoutInlineStyle(SUITone tone, string expected)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Excluir")
            .Add(component => component.ToneValue, tone));

        var root = cut.Find("section.sui-section");

        Assert.Contains("sui-section--tone", root.ClassList);
        Assert.Contains(expected, root.ClassList);
        Assert.Empty(cut.FindAll("[style]"));
    }

    [Theory]
    [InlineData(SUITone.Neutral)]
    [InlineData(SUITone.Info)]
    [InlineData(SUITone.Success)]
    public void NonConsequentialTones_RenderThePlainSection(SUITone tone)
    {
        using var context = new BunitContext();
        var plain = context.Render<SUISection>(parameters => parameters.Add(component => component.Title, "Filtros"));
        var toned = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Filtros")
            .Add(component => component.ToneValue, tone));

        toned.MarkupMatches(plain.Markup);
    }

    [Fact]
    public void DangerZone_KeepsCopyAndActionsInTheHeaderAndConfirmationInTheContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Excluir tema")
            .Add(component => component.Description, "Não pode ser desfeito.")
            .Add(component => component.ToneValue, SUITone.Danger)
            .Add(component => component.Actions, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "id", "delete");
                builder.AddContent(2, "Excluir");
                builder.CloseElement();
            }))
            .AddChildContent("<form id=\"confirm\"></form>"));

        var header = cut.Find(".sui-card__header");

        Assert.Equal("Excluir tema", header.QuerySelector("h2.sui-card__header-title")!.TextContent);
        Assert.Equal("Não pode ser desfeito.", header.QuerySelector(".sui-card__header-description")!.TextContent);
        Assert.NotNull(header.QuerySelector(".sui-card__header-actions > #delete"));
        Assert.NotNull(cut.Find(".sui-card__content").QuerySelector("#confirm"));
    }

    [Fact]
    public void FlatSection_TakesTheToneToo()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISection>(parameters => parameters
            .Add(component => component.Title, "Sessões")
            .Add(component => component.Flat, true)
            .Add(component => component.ToneValue, SUITone.Danger));

        var root = cut.Find("section.sui-section");

        Assert.Contains("sui-section--flat", root.ClassList);
        Assert.Contains("sui-section--danger", root.ClassList);
    }

    [Fact]
    public void Stylesheet_TintsOnlyBorderAndTitle_AndWrapsTheActions()
    {
        var css = RepositoryLayout.ReadStyles("sui-section.css");

        Assert.Contains(".sui-section--danger { --_tone: var(--sui-color-error); }", css);
        Assert.Contains(".sui-section--warning { --_tone: var(--sui-color-warning); }", css);
        Assert.Contains(".sui-section--tone > .sui-card {", css);
        Assert.Contains(".sui-section--tone > .sui-card .sui-card__header-title", css);
        Assert.Contains(":has(> .sui-card__header-actions)", css);
        Assert.Contains("flex-wrap: wrap;", css);
        // Doctrine: color distinguishes, it does not promote. No lift, no shadow.
        Assert.DoesNotContain("transform", css);
        Assert.DoesNotContain("box-shadow", css);
    }
}
