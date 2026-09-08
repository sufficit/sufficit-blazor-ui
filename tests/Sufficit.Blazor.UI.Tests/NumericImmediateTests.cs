using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class NumericImmediateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ImmediateChoosesOneEventWithoutDuplicatingTheCommit(bool immediate)
    {
        using var context = new BunitContext();
        var changes = new List<int>();
        var cut = context.Render<SUINumericField<int>>(p => p
            .Add(x => x.Immediate, immediate).Add(x => x.Value, 4)
            .Add(x => x.ValueChanged, value => changes.Add(value)));
        var input = cut.Find("input");
        input.Input("5");
        Assert.Equal(immediate ? 1 : 0, changes.Count);
        input.Change("5");
        Assert.Equal(new[] { 5 }, changes);
    }

    [Fact]
    public void ImmediateRejectsInvalidNumbersAndDisabledEvents()
    {
        using var context = new BunitContext();
        var changes = new List<int>();
        var cut = context.Render<SUINumericField<int>>(p => p
            .Add(x => x.Immediate, true).Add(x => x.Value, 4)
            .Add(x => x.ValueChanged, value => changes.Add(value)));
        cut.Find("input").Input("not-a-number");
        Assert.Empty(changes);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        cut.Find("input").Input("6");
        Assert.Equal(new[] { 6 }, changes);
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
        cut.Render(p => p.Add(x => x.Disabled, true));
        cut.Find("input").Input("7");
        Assert.Equal(new[] { 6 }, changes);
    }

    [Fact]
    public void ImmediateSupportsNullableDecimalAndClearing()
    {
        using var context = new BunitContext();
        var values = new List<decimal?>();
        var cut = context.Render<SUINumericField<decimal?>>(p => p
            .Add(x => x.Immediate, true)
            .Add(x => x.ValueChanged, value => values.Add(value)));
        cut.Find("input").Input("3.75");
        cut.Find("input").Input("");
        Assert.Equal(new decimal?[] { 3.75m, null }, values);
    }
}
