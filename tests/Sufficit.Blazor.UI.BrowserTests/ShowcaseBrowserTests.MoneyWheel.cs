using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(1280)]
    [TestCase(390)]
    public async Task MoneyWheelPreviewUpdatesLiveAndPreservesCents(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.GotoAsync(BaseUrl + "?component=SUINumericField");
        var input = Page.GetByLabel("Prévia monetária", new() { Exact = true });
        await input.ClickAsync();
        await input.HoverAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("360.37");
        await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir rolagem no valor" }).ClickAsync();
        await input.ClickAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("361.37");
        await Page.Mouse.WheelAsync(0, 100);
        await Expect(input).ToHaveValueAsync("360.37");
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Incremento do mouse e das setas" }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "R$ 10,00", Exact = true }).ClickAsync();
        await input.ClickAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("370.37");
        await input.FillAsync("51.23");
        await Page.Mouse.WheelAsync(0, 100);
        await Expect(input).ToHaveValueAsync("50");
        await input.FillAsync("2499.99");
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("2500");
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Incremento do mouse e das setas" }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "R$ 0,01", Exact = true }).ClickAsync();
        await input.FillAsync("360");
        await input.ClickAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("360.01");
        await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir rolagem no valor" }).ClickAsync();
        await input.ClickAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync("360.01");
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
    }

    [TestCase(1280, false, false)]
    [TestCase(1280, false, true)]
    [TestCase(1280, true, false)]
    [TestCase(1280, true, true)]
    [TestCase(390, false, false)]
    [TestCase(390, false, true)]
    [TestCase(390, true, false)]
    [TestCase(390, true, true)]
    public async Task MouseAndArrowsAreIndependent(int width, bool wheel, bool arrows)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.GotoAsync(BaseUrl + "?component=SUINumericField");
        if (wheel) await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir rolagem no valor" }).ClickAsync();
        if (!arrows) await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir setas no valor" }).ClickAsync();
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Incremento do mouse e das setas" }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "R$ 10,00", Exact = true }).ClickAsync();
        var input = Page.GetByLabel("Prévia monetária", new() { Exact = true });
        await input.FillAsync("360.37");
        // Clickable arrows remain available in all four mouse/keyboard combinations.
        var up = Page.GetByRole(AriaRole.Button, new() { Name = "Aumentar valor", Exact = true });
        var down = Page.GetByRole(AriaRole.Button, new() { Name = "Diminuir valor", Exact = true });
        await up.ClickAsync();
        await Expect(input).ToHaveValueAsync("370.37");
        await down.ClickAsync();
        await Expect(input).ToHaveValueAsync("360.37");
        Assert.That(await input.EvaluateAsync<bool>("e => e.validity.valid"), Is.True);
        await input.PressAsync("ArrowUp");
        await Expect(input).ToHaveValueAsync(arrows ? "370.37" : "360.37");
        await input.PressAsync("ArrowDown");
        await Expect(input).ToHaveValueAsync("360.37");
        await input.HoverAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync(wheel ? "370.37" : "360.37");
        await Page.Mouse.WheelAsync(0, 100);
        await Expect(input).ToHaveValueAsync("360.37");
        // Change only the keyboard setting on this same element, then confirm mouse state is retained.
        await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir setas no valor" }).ClickAsync();
        await input.FillAsync("2499.99");
        await input.PressAsync("ArrowUp");
        await Expect(input).ToHaveValueAsync(!arrows ? "2500" : "2499.99");
        await input.FillAsync("50.01");
        await input.PressAsync("ArrowDown");
        await Expect(input).ToHaveValueAsync(!arrows ? "50" : "50.01");
        await input.FillAsync("360.37");
        await input.HoverAsync();
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(input).ToHaveValueAsync(wheel ? "370.37" : "360.37");
        await input.FillAsync("123.45");
        await Expect(input).ToHaveValueAsync("123.45");
    }

    [TestCase(1280)]
    [TestCase(390)]
    public async Task ClickableArrowsUseLiveStepAndRespectLimitsAndDisabledFields(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.GotoAsync(BaseUrl + "?component=SUINumericField");
        await Page.Locator("label.sui-switch").Filter(new() { HasText = "Permitir setas no valor" }).ClickAsync();
        var input = Page.GetByLabel("Prévia monetária", new() { Exact = true });
        var up = Page.GetByRole(AriaRole.Button, new() { Name = "Aumentar valor", Exact = true });
        var down = Page.GetByRole(AriaRole.Button, new() { Name = "Diminuir valor", Exact = true });
        foreach (var step in new[] { ("R$ 0,01", "50.08"), ("R$ 1,00", "51.07"), ("R$ 10,00", "60.07") })
        {
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Incremento do mouse e das setas" }).ClickAsync();
            await Page.GetByRole(AriaRole.Option, new() { Name = step.Item1, Exact = true }).ClickAsync();
            await input.FillAsync("50.07");
            await up.ClickAsync();
            await Expect(input).ToHaveValueAsync(step.Item2);
            await down.ClickAsync();
            await Expect(input).ToHaveValueAsync("50.07");
        }
        await input.FillAsync("2499.99");
        await up.ClickAsync();
        await Expect(input).ToHaveValueAsync("2500");
        await input.FillAsync("50.07");
        await down.ClickAsync();
        await Expect(input).ToHaveValueAsync("50");
        await input.FillAsync("360.37");
        await up.FocusAsync();
        await up.PressAsync("Enter");
        await Expect(input).ToHaveValueAsync("370.37");
        await down.FocusAsync();
        await down.PressAsync("Space");
        await Expect(input).ToHaveValueAsync("360.37");
        await input.EvaluateAsync("e => e.readOnly = true");
        await up.ClickAsync();
        await Expect(input).ToHaveValueAsync("360.37");
        await input.EvaluateAsync("e => { e.readOnly = false; e.disabled = true; }");
        await up.ClickAsync();
        await Expect(input).ToHaveValueAsync("360.37");
        await input.EvaluateAsync("e => e.disabled = false");
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        await Page.Locator(".component-preview").ScreenshotAsync(new() { Path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts", $"numeric-spinner-{width}.png") });
    }
}
