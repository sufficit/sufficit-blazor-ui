using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// An editor that appears in response to an action has to take focus when it
/// appears, and only the page knows when that was. The field already focused
/// its own control internally; this makes the same handle reachable.
/// </summary>
public sealed class TextFieldFocusTests
{
    [Fact]
    public void TheFieldExposesAFocusHandleLikeTheButtonDoes()
    {
        var focus = typeof(SUITextField<string>).GetMethod(
            nameof(SUITextField<string>.FocusAsync));

        Assert.NotNull(focus);
        Assert.True(focus!.IsPublic);
        Assert.Equal(typeof(ValueTask), focus.ReturnType);
    }

    [Fact]
    public void BothModesCarryTheElementTheFocusHandleNeeds()
    {
        using var context = new BunitContext();

        // Multiline renders a textarea instead of an input; without a reference
        // on it, focusing a multiline field would have thrown.
        var singleLine = context.Render<SUITextField<string>>(p => p
            .Add(x => x.Label, "Nome"));
        var multiline = context.Render<SUITextField<string>>(p => p
            .Add(x => x.Label, "Observação")
            .Add(x => x.Multiline, true));

        Assert.NotNull(singleLine.Find("input.sui-field__input"));
        Assert.NotNull(multiline.Find("textarea.sui-field__input"));
    }
}
