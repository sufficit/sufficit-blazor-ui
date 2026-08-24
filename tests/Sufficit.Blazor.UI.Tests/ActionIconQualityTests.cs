using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ActionIconQualityTests
{
    public static TheoryData<string> RefinedActionIcons => new()
    {
        SUIIcons.Delete,
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
}
