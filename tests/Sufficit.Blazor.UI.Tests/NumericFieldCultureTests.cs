using Bunit;
using Sufficit.Blazor.UI.Components;
using System.Globalization;

namespace Sufficit.Blazor.UI.Tests;

public sealed class NumericFieldCultureTests
{
    [Fact]
    public void NullableDecimal_UsesHtmlInvariantValueAndParsesChanges()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
            using var context = new BunitContext();
            decimal? changed = null;
            var cut = context.Render<SUINumericField<decimal?>>(parameters => parameters
                .Add(component => component.Value, 12.5m)
                .Add(component => component.ValueChanged, value => changed = value));

            var input = cut.Find("input[type=number]");
            Assert.Equal("12.5", input.GetAttribute("value"));

            input.Change("3.75");
            Assert.Equal(3.75m, changed);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }

    [Fact]
    public void NullableInteger_AcceptsAnEmptyValue()
    {
        using var context = new BunitContext();
        int? changed = 7;
        var cut = context.Render<SUINumericField<int?>>(parameters => parameters
            .Add(component => component.Value, 7)
            .Add(component => component.ValueChanged, value => changed = value));

        cut.Find("input[type=number]").Change(string.Empty);

        Assert.Null(changed);
    }
}
