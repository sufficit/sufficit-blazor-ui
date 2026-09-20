using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// IsLoading comes from the parent, so the spinner can only appear after the
/// click reached the circuit and the diff came back. The button therefore
/// carries a marker a capture-phase listener acts on, which is what makes the
/// press feel answered on a slow link.
/// </summary>
public sealed class LoadingButtonInstantFeedbackTests
{
    [Fact]
    public void A_button_asks_for_instant_feedback_by_default()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var button = context.Render<SUILoadingButton>(p => p
            .AddChildContent("Sign in"));

        Assert.True(button.Find("button").HasAttribute("data-sui-instant-busy"));
    }

    [Fact]
    public void A_link_shaped_button_asks_for_it_too()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var button = context.Render<SUILoadingButton>(p => p
            .Add(b => b.Href, "/somewhere")
            .AddChildContent("Continue"));

        Assert.True(button.Find("a").HasAttribute("data-sui-instant-busy"));
    }

    [Fact]
    public void It_can_be_turned_off_for_a_click_that_shows_nothing()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var button = context.Render<SUILoadingButton>(p => p
            .Add(b => b.InstantFeedback, false)
            .AddChildContent("Quiet"));

        Assert.False(button.Find("button").HasAttribute("data-sui-instant-busy"));
    }

    [Fact]
    public void The_server_state_still_drives_the_real_spinner()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var button = context.Render<SUILoadingButton>(p => p
            .Add(b => b.IsLoading, true)
            .AddChildContent("Working"));

        var element = button.Find("button");
        Assert.Contains("sui-btn--loading", element.GetAttribute("class"));
        Assert.Equal("true", element.GetAttribute("aria-busy"));
        Assert.True(element.HasAttribute("disabled"));
    }
}
