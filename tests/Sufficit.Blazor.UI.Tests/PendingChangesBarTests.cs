using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class PendingChangesBarTests
{
    [Fact]
    public void Renders_pending_summary_and_forwards_the_save_action()
    {
        using var context = new BunitContext();
        var saved = false;

        var rendered = context.Render<SUIPendingChangesBar>(parameters => parameters
            .Add(component => component.Description, "3 seções alteradas")
            .Add(component => component.OnSave,
                EventCallback.Factory.Create<MouseEventArgs>(this, () => saved = true))
            .AddUnmatched("data-probe", "pending-bar"));

        Assert.Equal("pending-bar", rendered.Find("section").GetAttribute("data-probe"));
        Assert.Contains("Alterações pendentes", rendered.Markup, StringComparison.Ordinal);
        Assert.Contains("3 seções alteradas", rendered.Markup, StringComparison.Ordinal);

        rendered.Find("button").Click();

        Assert.True(saved);
    }

    [Fact]
    public void Renders_cancel_action_only_when_supplied_and_forwards_the_event()
    {
        using var context = new BunitContext();
        var cancelled = false;

        var rendered = context.Render<SUIPendingChangesBar>(parameters => parameters
            .Add(component => component.OnCancel,
                EventCallback.Factory.Create<MouseEventArgs>(this, () => cancelled = true)));

        var buttons = rendered.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Contains("Cancelar alterações", buttons[0].TextContent, StringComparison.Ordinal);

        buttons[0].Click();

        Assert.True(cancelled);
    }

    [Theory]
    [InlineData("phone")]
    [InlineData("inbox")]
    public void Renders_destination_icons_instead_of_the_generic_circle(string icon)
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, icon));

        Assert.NotEmpty(rendered.FindAll("svg > path"));
        Assert.Empty(rendered.FindAll("svg > circle"));
    }
}
