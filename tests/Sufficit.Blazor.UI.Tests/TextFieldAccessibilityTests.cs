using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class TextFieldAccessibilityTests
{
    [Fact]
    public void ClearableAdornmentHasAccessibleActionAndClearsValue()
    {
        using var context = new BunitContext();
        string? changed = "unchanged";
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Value, "provider")
            .Add(component => component.Clearable, true)
            .Add(component => component.ClearText, "Limpar busca")
            .Add(component => component.AdornmentIcon, "search")
            .Add(component => component.ValueChanged, value => changed = value));

        var clear = cut.Find("button.sui-text-field__clear");
        Assert.Equal("Limpar busca", clear.GetAttribute("aria-label"));
        Assert.All(cut.FindAll("svg"), icon => Assert.Equal("true", icon.GetAttribute("aria-hidden")));

        clear.Click();

        Assert.Null(changed);
    }
}
