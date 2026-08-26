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
    public void Clock_icon_is_an_outline_and_pending_reuses_the_same_glyph()
    {
        Assert.Equal(SUIIcons.Clock, SUIIcons.Pending);
        Assert.Contains("<circle", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("fill=\"none\"", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("stroke=\"currentColor\"", SUIIcons.Clock, StringComparison.Ordinal);
        Assert.Contains("M12 7v5l3 2", SUIIcons.Clock, StringComparison.Ordinal);
    }
}
