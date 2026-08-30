using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ResponsiveDrawerTests
{
    [Fact]
    public async Task CompactDrawer_UsesFullViewportAndExposesCloseAction()
    {
        using var context = CreateContext();
        var open = true;
        var cut = context.Render<SUIDrawer>(parameters => parameters
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value)
            .Add(component => component.Variant, "responsive")
            .Add(component => component.FullScreenOnCompact, true)
            .Add(component => component.Title, "Sufficit AI")
            .Add(component => component.Width, "280px")
            .AddChildContent("Navegação"));

        await cut.InvokeAsync(() => cut.Instance.SetCompactStateAsync(true));

        var drawer = cut.Find("aside.sui-drawer");
        Assert.Contains("sui-drawer--compact", drawer.ClassList);
        Assert.Contains("sui-drawer--fullscreen-compact", drawer.ClassList);
        Assert.Equal("dialog", drawer.GetAttribute("role"));
        Assert.Equal("true", drawer.GetAttribute("aria-modal"));
        Assert.Contains("--sui-drawer-width:280px", drawer.GetAttribute("style"));

        cut.Find("button[aria-label='Fechar navegação']").Click();

        Assert.False(open);
        Assert.DoesNotContain("sui-drawer--open", cut.Find("aside.sui-drawer").ClassList);
    }

    [Fact]
    public async Task CompactDrawer_ClosesAfterInternalNavigation()
    {
        using var context = CreateContext();
        var open = true;
        var cut = context.Render<SUIDrawer>(parameters => parameters
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value)
            .Add(component => component.Variant, "responsive")
            .Add(component => component.CloseOnNavigate, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SUINavLink>(0);
                builder.AddAttribute(1, nameof(SUINavLink.Href), "/providers");
                builder.AddAttribute(2, nameof(SUINavLink.Title), "Providers");
                builder.CloseComponent();
            }));
        await cut.InvokeAsync(() => cut.Instance.SetCompactStateAsync(true));

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        await cut.InvokeAsync(() => navigation.NavigateTo("/providers"));

        cut.WaitForAssertion(() => Assert.False(open));
    }

    [Fact]
    public async Task WideDrawer_RemainsOpenAfterNavigation()
    {
        using var context = CreateContext();
        var open = true;
        var cut = context.Render<SUIDrawer>(parameters => parameters
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, value => open = value)
            .Add(component => component.Variant, "responsive")
            .Add(component => component.CloseOnNavigate, true));
        await cut.InvokeAsync(() => cut.Instance.SetCompactStateAsync(false));

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        await cut.InvokeAsync(() => navigation.NavigateTo("/models"));

        Assert.True(open);
    }

    [Fact]
    public void NavLink_WithHrefAndCallback_RemainsANavigableLink()
    {
        using var context = CreateContext();
        var clicked = false;
        var cut = context.Render<SUINavLink>(parameters => parameters
            .Add(component => component.Href, "/activity")
            .Add(component => component.Title, "Activity")
            .Add(component => component.Match, NavLinkMatch.Prefix)
            .Add(component => component.OnClick, _ => clicked = true));

        var link = cut.Find("a[href='/activity']");
        link.Click();

        Assert.True(clicked);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }
}
