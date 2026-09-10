using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class NumericWheelSettingsTests
{
    [Fact]
    public void SpinnerButtonsAreOptInIndependentAndAccessible()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUINumericField<decimal>>(p => p.Add(x => x.Step, "0.01"));
        Assert.Empty(cut.FindAll("button"));
        cut.Render(p => p.Add(x => x.SpinnerStep, 10m)
            .Add(x => x.ChangeOnWheel, false).Add(x => x.ChangeOnArrowKeys, false));
        Assert.Equal("0.01", cut.Find("input").GetAttribute("step"));
        Assert.Equal(2, cut.FindAll("button[type=button]").Count);
        Assert.All(cut.FindAll("button"), b => {
            Assert.False(b.HasAttribute("disabled"));
            Assert.False(string.IsNullOrEmpty(b.GetAttribute("aria-label")));
            Assert.Equal(cut.Find("input").Id, b.GetAttribute("aria-controls"));
        });
        cut.Render(p => p.Add(x => x.Disabled, true));
        Assert.All(cut.FindAll("button"), b => Assert.True(b.HasAttribute("disabled")));
        cut.Render(p => p.Add(x => x.SpinnerStep, (decimal?)null));
        Assert.Empty(cut.FindAll("button"));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void IndependentSwitchesPreserveTypingPrecision(bool wheel, bool arrows)
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        decimal changed = 0;
        var cut = context.Render<SUINumericField<decimal>>(p => p
            .Add(x => x.Value, 360.37m).Add(x => x.Step, "0.01")
            .Add(x => x.ChangeOnWheel, wheel).Add(x => x.ChangeOnArrowKeys, arrows)
            .Add(x => x.WheelStep, 10m).Add(x => x.ArrowKeyStep, 10m)
            .Add(x => x.ValueChanged, v => changed = v));
        Assert.Equal(wheel, cut.Instance.ChangeOnWheel);
        Assert.Equal(arrows, cut.Instance.ChangeOnArrowKeys);
        Assert.Equal("0.01", cut.Find("input").GetAttribute("step"));
        cut.Find("input").Change("123.45");
        Assert.Equal(123.45m, changed);
    }

    [Fact]
    public void WheelIncrementIsSeparateFromCentPrecisionAndChangesWithoutRecreatingField()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        decimal received = 0;
        var cut = context.Render<SUINumericField<decimal>>(p => p
            .Add(x => x.Value, 360.37m).Add(x => x.Step, "0.01")
            .Add(x => x.WheelStep, 10m).Add(x => x.ChangeOnWheel, true)
            .Add(x => x.ValueChanged, v => received = v));
        Assert.Equal("0.01", cut.Find("input").GetAttribute("step"));
        cut.Find("input").Change("361.48");
        Assert.Equal(361.48m, received);
        cut.Render(p => p.Add(x => x.WheelStep, 1m).Add(x => x.ChangeOnWheel, false));
        Assert.False(cut.Instance.ChangeOnWheel);
        Assert.Equal(1m, cut.Instance.WheelStep);
        Assert.Equal("0.01", cut.Find("input").GetAttribute("step"));
    }
}
