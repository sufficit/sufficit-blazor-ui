using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SUITimelineTests
{
    [Fact]
    public void RendersStructuredDecisionContentAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITimelineItem>(parameters => parameters
            .Add(component => component.Title, "Provider selecionado")
            .Add(component => component.Description, "Primeira alternativa saudável compatível.")
            .Add(component => component.MetaContent, builder => builder.AddContent(0, "Etapa 2"))
            .Add(component => component.TrailingContent, builder => builder.AddMarkupContent(0, "<code>candidate_eligible</code>"))
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "modelo-x"))
            .AddUnmatched("data-sequence", "2"));

        Assert.Equal("Provider selecionado", cut.Find(".sui-timeline__title").TextContent);
        Assert.Equal("Primeira alternativa saudável compatível.", cut.Find(".sui-timeline__description").TextContent);
        Assert.Equal("Etapa 2", cut.Find(".sui-timeline__meta").TextContent);
        Assert.Equal("modelo-x", cut.Find(".sui-timeline__content").TextContent);
        Assert.Equal("candidate_eligible", cut.Find(".sui-timeline__trailing code").TextContent);
        Assert.Equal("2", cut.Find("li").GetAttribute("data-sequence"));
    }

    [Fact]
    public void KeepsFreeFormChildContentCompatible()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITimelineItem>(parameters => parameters
            .AddChildContent("Conteúdo legado"));

        Assert.Equal("Conteúdo legado", cut.Find(".sui-timeline__body").TextContent.Trim());
        Assert.Empty(cut.FindAll(".sui-timeline__heading"));
    }
}
