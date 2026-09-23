using System.Globalization;
using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Number inputs are dot-decimal only, whatever the user culture is: a pt-BR
/// user typing 0.40 must get forty cents, not forty reais, and the field must
/// never send the browser a value with a comma.
/// </summary>
public sealed class TextFieldNumberCultureTests
{
    [Fact]
    public async Task DotDecimalFractionsSurviveACommaCulture()
    {
        using var culture = CultureOverride(new CultureInfo("pt-BR"));
        using var context = new BunitContext();

        decimal? received = null;
        var field = context.Render<SUITextField<decimal>>(p => p
            .Add(x => x.InputType, "number")
            .Add(x => x.Value, 0m)
            .Add(x => x.ValueChanged, v => received = v));

        // Immediate off: the change event is what carries the value.
        await field.InvokeAsync(() => field.Find("input").Change("0.40"));

        Assert.Equal(0.40m, received);
    }

    [Fact]
    public void TheRenderedValueNeverCarriesAComma()
    {
        using var culture = CultureOverride(new CultureInfo("pt-BR"));
        using var context = new BunitContext();

        var field = context.Render<SUITextField<decimal>>(p => p
            .Add(x => x.InputType, "number")
            .Add(x => x.Value, 0.40m));

        var attribute = field.Find("input").GetAttribute("value");
        Assert.Equal("0.40", attribute);
    }

    [Fact]
    public void TextFieldsKeepUsingTheUserCulture()
    {
        // The invariant treatment is for numeric inputs only: a plain text
        // field must keep rendering 1.5m as "1,5" for a pt-BR user, the same
        // way it did before.
        using var culture = CultureOverride(new CultureInfo("pt-BR"));
        using var context = new BunitContext();

        var field = context.Render<SUITextField<decimal>>(p => p
            .Add(x => x.InputType, "text")
            .Add(x => x.Value, 1.5m));

        Assert.Equal("1,5", field.Find("input").GetAttribute("value"));
    }

    private static CultureOverrideScope CultureOverride(CultureInfo culture)
        => new(culture);

    private sealed class CultureOverrideScope : IDisposable
    {
        private readonly CultureInfo _previous;

        public CultureOverrideScope(CultureInfo culture)
        {
            _previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = culture;
        }

        public void Dispose() => CultureInfo.CurrentCulture = _previous;
    }
}
