using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>Contracts for the shared markup-free SUIComponentBase and its first adopters.</summary>
public sealed class SUIComponentBaseTests
{
    [Theory]
    [InlineData(typeof(SUILink))]
    [InlineData(typeof(SUICheckbox))]
    [InlineData(typeof(SUISection))]
    [InlineData(typeof(SUIPagination))]
    [InlineData(typeof(SUIStat))]
    [InlineData(typeof(SUIProgressSteps))]
    [InlineData(typeof(SUISelect<string>))]
    [InlineData(typeof(SUITextField<string>))]
    public void MigratedComponents_InheritTheCanonicalSurface(Type component)
    {
        Assert.True(typeof(SUIComponentBase).IsAssignableFrom(component));
        Assert.Equal(typeof(SUIComponentBase), component.GetProperty(nameof(SUIComponentBase.Class))!.DeclaringType);
        Assert.Equal(typeof(SUIComponentBase), component.GetProperty(nameof(SUIComponentBase.Style))!.DeclaringType);
        Assert.Equal(typeof(SUIComponentBase), component.GetProperty(nameof(SUIComponentBase.AdditionalAttributes))!.DeclaringType);
        Assert.True(component.GetProperty(nameof(SUIComponentBase.AdditionalAttributes))!
            .IsDefined(typeof(ParameterAttribute), inherit: true));
    }

    [Fact]
    public void Link_PreservesExistingUnmatchedAttributes_AndSupportsInheritedStyle()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUILink>(parameters => parameters
            .Add(component => component.Href, "/conta")
            .Add(component => component.Class, "probe")
            .Add(component => component.Style, "font-weight:600")
            .AddUnmatched("data-test", "link"));

        var anchor = cut.Find("a.sui-link");
        Assert.Equal("/conta", anchor.GetAttribute("href"));
        Assert.Equal("link", anchor.GetAttribute("data-test"));
        Assert.True(anchor.ClassList.Contains("probe"));
        Assert.Contains("font-weight", anchor.GetAttribute("style"));
    }

    [Fact]
    public void GenericInputs_ForwardInheritedAndLegacyAttributesToTheControl()
    {
        using var context = new BunitContext();
        var select = context.Render<SUISelect<string>>(parameters => parameters
            .Add(component => component.Style, "max-width:200px")
            .AddUnmatched("data-probe", "select"));
        Assert.Equal("select", select.Find("button[role=combobox]").GetAttribute("data-probe"));
        Assert.Contains("max-width", select.Find(".sui-select").GetAttribute("style"));

#pragma warning disable CS0618 // Deliberately exercising the deprecated bridge.
        var text = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Style, "max-width:300px")
            .Add(component => component.UserAttributes, new Dictionary<string, object?>
            {
                ["data-legacy"] = "yes",
            })
            .AddUnmatched("data-probe", "text"));
#pragma warning restore CS0618
        Assert.Equal("yes", text.Find("input.sui-field__input").GetAttribute("data-legacy"));
        Assert.Equal("text", text.Find("input.sui-field__input").GetAttribute("data-probe"));
        Assert.Contains("max-width", text.Find(".sui-field").GetAttribute("style"));
    }

    [Fact]
    public void Link_LegacyUserAttributes_MergesWithoutClobberingUnmatchedValues()
    {
        using var context = new BunitContext();
#pragma warning disable CS0618 // Deliberately exercising the deprecated bridge.
        var cut = context.Render<SUILink>(parameters => parameters
            .Add(component => component.Href, "/conta")
            .Add(component => component.UserAttributes, new Dictionary<string, object?>
            {
                ["data-legacy"] = "yes",
                ["data-same"] = "old",
            })
            .AddUnmatched("data-same", "new"));
#pragma warning restore CS0618

        var anchor = cut.Find("a.sui-link");
        Assert.Equal("yes", anchor.GetAttribute("data-legacy"));
        Assert.Equal("new", anchor.GetAttribute("data-same"));
    }
}
