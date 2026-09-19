using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Defaults that keep the library compliant when the consumer does nothing
/// special (docs/rfc): a new-tab link protects its opener, and a sortable
/// header announces its state where assistive technology reads it — the th.
/// </summary>
public sealed class StandardsDefaultsTests
{
    [Theory]
    [InlineData("_blank", null, "noopener noreferrer")]
    [InlineData("_self", null, null)]
    [InlineData(null, null, null)]
    [InlineData("_blank", "noopener", "noopener")]
    public void LinkDefaultsRelLikeTheAnchorButtons(string? target, string? rel, string? expected)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUILink>(p => p
            .Add(x => x.Href, "https://example.com/")
            .Add(x => x.Target, target)
            .Add(x => x.Rel, rel));

        Assert.Equal(expected, cut.Find("a").GetAttribute("rel"));
    }

    [Fact]
    public void SortLabelInsideAHeaderPutsAriaSortOnTheHeader()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITh>(p => p.AddChildContent(SortLabel(SUISortDirection.Ascending)));
        Assert.Equal("ascending", cut.Find("th").GetAttribute("aria-sort"));

        // The consumer owns the direction and re-renders the header with it.
        cut.Render(p => p.AddChildContent(SortLabel(SUISortDirection.None)));
        cut.WaitForAssertion(() => Assert.Equal("none", cut.Find("th").GetAttribute("aria-sort")));
    }

    private static RenderFragment SortLabel(SUISortDirection direction) => builder =>
    {
        builder.OpenComponent<SUITableSortLabel<string>>(0);
        builder.AddComponentParameter(1, nameof(SUITableSortLabel<string>.SortDirection), direction);
        builder.AddComponentParameter(2, nameof(SUITableSortLabel<string>.ChildContent), (RenderFragment)(b => b.AddContent(0, "Nome")));
        builder.CloseComponent();
    };

    [Fact]
    public void HeaderWithoutSortingCarriesNoAriaSort()
    {
        using var context = new BunitContext();
        var plain = context.Render<SUITh>(p => p.AddChildContent("Nome"));
        Assert.Null(plain.Find("th").GetAttribute("aria-sort"));

        var explicitSort = context.Render<SUITh>(p => p
            .Add(x => x.SortDirection, SUISortDirection.Descending)
            .AddChildContent("Data"));
        Assert.Equal("descending", explicitSort.Find("th").GetAttribute("aria-sort"));
    }

    [Fact]
    public void Table_WithoutRowClick_StaysAPlainTableAndLoadsNoScript()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["API"])
            .Add(component => component.RowTemplate,
                item => builder =>
                {
                    builder.OpenElement(0, "td");
                    builder.AddContent(1, item);
                    builder.CloseElement();
                }));

        Assert.Null(cut.Find("table").GetAttribute("role"));
        Assert.Null(cut.Find("tbody tr").GetAttribute("tabindex"));
        Assert.Empty(context.JSInterop.Invocations);
    }
}
