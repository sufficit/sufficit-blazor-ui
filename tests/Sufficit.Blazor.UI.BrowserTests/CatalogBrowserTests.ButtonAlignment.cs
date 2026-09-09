using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class CatalogBrowserTests
{
    [Test]
    public async Task ButtonText_IsOpticallyCentredInsideTheControl()
    {
        var metricsJson = await Page.EvaluateAsync<string>(
            """
            () => JSON.stringify([...document.querySelectorAll('button.sui-btn')]
                .filter(button => {
                    const label = button.querySelector('.sui-btn__label');
                    return label && !label.querySelector('.sui-btn__icon') && label.textContent.trim();
                })
                .map(button => {
                    const label = button.querySelector('.sui-btn__label');
                    const range = document.createRange();
                    range.selectNodeContents(label);
                    const buttonBox = button.getBoundingClientRect();
                    const textBox = range.getClientRects()[0] ?? range.getBoundingClientRect();
                    const style = getComputedStyle(label);
                    const centre = box => box.top + box.height / 2;
                    return {
                        text: label.textContent.trim(),
                        lineHeightRatio: parseFloat(style.lineHeight) / parseFloat(style.fontSize),
                        centreDelta: centre(textBox) - centre(buttonBox),
                    };
                }))
            """);

        using var metrics = JsonDocument.Parse(metricsJson);
        var samples = metrics.RootElement.EnumerateArray().ToArray();
        Assert.That(samples, Is.Not.Empty, "The catalog rendered no text-only SUI buttons.");
        foreach (var sample in samples)
        {
            Assert.That(sample.GetProperty("lineHeightRatio").GetDouble(), Is.EqualTo(1.3).Within(.01),
                $"{sample.GetProperty("text").GetString()} must use the compact button line box.");
            Assert.That(Math.Abs(sample.GetProperty("centreDelta").GetDouble()), Is.LessThanOrEqualTo(.25),
                $"{sample.GetProperty("text").GetString()} is not optically centred inside its button.");
        }
    }

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
