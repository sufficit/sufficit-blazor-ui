using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Virtualization contract of <see cref="SUITable{TItem}"/>: the windowed body
/// keeps the same row markup, grid semantics and original indexes as the
/// materialized one, spacer rows stay inside the row group, and the feature
/// remains opt-in.
/// </summary>
public sealed class SUITableVirtualizationTests
{
    private static readonly Regex RowIndexClass = new("row-(\\d+)", RegexOptions.Compiled);

    private static RenderFragment<string> ItemToCell => item => builder =>
    {
        builder.OpenElement(0, "td");
        builder.AddContent(1, item);
        builder.CloseElement();
    };

    [Fact]
    public void Table_WithoutVirtualize_MaterializesEveryRow()
    {
        using var context = new BunitContext();
        var items = Enumerable.Range(1, 200).Select(i => $"item {i}").ToArray();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, items)
            .Add(component => component.RowTemplate, ItemToCell));

        // Opt-in means opt-in: the default path still renders one tr per item.
        Assert.Equal(200, cut.FindAll("tbody tr.sui-table__row").Count);
        Assert.Empty(cut.FindAll("tbody tr:not(.sui-table__row)"));
    }

    [Fact]
    public void Table_Virtualize_RendersARowWindowWithTableSpacers()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var items = Enumerable.Range(1, 200).Select(i => $"item {i}").ToArray();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, items)
            .Add(component => component.Virtualize, true)
            .Add(component => component.RowTemplate, ItemToCell));

        // Only the window exists in the DOM, and it starts at the first item.
        var dataRows = cut.FindAll("tbody tr.sui-table__row");
        Assert.InRange(dataRows.Count, 1, 199);
        Assert.Equal("item 1", dataRows[0].TextContent);

        // Spacers are table rows inside tbody: a div spacer would be hoisted
        // out of the table by the HTML parser and break the grid geometry.
        var spacers = cut.FindAll("tbody tr:not(.sui-table__row)");
        Assert.Equal(2, spacers.Count);
        Assert.All(spacers, spacer => Assert.Empty(spacer.QuerySelectorAll("td")));
    }

    [Fact]
    public void Table_Virtualize_PreservesGridSemanticsAndOriginalIndexes()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        // 200 identical values: any implementation that recovers the index by
        // searching the item back in the list misindexes every row.
        var items = Enumerable.Repeat("Dup", 200).ToArray();
        var clicked = new List<string>();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, items)
            .Add(component => component.Virtualize, true)
            .Add(component => component.RowClassFunc, (_, index) => $"row-{index}")
            .Add(component => component.OnRowClick, item => clicked.Add(item))
            .Add(component => component.RowTemplate, ItemToCell));

        Assert.Equal("grid", cut.Find("table").GetAttribute("role"));
        var rows = cut.FindAll("tbody tr.sui-table__row");
        Assert.True(rows.Count > 1);

        // The original index travels with the virtualized item: windowed rows
        // carry sequential indexes from zero even for duplicate values.
        var indexes = rows
            .Select(row => RowIndexClass.Match(row.GetAttribute("class") ?? string.Empty))
            .Select(match => int.Parse(match.Groups[1].Value))
            .ToArray();
        Assert.Equal(Enumerable.Range(0, indexes.Length), indexes);

        // Roving tabindex survives: one row in the tab order, the rest at -1.
        Assert.Equal("0", rows[0].GetAttribute("tabindex"));
        Assert.All(rows.Skip(1), row => Assert.Equal("-1", row.GetAttribute("tabindex")));

        rows[1].Click();
        Assert.Equal(["Dup"], clicked);
    }

    [Fact]
    public void Table_Virtualize_ReadOnlyScrollerIsAFocusableNamedRegion()
    {
        using var context = new BunitContext();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["a", "b"])
            .Add(component => component.Virtualize, true)
            .Add(component => component.ScrollLabel, "Lançamentos")
            .Add(component => component.RowTemplate, ItemToCell));

        // A scroller keyboard users cannot focus is unreachable (WCAG 2.1.1;
        // axe: scrollable-region-focusable).
        var wrapper = cut.Find(".sui-table-wrapper--virtual");
        Assert.Equal("0", wrapper.GetAttribute("tabindex"));
        Assert.Equal("region", wrapper.GetAttribute("role"));
        Assert.Equal("Lançamentos", wrapper.GetAttribute("aria-label"));
    }

    [Fact]
    public void Table_WithoutVirtualize_WrapperStaysAPlainContainer()
    {
        using var context = new BunitContext();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["a"])
            .Add(component => component.RowTemplate, ItemToCell));

        var wrapper = cut.Find(".sui-table-wrapper");
        Assert.Null(wrapper.GetAttribute("tabindex"));
        Assert.Null(wrapper.GetAttribute("role"));
        Assert.Null(wrapper.GetAttribute("aria-label"));
        Assert.DoesNotContain("sui-table-wrapper--virtual", wrapper.ClassList);
    }

    [Fact]
    public void Table_Virtualize_InteractiveRowsDoNotAlsoFocusTheWrapper()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["a", "b"])
            .Add(component => component.Virtualize, true)
            .Add(component => component.OnRowClick, _ => { })
            .Add(component => component.RowTemplate, ItemToCell));

        // Focusable rows already make the region reachable: a second tab stop
        // on the wrapper would only add noise to the grid's single tab stop.
        var wrapper = cut.Find(".sui-table-wrapper--virtual");
        Assert.Null(wrapper.GetAttribute("tabindex"));
        Assert.Null(wrapper.GetAttribute("role"));
    }

    [Fact]
    public void Table_Virtualize_EmptyItemsStillShowTheNoRecordsRow()
    {
        using var context = new BunitContext();

        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, Array.Empty<string>())
            .Add(component => component.Virtualize, true)
            .Add(component => component.ColumnCount, 2)
            .Add(component => component.NoRecordsContent,
                (RenderFragment)(builder => builder.AddContent(0, "Sem registros"))));

        var cell = cut.Find("tbody td");
        Assert.Equal("Sem registros", cell.TextContent);
        Assert.Equal("2", cell.GetAttribute("colspan"));
        // No window, no spacers: only the empty-state row exists.
        Assert.Empty(cut.FindAll("tbody tr:not(.sui-table__empty-row)"));
    }
}
