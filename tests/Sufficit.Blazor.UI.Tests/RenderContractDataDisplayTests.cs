using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for the data-display family: root element, base
/// class, attribute forwarding where supported, and the accessibility contract
/// of the sort label.
/// </summary>
public sealed class RenderContractDataDisplayTests
{
    [Fact]
    public void List_RendersUnorderedListWithDenseModifier()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIList>(parameters => parameters
            .Add(component => component.Dense, true)
            .Add(component => component.Class, "probe")
            .AddChildContent("<li>item</li>"));

        var root = cut.Find("ul.sui-list");
        Assert.True(root.ClassList.Contains("sui-list--dense"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("item", root.QuerySelector("li")!.TextContent);
    }

    [Fact]
    public void ListItem_RendersPlainItemWithoutClickHandler()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIListItem>(parameters => parameters
            .AddChildContent("texto"));

        var root = cut.Find("li.sui-list__item");
        Assert.False(root.ClassList.Contains("sui-list__item--clickable"));
        Assert.Empty(cut.FindAll("button"));
        Assert.Equal("texto", root.TextContent.Trim());
    }

    [Fact]
    public void ListItem_WrapsContentInButtonWhenClickable()
    {
        using var context = new BunitContext();
        var clicked = false;
        var cut = context.Render<SUIListItem>(parameters => parameters
            .Add(component => component.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
            .AddChildContent("texto"));

        Assert.True(cut.Find("li").ClassList.Contains("sui-list__item--clickable"));
        var button = cut.Find("button.sui-list__button");
        Assert.Equal("button", button.GetAttribute("type"));

        button.Click();

        Assert.True(clicked);
    }

    [Fact]
    public void Stat_RendersValueLabelAndColorModifier()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStat>(parameters => parameters
            .Add(component => component.Value, "42")
            .Add(component => component.Label, "Chamadas")
            .Add(component => component.Class, "probe"));

        var root = cut.Find(".sui-stat");
        Assert.True(root.ClassList.Contains("sui-card"));
        Assert.True(root.ClassList.Contains("sui-stat--primary"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Contains("42", cut.Find(".sui-stat__copy").TextContent, StringComparison.Ordinal);
        Assert.Contains("Chamadas", cut.Find(".sui-stat__copy").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".sui-avatar"));
    }

    [Fact]
    public void Stat_RendersIconInsideAvatarWhenSupplied()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStat>(parameters => parameters
            .Add(component => component.Value, "1")
            .Add(component => component.Icon, "phone"));

        Assert.NotEmpty(cut.FindAll(".sui-avatar svg"));
    }

    [Fact]
    public void TableSortLabel_IsAButtonThatAnnouncesItsStateAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITableSortLabel<string>>(parameters => parameters
            .AddUnmatched("data-test", "sort")
            .AddChildContent("Nome"));

        var root = cut.Find("button.sui-sort-label");
        Assert.Equal("button", root.GetAttribute("type"));
        Assert.Equal("sort", root.GetAttribute("data-test"));
        Assert.False(root.ClassList.Contains("sui-sort-label--active"));
        Assert.Equal("Nome", cut.Find(".sui-sort-label__text").TextContent);
        Assert.Equal(", not sorted", cut.Find(".sui-sr-only").TextContent);
        Assert.Equal("true", cut.Find(".sui-sort-label__icon").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TableSortLabel_ClickRequestsTheNextDirection()
    {
        using var context = new BunitContext();
        SUISortDirection? requested = null;
        var cut = context.Render<SUITableSortLabel<string>>(parameters => parameters
            .Add(component => component.SortDirection, SUISortDirection.Ascending)
            .Add(component => component.SortDirectionChanged,
                EventCallback.Factory.Create<SUISortDirection>(this, next => requested = next)));

        var root = cut.Find("button");
        Assert.True(root.ClassList.Contains("sui-sort-label--active"));
        Assert.Equal(", sorted ascending", cut.Find(".sui-sr-only").TextContent);

        root.Click();

        Assert.Equal(SUISortDirection.Descending, requested);
    }

    [Fact]
    public void Timeline_RendersOrderedListWithPositionAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITimeline>(parameters => parameters
            .Add(component => component.TimelinePosition, "End")
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "timeline")
            .AddChildContent("<li>etapa</li>"));

        var root = cut.Find("ol.sui-timeline");
        Assert.True(root.ClassList.Contains("sui-timeline--end"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("timeline", root.GetAttribute("data-test"));
        Assert.Equal("etapa", root.QuerySelector("li")!.TextContent);
    }

    [Fact]
    public void TableEmpty_RendersDefaultTitleAndSplitsDescriptionIntoLines()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITableEmpty>();

        var root = cut.Find(".sui-table-empty");
        Assert.True(root.ClassList.Contains("sui-width-full"));
        Assert.Equal("Nenhum registro encontrado", cut.Find(".sui-table-empty__title").TextContent);
        Assert.Equal("true", cut.Find(".sui-table-empty__icon svg").GetAttribute("aria-hidden"));
        Assert.Equal(2, cut.FindAll(".sui-table-empty__description-line").Count);
        Assert.Empty(cut.FindAll(".sui-table-empty__content"));
    }

    [Fact]
    public void TableEmpty_RendersCustomCopyAndChildContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITableEmpty>(parameters => parameters
            .Add(component => component.Title, "Sem contatos")
            .Add(component => component.Description, null)
            .Add(component => component.Class, "probe")
            .AddChildContent("<button>Novo</button>"));

        Assert.True(cut.Find(".sui-table-empty").ClassList.Contains("probe"));
        Assert.Equal("Sem contatos", cut.Find(".sui-table-empty__title").TextContent);
        Assert.Empty(cut.FindAll(".sui-table-empty__description"));
        Assert.Equal("Novo", cut.Find(".sui-table-empty__content button").TextContent);
    }

    [Fact]
    public void Td_RendersCellWithDataLabelAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITd>(parameters => parameters
            .Add(component => component.DataLabel, "Nome")
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "cell")
            .AddUnmatched("colspan", "2")
            .AddChildContent("valor"));

        var root = cut.Find("td");
        Assert.Equal("Nome", root.GetAttribute("data-label"));
        Assert.Equal("probe", root.GetAttribute("class"));
        Assert.Equal("cell", root.GetAttribute("data-test"));
        Assert.Equal("2", root.GetAttribute("colspan"));
        Assert.Equal("valor", root.TextContent);
    }
}
