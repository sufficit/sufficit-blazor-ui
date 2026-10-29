using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SelectIconTests
{
    [Fact]
    public void TrailingIconStaysBeforeIndicatorWithoutMovingTheOptionIcon()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUISelect<string>>(p => p
            .Add(x => x.EndIcon, "phone")
            .Add(x => x.IconColorValue, SUIColor.Warning)
            .Add(x => x.Value, "a")
            .AddChildContent<SUISelectItem>(item => item
                .Add(x => x.Value, "a")
                .Add(x => x.Icon, "volume")
                .AddChildContent("Opção")));
        Assert.NotNull(cut.Find(".sui-select__value:first-child > .sui-select__icon"));
        Assert.Equal("true", cut.Find(".sui-select__value + .sui-color-warning").GetAttribute("aria-hidden"));
        Assert.NotNull(cut.Find(".sui-select__value + .sui-select__icon + .sui-select__indicator:last-child"));
        Assert.Single(cut.FindAll("[role=option] .sui-select__icon"));
        cut.Render(p => p.Add(x => x.EndIcon, (string?)null).Add(x => x.StartIcon, "phone"));
        Assert.NotNull(cut.Find(".sui-select__trigger > .sui-select__icon:first-child + .sui-select__value + .sui-select__indicator"));
        Assert.Equal("Opção", cut.Find(".sui-select__value").TextContent.Trim());
    }

    [Fact]
    public void IconsAreDecorativeAndSelectionStillSubmitsTheValue()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        Guid? changed = null;
        var cut = context.Render<SUISelect<Guid?>>(p => p
            .Add(x => x.StartIcon, "phone")
            .Add(x => x.IconColorValue, SUIColor.Success)
            .Add(x => x.Label, "Áudio")
            .Add(x => x.ValueChanged, value => changed = value)
            .AddChildContent<SUISelectItem>(item => item
                .Add(x => x.Value, Guid.Empty)
                .Add(x => x.Icon, "volume")
                .Add(x => x.IconColorValue, SUIColor.Info)
                .AddChildContent("Sem áudio")));
        Assert.Equal("true", cut.Find(".sui-select__trigger > .sui-select__icon").GetAttribute("aria-hidden"));
        Assert.NotNull(cut.Find(".sui-select__trigger > .sui-color-success svg"));
        cut.Find("[role=combobox]").Click();
        Assert.NotNull(cut.Find("[role=option] .sui-color-info svg"));
        cut.Find("[role=option]").Click();
        Assert.Equal(Guid.Empty, changed);
        // The owner applies the callback value, as @bind-Value does.
        cut.Render(p => p.Add(x => x.Value, changed));
        Assert.NotNull(cut.Find(".sui-select__value .sui-color-info svg"));
        Assert.Equal("Sem áudio", cut.Find(".sui-select__value").TextContent.Trim());
    }

    [Fact]
    public void ChangingAnItemIconRefreshesMenuAndSelectedValue()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUISelect<string>>(p => p
            .Add(x => x.Value, "a")
            .AddChildContent<SUISelectItem>(item => item
                .Add(x => x.Value, "a")
                .AddChildContent("Opção")));
        Assert.Empty(cut.FindAll("svg"));
        var item = cut.FindComponent<SUISelectItem>();
        item.Render(p => p.Add(x => x.Icon, "phone").Add(x => x.IconColorValue, SUIColor.Warning));
        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".sui-color-warning svg").Count));
        item.Render(p => p.Add(x => x.Icon, (string?)null));
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("svg")));
    }
}
