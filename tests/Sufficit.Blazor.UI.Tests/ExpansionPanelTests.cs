using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ExpansionPanelTests
{
    [Fact]
    public void ToggleExposesExplicitAccessibleStateAndHidesCollapsedContent()
    {
        using var context = new BunitContext();
        var panel = context.Render<SUIExpansionPanel>(p => p
            .Add(c => c.Title, "Histórico")
            .AddChildContent("<p>Conteúdo preservado</p>"));

        Assert.Equal("false", panel.Find("button").GetAttribute("aria-expanded"));
        Assert.Empty(panel.FindAll(".sui-expansion__content"));
        panel.Find("button").Click();
        Assert.Equal("true", panel.Find("button").GetAttribute("aria-expanded"));
        Assert.Equal("Conteúdo preservado", panel.Find(".sui-expansion__content").TextContent);
        panel.Find("button").Click();
        Assert.Equal("false", panel.Find("button").GetAttribute("aria-expanded"));
        Assert.Empty(panel.FindAll(".sui-expansion__content"));
    }
}
