using System.Globalization;
using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class DateFieldTests
{
    [Fact]
    public void DateField_RendersPortugueseCalendarAndIsoFormValue()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var selected = new DateOnly(2026, 8, 1);
        var cut = context.Render<SUIDateField>(parameters => parameters
            .Add(component => component.Id, "registered-from")
            .Add(component => component.Name, "registeredFrom")
            .Add(component => component.Label, "Cadastro inicial")
            .Add(component => component.HelperText, "Use a data local.")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("pt-BR"))
            .Add(component => component.Value, selected)
            .Add(component => component.ValueChanged, value => selected = value ?? selected));

        var trigger = cut.Find("button.sui-date-field__trigger");
        var label = cut.Find("label");
        var helper = cut.Find(".sui-field__helper");

        Assert.Contains("01/08/2026", trigger.TextContent, StringComparison.Ordinal);
        Assert.Equal("registered-from", label.GetAttribute("for"));
        Assert.False(string.IsNullOrWhiteSpace(helper.Id));
        Assert.Contains(helper.Id!, trigger.GetAttribute("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);
        Assert.Equal("2026-08-01", cut.Find("input[type=hidden]").GetAttribute("value"));
        Assert.Empty(cut.FindAll("input[type=date]"));

        trigger.Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("true", trigger.GetAttribute("aria-expanded"));
            Assert.Equal("agosto de 2026", cut.Find(".sui-date-field__month").TextContent);
            Assert.Equal("Hoje", cut.FindAll(".sui-date-field__action")[0].TextContent);
            Assert.Equal("Limpar", cut.FindAll(".sui-date-field__action")[1].TextContent);
        });

        cut.Find("[data-sui-date='2026-08-02']").Click();
        Assert.Equal(new DateOnly(2026, 8, 2), selected);
    }

    [Fact]
    public void DateField_UsesEnglishCultureAndExposesInvalidRelationship()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUIDateField>(parameters => parameters
            .Add(component => component.Label, "Registered from")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo("en-US"))
            .Add(component => component.Value, new DateOnly(2026, 8, 17))
            .Add(component => component.Invalid, true)
            .Add(component => component.ErrorText, "Choose a valid date."));

        var trigger = cut.Find("button.sui-date-field__trigger");
        var error = cut.Find(".sui-field__error");

        Assert.Contains("8/17/2026", trigger.TextContent, StringComparison.Ordinal);
        Assert.Equal("true", trigger.GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, trigger.GetAttribute("aria-errormessage"));
        Assert.False(string.IsNullOrWhiteSpace(error.Id));
        Assert.Contains(error.Id!, trigger.GetAttribute("aria-describedby") ?? string.Empty,
            StringComparison.Ordinal);

        trigger.Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("August 2026", cut.Find(".sui-date-field__month").TextContent);
            Assert.Equal("Today", cut.FindAll(".sui-date-field__action")[0].TextContent);
            Assert.Equal("Clear", cut.FindAll(".sui-date-field__action")[1].TextContent);
        });
    }
}
