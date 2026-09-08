using Microsoft.Playwright;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase("SUIButton", 1440)]
    [TestCase("SUIIconButton", 1440)]
    [TestCase("SUIChip", 1440)]
    [TestCase("SUIButton", 390)]
    [TestCase("SUIIconButton", 390)]
    [TestCase("SUIChip", 390)]
    public async Task PlaygroundSizesChangeRenderedDimensions(string component, int width)
    {
        await Page.SetViewportSizeAsync(width, 900);
        await Page.GotoAsync(BaseUrl + "?component=" + component);
        var result = Page.Locator(component == "SUIChip" ? ".playground-result .sui-chip" : ".playground-result button");
        var sizes = new List<(float Width, float Height, float Icon, float Font)>();
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "sizes");
        Directory.CreateDirectory(directory);
        foreach (var (label, value) in new[] { ("Pequeno", "Small"), ("Médio", "Medium"), ("Grande", "Large") })
        {
            await ChooseAsync("Tamanho da demonstração", label);
            await Expect(result).ToHaveClassAsync(new Regex("--" + value.ToLowerInvariant() + "(?: |$)"));
            await Expect(Page.Locator(".playground code")).ToContainTextAsync("SUISize." + value);
            var box = (await result.BoundingBoxAsync())!;
            var icon = component != "SUIChip" ? (await result.Locator("svg").BoundingBoxAsync())!.Width : 0;
            var font = await result.EvaluateAsync<float>("el => parseFloat(getComputedStyle(el).fontSize)");
            sizes.Add((box.Width, box.Height, icon, font));
            if (component == "SUIIconButton") Assert.That(box.Width, Is.EqualTo(box.Height).Within(1), "Icon buttons remain square");
            if (width == 390 && component != "SUIChip") Assert.That(box.Height, Is.GreaterThanOrEqualTo(44), "Touch target");
            await Page.Locator(".playground").ScreenshotAsync(new() { Path = Path.Combine(directory, $"{component}-{width}-{value}.png") });
        }
        if (width == 1440 || component == "SUIChip")
        {
            Assert.That(sizes[0].Height, Is.LessThan(sizes[1].Height));
            Assert.That(sizes[1].Height, Is.LessThan(sizes[2].Height));
        }
        if (component != "SUIChip")
        {
            Assert.That(sizes[0].Icon, Is.LessThan(sizes[1].Icon));
            Assert.That(sizes[2].Icon, Is.EqualTo(sizes[1].Icon).Within(.1), "Large grows the target, not the glyph");
            Assert.That(sizes[2].Font, Is.EqualTo(sizes[1].Font), "Large keeps the button type scale");
            Assert.That(sizes[2].Icon, Is.LessThanOrEqualTo(sizes[2].Font * 1.5f));
        }
        if (component != "SUIChip")
        {
            await Page.GetByLabel("Desabilitado", new() { Exact = true }).PressAsync("Space");
            await Expect(result).ToBeDisabledAsync();
            var disabled = (await result.BoundingBoxAsync())!;
            Assert.That(disabled.Width, Is.EqualTo(sizes[2].Width).Within(1));
            Assert.That(disabled.Height, Is.EqualTo(sizes[2].Height).Within(1));
        }
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        var measurements = sizes.Select((size, index) => new { index, size.Width, size.Height, size.Icon, size.Font });
        File.WriteAllText(Path.Combine(directory, $"{component}-{width}.json"), JsonSerializer.Serialize(measurements));
    }
}
