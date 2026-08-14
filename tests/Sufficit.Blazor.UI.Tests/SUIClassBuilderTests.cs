using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Tests;

public sealed class SUIClassBuilderTests
{
    [Fact]
    public void Build_TrimsAndSkipsConditionalClasses()
    {
        var result = SUIClassBuilder.Default(" root ")
            .AddClass("enabled", true)
            .AddClass("ignored", false)
            .AddClass("  custom  ")
            .Build();

        Assert.Equal("root enabled custom", result);
    }

    [Theory]
    [InlineData(SUIColor.Primary, "primary")]
    [InlineData(SUIVariant.Outlined, "outlined")]
    [InlineData(null, "")]
    public void Slug_NormalizesEnumLikeValues(object? value, string expected)
        => Assert.Equal(expected, SUIClassBuilder.Slug(value));
}
