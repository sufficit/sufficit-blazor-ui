using Bunit;
using Sufficit.Blazor.UI.Components;
using System.Globalization;

namespace Sufficit.Blazor.UI.Tests;

public sealed class NativeTemporalInputTests
{
    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    public void CivilDatesRoundTripWithoutCultureOrTimezoneConversion(string culture)
    {
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            using var context = new BunitContext();
            DateTime? changed = null;
            var field = context.Render<SUITextField<DateTime?>>(p => p.Add(x => x.InputType, "date")
                .Add(x => x.Value, new DateTime(2026, 8, 1, 23, 30, 0, DateTimeKind.Utc))
                .Add(x => x.ValueChanged, value => changed = value));
            Assert.Equal("2026-08-01", field.Find("input").GetAttribute("value"));
            field.Find("input").Change("2026-08-02");
            Assert.Equal(new DateTime(2026, 8, 2), changed);
            Assert.Equal(DateTimeKind.Unspecified, changed!.Value.Kind);
            field.Find("input").Change("2026-02-30");
            Assert.Equal(new DateTime(2026, 8, 2), changed);
            Assert.Equal("true", field.Find("input").GetAttribute("aria-invalid"));
            field.Find("input").Change("");
            Assert.Null(changed);
        }
        finally { CultureInfo.CurrentCulture = prior; }
    }

    [Fact]
    public void NullableClockPreservesMillisecondsAndRejectsInvalidHours()
    {
        using var context = new BunitContext();
        TimeSpan? changed = null;
        var field = context.Render<SUITextField<TimeSpan?>>(p => p.Add(x => x.InputType, "time")
            .Add(x => x.Value, new TimeSpan(0, 9, 30, 0, 125)).Add(x => x.ValueChanged, value => changed = value));
        Assert.Equal("09:30:00.125", field.Find("input").GetAttribute("value"));
        field.Find("input").Change("10:45:12.250");
        Assert.Equal(new TimeSpan(0, 10, 45, 12, 250), changed);
        field.Find("input").Change("25:10");
        Assert.Equal(new TimeSpan(0, 10, 45, 12, 250), changed);
        Assert.Equal("true", field.Find("input").GetAttribute("aria-invalid"));
        field.Find("input").Change("");
        Assert.Null(changed);
    }

    [Fact]
    public void NativeRangeUsesInvariantDecimalsDuringImmediateInput()
    {
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            using var context = new BunitContext();
            decimal changed = 0;
            var field = context.Render<SUITextField<decimal>>(p => p.Add(x => x.InputType, "range")
                .Add(x => x.Value, .4m).Add(x => x.Immediate, true).Add(x => x.ValueChanged, value => changed = value));
            Assert.Equal("0.4", field.Find("input").GetAttribute("value"));
            field.Find("input").Input("0.75");
            Assert.Equal(.75m, changed);
        }
        finally { CultureInfo.CurrentCulture = prior; }
    }
}
