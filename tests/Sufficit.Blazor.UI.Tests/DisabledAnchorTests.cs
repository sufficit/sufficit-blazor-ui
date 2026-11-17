using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class DisabledAnchorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledIconLinkHasNoNavigableTargetAndRestoresEnabledHref(bool loading)
    {
        using var context = new BunitContext(); context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIIconButton>(p => p.Add(c=>c.Href,"#private-target").Add(c=>c.Title,"Rastreamento").Add(c=>c.Disabled,!loading).Add(c=>c.Loading,loading));
        Assert.Equal("true",cut.Find("a").GetAttribute("aria-disabled")); Assert.False(cut.Find("a").HasAttribute("href")); Assert.Equal("-1",cut.Find("a").GetAttribute("tabindex"));
        cut.Render(p=>p.Add(c=>c.Disabled,false).Add(c=>c.Loading,false));Assert.Equal("#private-target",cut.Find("a").GetAttribute("href"));Assert.Null(cut.Find("a").GetAttribute("tabindex"));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledTextLinkHasNoNavigableTargetAndRestoresEnabledHref(bool loading)
    {
        using var context = new BunitContext(); context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIButton>(p => p.Add(c=>c.Href,"#private-target").Add(c=>c.ChildContent,"Abrir registro").Add(c=>c.Disabled,!loading).Add(c=>c.Loading,loading));
        Assert.Equal("true",cut.Find("a").GetAttribute("aria-disabled")); Assert.False(cut.Find("a").HasAttribute("href")); Assert.Equal("-1",cut.Find("a").GetAttribute("tabindex"));
        cut.Render(p=>p.Add(c=>c.Disabled,false).Add(c=>c.Loading,false));Assert.Equal("#private-target",cut.Find("a").GetAttribute("href"));Assert.Null(cut.Find("a").GetAttribute("tabindex"));
    }
}
