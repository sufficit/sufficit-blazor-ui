using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class TextFieldAccessibilityTests
{
    [Fact]
    public void ClearableAdornmentHasAccessibleActionAndClearsValue()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true);
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

    [Fact]
    public void RevealablePasswordTogglesInputTypeWithAccessibleState()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Id, "secret")
            .Add(component => component.Value, "hunter2")
            .Add(component => component.InputType, "password")
            .Add(component => component.Revealable, true));

        Assert.Equal("password", cut.Find("input").GetAttribute("type"));
        var toggle = cut.Find("button.sui-text-field__reveal");
        Assert.Equal("false", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("secret", toggle.GetAttribute("aria-controls"));
        Assert.Equal("Mostrar valor", toggle.GetAttribute("aria-label"));
        Assert.Contains("sui-text-field__input--pad-trailing", cut.Find("input").ClassName);

        toggle.Click();

        Assert.Equal("text", cut.Find("input").GetAttribute("type"));
        toggle = cut.Find("button.sui-text-field__reveal");
        Assert.Equal("true", toggle.GetAttribute("aria-pressed"));
        Assert.Equal("Ocultar valor", toggle.GetAttribute("aria-label"));
    }

    [Fact]
    public void RevealIsIgnoredOutsidePasswordInputs()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Value, "plain")
            .Add(component => component.Revealable, true));

        Assert.Empty(cut.FindAll("button.sui-text-field__reveal"));
        Assert.Equal("sui-field__input", cut.Find("input").ClassName);
    }
}
