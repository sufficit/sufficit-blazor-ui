using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Controls that accept a <c>Name</c> are promising to take part in a plain
/// HTML form, and a form submits values, not states. A checkbox or a radio
/// without a value attribute can only submit the literal <c>on</c>: enough to
/// know something was ticked, never enough to know what.
/// </summary>
public sealed class PlainFormSubmissionTests
{
    [Fact]
    public void CheckboxSubmitsTheTokenTheReceiverExpects()
    {
        using var context = new BunitContext();

        var cut = context.Render<SUICheckbox>(p => p
            .Add(x => x.Name, "RememberMe")
            .Add(x => x.FormValue, "true")
            .Add(x => x.Value, true));

        var input = cut.Find("input");
        Assert.Equal("RememberMe", input.GetAttribute("name"));
        Assert.Equal("true", input.GetAttribute("value"));
    }

    [Fact]
    public void CheckboxWithoutAFormValueKeepsTheHtmlDefault()
    {
        using var context = new BunitContext();

        // Existing forms must not change shape just because the parameter now
        // exists: no value attribute means the browser submits "on".
        var cut = context.Render<SUICheckbox>(p => p.Add(x => x.Name, "Accepted"));

        Assert.Null(cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void CheckboxShowsItsTickWithoutWaitingForARerender()
    {
        using var context = new BunitContext();

        // Under static server rendering there is no re-render to wait for, and
        // two of the three checkboxes this library has to serve live there.
        // The tick has to be in the markup, revealed by :checked.
        var cut = context.Render<SUICheckbox>(p => p.Add(x => x.Name, "RememberMe"));

        Assert.NotNull(cut.Find(".sui-checkbox__box svg"));
        Assert.DoesNotContain("sui-checkbox--checked", cut.Find("label").ClassName);
    }

    [Fact]
    public void ChoiceCardSubmitsTheOptionItAlreadyRepresents()
    {
        using var context = new BunitContext();

        var cut = context.Render<SUIChoiceCard<string>>(p => p
            .Add(x => x.Name, "plan")
            .Add(x => x.Value, "annual")
            .Add(x => x.SelectedValue, "annual")
            .Add(x => x.Title, "Anual"));

        var input = cut.Find("input");
        Assert.Equal("radio", input.GetAttribute("type"));
        Assert.Equal("plan", input.GetAttribute("name"));
        Assert.Equal("annual", input.GetAttribute("value"));
    }

    [Fact]
    public void ChoiceCardRendersItsValueInvariantlyOfTheReadersCulture()
    {
        using var context = new BunitContext();

        // The token on the wire is read by a server, not by a person: a decimal
        // that turns into "1,5" because the page is Portuguese is a bug the
        // receiver cannot defend against.
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
        try
        {
            var cut = context.Render<SUIChoiceCard<decimal>>(p => p
                .Add(x => x.Name, "amount")
                .Add(x => x.Value, 1.5m)
                .Add(x => x.Title, "Um e meio"));

            Assert.Equal("1.5", cut.Find("input").GetAttribute("value"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }
}
