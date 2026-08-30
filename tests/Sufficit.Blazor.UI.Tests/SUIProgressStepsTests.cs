using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SUIProgressStepsTests
{
    [Fact]
    public void ExposesCurrentStepAndKeepsFutureStepsDisabled()
    {
        using var context = new BunitContext();
        var selected = -1;
        var cut = context.Render<SUIProgressSteps>(parameters => parameters
            .Add(component => component.Steps, ["Objetivo", "Origem", "Configuração", "Pronto"])
            .Add(component => component.ActiveIndex, 1)
            .Add(component => component.MaxReachableIndex, 2)
            .Add(component => component.ActiveIndexChanged, index => selected = index));

        var nav = cut.Find("nav");
        var buttons = cut.FindAll("button");

        Assert.Equal("Progresso da configuração", nav.GetAttribute("aria-label"));
        Assert.Equal("step", buttons[1].GetAttribute("aria-current"));
        Assert.Contains("Etapa 2 de 4", buttons[1].GetAttribute("aria-label"), StringComparison.Ordinal);
        Assert.False(buttons[2].HasAttribute("disabled"));
        Assert.True(buttons[3].HasAttribute("disabled"));
        Assert.NotEmpty(cut.FindAll(".is-completed svg"));

        buttons[0].Click();
        Assert.Equal(0, selected);
    }

    [Fact]
    public void ClampsAnOutOfRangeActiveIndex()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIProgressSteps>(parameters => parameters
            .Add(component => component.Steps, ["Primeira", "Segunda"])
            .Add(component => component.ActiveIndex, 99));

        var buttons = cut.FindAll("button");
        Assert.Null(buttons[0].GetAttribute("aria-current"));
        Assert.Equal("step", buttons[1].GetAttribute("aria-current"));
    }
}
