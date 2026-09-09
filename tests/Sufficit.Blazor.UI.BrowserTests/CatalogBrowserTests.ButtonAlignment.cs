using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class CatalogBrowserTests
{
    [Test]
    public async Task ButtonIcon_AlignsWithTheLabelWithoutAnArtificialOffset()
    {
        var metricsJson = await Page.EvaluateAsync<string>(
            """
            () => {
                const icon = document.querySelector('.sui-btn__icon');
                const label = icon?.closest('.sui-btn__label');
                if (!icon || !label) return JSON.stringify({ found: false });

                const iconBox = icon.getBoundingClientRect();
                const iconCentre = iconBox.top + iconBox.height / 2;
                const labelBox = label.getBoundingClientRect();
                return JSON.stringify({
                    found: true,
                    iconTransform: getComputedStyle(icon).transform,
                    labelTransform: getComputedStyle(label).transform,
                    centreDelta: iconCentre - (labelBox.top + labelBox.height / 2),
                });
            }
            """);

        using var metrics = JsonDocument.Parse(metricsJson);
        var root = metrics.RootElement;
        Assert.That(root.GetProperty("found").GetBoolean(), Is.True,
            "The catalog rendered no SUI button with an icon.");
        Assert.That(root.GetProperty("labelTransform").GetString(), Is.EqualTo("none"));
        Assert.That(root.GetProperty("iconTransform").GetString(),
            Is.EqualTo("none"));
        Assert.That(Math.Abs(root.GetProperty("centreDelta").GetDouble()),
            Is.LessThanOrEqualTo(.1),
            "The icon frame must share the vertical centre of the label.");
    }
}
