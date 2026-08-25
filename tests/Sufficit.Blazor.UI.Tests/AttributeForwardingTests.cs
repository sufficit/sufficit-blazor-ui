using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class AttributeForwardingTests
{
    [Fact]
    public void InteractiveRoots_ForwardUnmatchedAttributes()
    {
        using var context = new BunitContext();
        var button = context.Render<SUIButton>(parameters => parameters
            .Add(component => component.UserAttributes,
                new Dictionary<string, object?> { ["data-probe"] = "button", ["aria-label"] = "Salvar" }));
        var card = context.Render<SUICard>(parameters => parameters
            .Add(component => component.Attributes,
                new Dictionary<string, object?> { ["data-probe"] = "card" }));

        Assert.Equal("button", button.Find("button").GetAttribute("data-probe"));
        Assert.Equal("Salvar", button.Find("button").GetAttribute("aria-label"));
        Assert.Equal("card", card.Find(".sui-card").GetAttribute("data-probe"));

        var alert = context.Render<SUIAlert>(parameters => parameters
            .AddUnmatched("data-probe", "alert")
            .AddUnmatched("aria-label", "Aviso"));
        var table = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, Array.Empty<string>())
            .AddUnmatched("data-probe", "table"));
        var chip = context.Render<SUIChip>(parameters => parameters
            .AddUnmatched("aria-live", "polite")
            .AddUnmatched("role", "status")
            .AddChildContent("3 notas encontradas"));

        Assert.Equal("alert", alert.Find("[role=alert]").GetAttribute("data-probe"));
        Assert.Equal("Aviso", alert.Find("[role=alert]").GetAttribute("aria-label"));
        Assert.Equal("table", table.Find("table").GetAttribute("data-probe"));
        Assert.Equal("polite", chip.Find(".sui-chip").GetAttribute("aria-live"));
        Assert.Equal("status", chip.Find(".sui-chip").GetAttribute("role"));
    }
}
