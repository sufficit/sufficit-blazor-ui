using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class CatalogBrowserTests
{
    [Test]
    public async Task ButtonIcon_ReceivesOpticalOffsetWithoutMovingTheLabel()
    {
        var metricsJson = await Page.EvaluateAsync<string>(
            """
            () => {
                const icon = document.querySelector('.sui-btn__icon');
                const label = icon?.closest('.sui-btn__label');
                if (!icon || !label) return JSON.stringify({ found: false });

                const iconBox = icon.getBoundingClientRect();
                const unshiftedIconCentre = iconBox.top + iconBox.height / 2 - 1;
                const labelBox = label.getBoundingClientRect();
                return JSON.stringify({
                    found: true,
                    iconTransform: getComputedStyle(icon).transform,
                    labelTransform: getComputedStyle(label).transform,
                    centreDelta: unshiftedIconCentre - (labelBox.top + labelBox.height / 2),
                });
            }
            """);

        using var metrics = JsonDocument.Parse(metricsJson);
        var root = metrics.RootElement;
        Assert.That(root.GetProperty("found").GetBoolean(), Is.True,
            "The catalog rendered no SUI button with an icon.");
        Assert.That(root.GetProperty("labelTransform").GetString(), Is.EqualTo("none"));
        Assert.That(root.GetProperty("iconTransform").GetString(),
            Does.Match(@"^matrix\(1, 0, 0, 1, 0, 1\)$"));
        Assert.That(Math.Abs(root.GetProperty("centreDelta").GetDouble()),
            Is.LessThanOrEqualTo(.1),
            "The icon frame was not geometrically centred before its optical offset.");
    }
}
