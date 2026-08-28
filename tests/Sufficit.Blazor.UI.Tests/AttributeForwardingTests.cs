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

    [Fact]
    public void Chip_AlignsItsContentAndExposesAnAccessibleRemoveAction()
    {
        using var context = new BunitContext();
        var selected = false;
        var removed = false;
        var chip = context.Render<SUIChip>(parameters => parameters
            .Add(component => component.Icon, "check-circle")
            .Add(component => component.SizeValue, SUISize.Small)
            .Add(component => component.ActionLabel, "Editar filtro Situação")
            .Add(component => component.OnClick, () => selected = true)
            .Add(component => component.RemoveLabel, "Remover filtro Situação")
            .Add(component => component.OnRemove, () => removed = true)
            .AddChildContent("Situação: Em aberto e pagos"));

        Assert.NotNull(chip.Find(".sui-chip__leading"));
        Assert.Equal("Situação: Em aberto e pagos", chip.Find(".sui-chip__label").TextContent);

        var action = chip.Find(".sui-chip__action");
        Assert.Equal("Editar filtro Situação", action.GetAttribute("aria-label"));
        action.Click();

        var remove = chip.Find(".sui-chip__remove");
        Assert.Equal("Remover filtro Situação", remove.GetAttribute("aria-label"));
        remove.Click();

        Assert.True(selected);
        Assert.True(removed);
    }
}
