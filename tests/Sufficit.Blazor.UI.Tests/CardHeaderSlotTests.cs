using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class CardHeaderSlotTests
{
    [Fact]
    public void MetaContent_RendersInTheMetaRegion()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUICardHeader>(parameters => parameters
            .Add(component => component.Title, "Workspace")
            .Add(component => component.MetaContent, "<span class=\"probe\">Saudável</span>"));

        Assert.Equal("Saudável", rendered.Find(".sui-card__header-meta .probe").TextContent);
    }

    [Fact]
    public void Meta_WinsWhenBothSlotsAreSet()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUICardHeader>(parameters => parameters
            .Add(component => component.Meta, "<span>primary</span>")
            .Add(component => component.MetaContent, "<span>alias</span>"));

        Assert.Equal("primary", rendered.Find(".sui-card__header-meta").TextContent.Trim());
    }
}
