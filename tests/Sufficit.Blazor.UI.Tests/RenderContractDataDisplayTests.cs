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
    public void DateTime_WrapsEachTokenInNonBreakingSpan()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDateTime>(parameters => parameters
            .Add(component => component.Value, new DateTime(2026, 9, 7, 14, 30, 0))
            .Add(component => component.Format, "dd/MM/yyyy HH:mm")
            .Add(component => component.Class, "probe"));

        var root = cut.Find("span.sui-datetime");
        Assert.True(root.ClassList.Contains("probe"));
        // Each whitespace-separated token (date, time) is its own nowrap span,
        // while the space between them stays a soft-wrap opportunity.
        var tokens = cut.FindAll("span.sui-datetime__token");
        Assert.Equal(2, tokens.Count);
        Assert.Equal("07/09/2026", tokens[0].TextContent);
        Assert.Equal("14:30", tokens[1].TextContent);
    }

    [Fact]
    public void DateTime_SingleTokenForDateOnlyFormat()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDateTime>(parameters => parameters
            .Add(component => component.Value, new DateTime(2026, 9, 7)));

        var tokens = cut.FindAll("span.sui-datetime__token");
        Assert.Single(tokens);
        Assert.Equal("07/09/2026", tokens[0].TextContent);
    }

    [Fact]
    public void DateTime_RendersNothingWhenValueIsNull()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDateTime>();

        Assert.Empty(cut.FindAll("span.sui-datetime__token"));
        Assert.Equal(string.Empty, cut.Find("span.sui-datetime").TextContent);
    }

    [Fact]
    public void DateTime_HtmlEncodesLiteralTokens()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIDateTime>(parameters => parameters
            .Add(component => component.Value, new DateTime(2026, 9, 7))
            .Add(component => component.Format, "'<b>' dd/MM/yyyy"));

        var tokens = cut.FindAll("span.sui-datetime__token");
        Assert.Equal("<b>", tokens[0].TextContent);
        // Encoded as text, never parsed as markup by the browser.
        Assert.DoesNotContain("<b>", cut.Markup);
    }

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
    public void Stat_AppearanceDeltaAndSparkline()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStat>(parameters => parameters
            .Add(component => component.Value, "9")
            .Add(component => component.Unit, "/20")
            .Add(component => component.Icon, "phone")
            .Add(component => component.ColorValue, SUIColor.Error)
            .Add(component => component.Appearance, SUIStatAppearance.Gradient)
            .Add(component => component.Delta, "-2")
            .Add(component => component.Trend, SUITrend.Down)
            .Add(component => component.HigherIsBetter, false)
            .Add(component => component.Caption, "vs. ontem")
            .Add(component => component.Sparkline, new double[] { 1, 3, 2 }));

        var root = cut.Find(".sui-stat");
        Assert.True(root.ClassList.Contains("sui-stat--gradient"));
        Assert.True(root.ClassList.Contains("sui-stat--error"));
        Assert.Equal("/20", cut.Find(".sui-stat__unit").TextContent);
        // Lower is better: a fall is good news.
        Assert.True(cut.Find(".sui-stat__delta").ClassList.Contains("sui-stat__delta--good"));
        Assert.Equal("true", cut.Find(".sui-stat__mark").GetAttribute("aria-hidden"));
        Assert.Equal(3, cut.Find(".sui-stat__spark-line").GetAttribute("points")!.Split(' ').Length);
    }

    [Fact]
    public void Stat_LoadingSkeletonAndAnimationFlags()
    {
        using var context = new BunitContext();
        var loading = context.Render<SUIStat>(parameters => parameters.Add(c => c.Label, "Chamadas").Add(c => c.Loading, true));
        Assert.Equal("true", loading.Find(".sui-stat__copy").GetAttribute("aria-busy"));
        Assert.Equal(2, loading.FindAll(".sui-stat__skeleton").Count);
        Assert.True(loading.Find(".sui-stat").ClassList.Contains("sui-stat--anim-loading"));
        var none = context.Render<SUIStat>(parameters => parameters.Add(c => c.Value, "1").Add(c => c.Loading, true).Add(c => c.Animation, SUIAnimation.None));
        Assert.DoesNotContain(none.Find(".sui-stat").ClassList, name => name.StartsWith("sui-stat--anim-", StringComparison.Ordinal));
        var stat = context.Render<SUIStat>(parameters => parameters.Add(c => c.Value, "5").Add(c => c.Animation, SUIAnimation.ValueChange));
        Assert.Empty(stat.FindAll(".sui-stat__number--changed"));
        stat.Render(parameters => parameters.Add(c => c.Value, "6"));
        Assert.NotEmpty(stat.FindAll(".sui-stat__number--changed"));
        Assert.Equal("6", stat.Find(".sui-stat__number").TextContent);
    }

    [Fact]
    public void AnimationPolicy_ComponentParameterWinsOverApplicationDefault()
    {
        var policy = new Sufficit.Blazor.UI.Services.SUIAnimationPolicy(SUIAnimation.None);
        Assert.Equal(SUIAnimation.None, Sufficit.Blazor.UI.Services.SUIAnimationPolicy.Resolve(null, policy));
        Assert.Equal(SUIAnimation.Enter, Sufficit.Blazor.UI.Services.SUIAnimationPolicy.Resolve(SUIAnimation.Enter, policy));
        Assert.Equal(SUIAnimation.Default, Sufficit.Blazor.UI.Services.SUIAnimationPolicy.Resolve(null, null));
    }

    [Fact]
    public void Stat_PlainHasNoWatermarkAndImageRejectsUnsafeUrls()
    {
        using var context = new BunitContext();
        var plain = context.Render<SUIStat>(parameters => parameters.Add(c => c.Value, "1").Add(c => c.Icon, "phone"));
        Assert.Empty(plain.FindAll(".sui-stat__mark"));
        var unsafeImage = context.Render<SUIStat>(parameters => parameters
            .Add(c => c.Appearance, SUIStatAppearance.Image)
            .Add(c => c.BackgroundImage, "x\"); background:url(javascript:alert(1))"));
        Assert.DoesNotContain("--sui-stat-image", unsafeImage.Find(".sui-stat").GetAttribute("style") ?? "", StringComparison.Ordinal);
        var image = context.Render<SUIStat>(parameters => parameters
            .Add(c => c.Appearance, SUIStatAppearance.Image)
            .Add(c => c.BackgroundImage, "/images/bg.webp"));
        Assert.Contains("--sui-stat-image:url(\"/images/bg.webp\")", image.Find(".sui-stat").GetAttribute("style"), StringComparison.Ordinal);
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
