using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Behaviour promoted from sufficit-blazor's FilterTree, where 100 usages
/// depend on it. These pin the matching semantics so the promotion cannot
/// silently change them: case-insensitive contains on tags, groups following
/// their children, and everything visible while no filter is active.
/// </summary>
public sealed class FilterTreeTests
{
    [Fact]
    public void WithoutScope_EverythingIsVisible()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFilterTree>(parameters => parameters
            .Add(component => component.Tags, new[] { "telefonia" })
            .AddChildContent("<span id='leaf'>Telefonia</span>"));

        Assert.NotNull(cut.Find("#leaf"));
    }

    [Fact]
    public void BlankFilter_ShowsLeavesWithoutTags()
    {
        using var context = new BunitContext();
        var cut = RenderInScope(context, "   ", tree => tree
            .AddChildContent("<span id='leaf'>Sem tags</span>"));

        Assert.NotNull(cut.Find("#leaf"));
    }

    [Theory]
    [InlineData("tele")]
    [InlineData("TELE")]
    [InlineData("fonia")]
    public void MatchingIsCaseInsensitiveContains(string filter)
    {
        using var context = new BunitContext();
        var cut = RenderInScope(context, filter, tree => tree
            .Add(component => component.Tags, new[] { "Telefonia" })
            .AddChildContent("<span id='leaf'>Telefonia</span>"));

        Assert.NotNull(cut.Find("#leaf"));
    }

    [Fact]
    public void ActiveFilter_HidesLeafWithoutTags()
    {
        using var context = new BunitContext();
        var cut = RenderInScope(context, "vendas", tree => tree
            .AddChildContent("<span id='leaf'>Sem tags</span>"));

        Assert.Empty(cut.FindAll("#leaf"));
    }

    [Fact]
    public void Group_StaysVisibleWhileAnyChildMatches()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFilterScope>(parameters => parameters
            .Add(scope => scope.FilterText, "vendas")
            .AddChildContent<SUIFilterTree>(group => group
                .Add(component => component.Group, true)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<SUIFilterTree>(0);
                    builder.AddAttribute(1, nameof(SUIFilterTree.Tags), new[] { "vendas" });
                    builder.AddAttribute(2, "ChildContent",
                        (RenderFragment)(b => b.AddMarkupContent(0, "<span id='match'>Vendas</span>")));
                    builder.CloseComponent();

                    builder.OpenComponent<SUIFilterTree>(3);
                    builder.AddAttribute(4, nameof(SUIFilterTree.Tags), new[] { "compras" });
                    builder.AddAttribute(5, "ChildContent",
                        (RenderFragment)(b => b.AddMarkupContent(0, "<span id='other'>Compras</span>")));
                    builder.CloseComponent();
                })));

        // The matching leaf renders; the non-matching sibling does not; and the
        // group wrapper is present without display:none.
        Assert.NotNull(cut.Find("#match"));
        Assert.Empty(cut.FindAll("#other"));
        var wrapper = cut.Find("div");
        Assert.DoesNotContain("display:none", wrapper.GetAttribute("style") ?? string.Empty);
    }

    [Fact]
    public void Group_HidesWhenNoChildMatches_ButKeepsChildrenMounted()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFilterScope>(parameters => parameters
            .Add(scope => scope.FilterText, "financeiro")
            .AddChildContent<SUIFilterTree>(group => group
                .Add(component => component.Group, true)
                .AddChildContent<SUIFilterTree>(leaf => leaf
                    .Add(component => component.Tags, new[] { "vendas" })
                    .AddChildContent("<span id='leaf'>Vendas</span>"))));

        // Hidden with display:none rather than removed: unmounting would
        // dispose the children, and disposed children cannot report a match
        // when the filter changes again.
        var wrapper = cut.Find("div");
        Assert.Contains("display:none", wrapper.GetAttribute("style") ?? string.Empty);
    }

    [Fact]
    public void ClearingTheFilter_BringsEverythingBack()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIFilterScope>(parameters => parameters
            .Add(scope => scope.FilterText, "nada-casa")
            .AddChildContent<SUIFilterTree>(leaf => leaf
                .Add(component => component.Tags, new[] { "vendas" })
                .AddChildContent("<span id='leaf'>Vendas</span>")));

        Assert.Empty(cut.FindAll("#leaf"));

        cut.Render(parameters => parameters
            .Add(scope => scope.FilterText, string.Empty));

        Assert.NotNull(cut.Find("#leaf"));
    }

    private static IRenderedComponent<SUIFilterScope> RenderInScope(
        BunitContext context,
        string? filterText,
        Action<ComponentParameterCollectionBuilder<SUIFilterTree>> configureTree)
        => context.Render<SUIFilterScope>(parameters => parameters
            .Add(scope => scope.FilterText, filterText)
            .AddChildContent(configureTree));
}
