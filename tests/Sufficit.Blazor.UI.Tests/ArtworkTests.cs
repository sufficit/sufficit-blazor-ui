using Bunit;
using Sufficit.Blazor.UI.Artwork;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ArtworkTests
{
    [Fact]
    public void SignalWaves_IsDecorativeAndDrawsOneLeadLine()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIArtwork>(parameters => parameters
            .Add(component => component.Lines, 24));

        var svg = rendered.Find("svg.sui-artwork");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
        Assert.Null(svg.GetAttribute("role"));

        var paths = rendered.FindAll("path");
        Assert.Equal(24, paths.Count);
        Assert.Single(paths, path => path.GetAttribute("stroke")!.Contains("sui-artwork-lead-", StringComparison.Ordinal));
    }

    [Fact]
    public void SameInputs_DrawTheSamePicture_AndSeedChangesIt()
    {
        using var context = new BunitContext();

        string FirstPath(int seed) => context.Render<SUIArtwork>(parameters => parameters
                .Add(component => component.Seed, seed))
            .Find("path").GetAttribute("d")!;

        Assert.Equal(FirstPath(3), FirstPath(3));
        Assert.NotEqual(FirstPath(3), FirstPath(4));
    }

    [Fact]
    public void Instances_UseUniqueGradientIds()
    {
        using var context = new BunitContext();

        var first = context.Render<SUIArtwork>().Find("linearGradient").Id;
        var second = context.Render<SUIArtwork>().Find("linearGradient").Id;

        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(SUIArtworkCalm.Start, "sui-artwork--calm-start")]
    [InlineData(SUIArtworkCalm.Top, "sui-artwork--calm-top")]
    [InlineData(SUIArtworkCalm.Adaptive, "sui-artwork--calm-adaptive")]
    public void Calm_MapsToItsMaskClass(SUIArtworkCalm calm, string expected)
    {
        using var context = new BunitContext();

        var svg = context.Render<SUIArtwork>(parameters => parameters
            .Add(component => component.Calm, calm)
            .Add(component => component.Motion, false)).Find("svg");

        Assert.Contains(expected, svg.ClassList);
        Assert.DoesNotContain("sui-artwork--motion", svg.ClassList);
    }

    [Fact]
    public void OutOfRangeInputs_AreClamped()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIArtwork>(parameters => parameters
            .Add(component => component.Lines, 500)
            .Add(component => component.Peak, 5.0));

        Assert.Equal(64, rendered.FindAll("path").Count);
        Assert.Contains(rendered.FindAll("stop"), stop => stop.GetAttribute("offset") == "0.9");
    }

    [Fact]
    public void GlassCard_AddsTheGlassSurfaceOnlyWhenAsked()
    {
        using var context = new BunitContext();

        var glass = context.Render<SUICard>(parameters => parameters
            .Add(component => component.VariantValue, SUIVariant.Glass)).Find(".sui-card");
        var plain = context.Render<SUICard>().Find(".sui-card");

        Assert.Contains("sui-card--glass", glass.ClassList);
        Assert.DoesNotContain("sui-card--glass", plain.ClassList);
    }
}
