using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ActionIconQualityTests
{
    public static TheoryData<string> RefinedActionIcons => new()
    {
        SUIIcons.Delete,
        SUIIcons.Edit,
        SUIIcons.Storage,
        SUIIcons.Unlink,
        SUIIcons.Save,
        SUIIcons.Restart,
        SUIIcons.Shield,
        SUIIcons.Bolt,
        SUIIcons.Devices,
    };

    [Theory]
    [MemberData(nameof(RefinedActionIcons))]
    public void Action_icons_use_the_shared_lightweight_outline(string icon)
    {
        Assert.Contains("fill=\"none\"", icon, StringComparison.Ordinal);
        Assert.Contains("stroke=\"currentColor\"", icon, StringComparison.Ordinal);
        Assert.Contains("stroke-width=\"1.75\"", icon, StringComparison.Ordinal);
        Assert.Contains("stroke-linecap=\"round\"", icon, StringComparison.Ordinal);
        Assert.Contains("stroke-linejoin=\"round\"", icon, StringComparison.Ordinal);
    }

    [Fact]
    public void Shield_bolt_and_devices_are_three_distinct_glyphs()
    {
        Assert.Contains("m9.25 12 2 2 3.75-4", SUIIcons.Shield, StringComparison.Ordinal);
        Assert.Contains("M13.25 2.75", SUIIcons.Bolt, StringComparison.Ordinal);
        Assert.Contains("<rect", SUIIcons.Devices, StringComparison.Ordinal);
        Assert.NotEqual(SUIIcons.Shield, SUIIcons.Admin);
        Assert.NotEqual(SUIIcons.Devices, SUIIcons.Phone);
        Assert.NotEqual(SUIIcons.Bolt, SUIIcons.Shield);
    }

    [Fact]
    public void Clock_icon_is_an_outline_and_pending_reuses_the_same_glyph()
    {
        Assert.Equal(SUIIcons.Clock, SUIIcons.Pending);
        Assert.Contains("<circle", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("fill=\"none\"", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("stroke=\"currentColor\"", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("M12 7v5l3 2", SUIIcons.Clock, StringComparison.Ordinal);
    }
}
