using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class IconRenderingTests
{
    [Fact]
    public void EyeOff_RendersItsDistinctVisibilityGlyph()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, "eye-off"));

        Assert.Contains("M3 3l18 18", rendered.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("cx=\"12\" cy=\"12\" r=\"9\"", rendered.Markup, StringComparison.Ordinal);
    }
}
