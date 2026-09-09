using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SwitchAccessibilityTests
{
    [Fact]
    public void HelpAndErrorDescribeTheCheckboxWithoutChangingItsName()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISwitch>(p => p
            .Add(x => x.Label, "Notificações")
            .Add(x => x.HelperText, "Receber avisos.")
            .Add(x => x.ErrorText, "Confirme a preferência."));
        var input = cut.Find("input");
        Assert.Equal("Notificações", cut.Find("#" + input.GetAttribute("aria-labelledby")).TextContent);
        var descriptions = input.GetAttribute("aria-describedby")!.Split(' ');
        Assert.Equal(new[] { "Receber avisos.", "Confirme a preferência." }, descriptions.Select(id => cut.Find("#" + id).TextContent));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(descriptions[1], input.GetAttribute("aria-errormessage"));
        var id = input.Id;
        cut.Render(p => p.Add(x => x.ErrorText, null).Add(x => x.HelperText, null));
        Assert.Equal(id, cut.Find("input").Id);
        Assert.Null(cut.Find("input").GetAttribute("aria-describedby"));
        Assert.Null(cut.Find("input").GetAttribute("aria-errormessage"));
    }

    [Fact]
    public void InstancesHaveUniqueIdsAndDisabledSwitchCannotEmitChanges()
    {
        using var context = new BunitContext();
        var changes = 0;
        var first = context.Render<SUISwitch>(p => p.Add(x => x.Label, "Primeiro")
            .Add(x => x.ValueChanged, _ => changes++));
        var second = context.Render<SUISwitch>(p => p.Add(x => x.Label, "Segundo"));
        Assert.NotEqual(first.Find("input").Id, second.Find("input").Id);
        first.Find("input").Change(true);
        Assert.Equal(1, changes);
        first.Render(p => p.Add(x => x.Disabled, true));
        first.Find("input").Change(false);
        Assert.Equal(1, changes);
    }
}
