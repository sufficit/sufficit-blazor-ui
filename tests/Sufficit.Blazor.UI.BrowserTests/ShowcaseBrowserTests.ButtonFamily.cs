using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(1440, "Claro")]
    [TestCase(390, "Escuro")]
    public async Task ButtonFamilyKeepsGeometryAndDistinctPressedFeedback(int width, string theme)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await ChooseAsync("Tema", theme);
        string? actionColor = null;
        foreach (var component in new[] { "SUIButton", "SUIIconButton" })
        {
            await Page.GotoAsync(BaseUrl + "?component=" + component);
            var button = Page.Locator(".playground-result button.sui-btn");
            float? restingWidth = null;
            foreach (var variant in new[] { "Preenchida", "Suave", "Contorno", "Texto" })
            {
                await ChooseAsync("Variante da demonstração", variant);
                await Page.Mouse.MoveAsync(0, 0);
                var box = (await button.BoundingBoxAsync())!;
                restingWidth ??= box.Width;
                Assert.That(box.Width, Is.EqualTo(restingWidth.Value).Within(.1), "Variant must not change content width");
                Assert.That(await button.EvaluateAsync<string>("el => getComputedStyle(el).borderRadius"), Is.EqualTo("10px"));
                Assert.That(await button.EvaluateAsync<string>("el => getComputedStyle(el).fontWeight"), Is.EqualTo("500"));
                if (width == 390) Assert.That(box.Height, Is.GreaterThanOrEqualTo(44));
                if (variant == "Preenchida")
                {
                    var color = await button.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
                    actionColor ??= color;
                    Assert.That(color, Is.EqualTo(actionColor), "Equivalent create actions use the same primary color");
                }
                await button.HoverAsync();
                var hover = await button.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
                await Page.Mouse.DownAsync();
                var pressed = await button.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
                await Page.Mouse.UpAsync();
                Assert.That(pressed, Is.Not.EqualTo(hover), variant + " needs distinct pressed feedback");
            }
            await Expect(Page.Locator(".playground-result [role=status]")).ToHaveTextAsync("Ações realizadas: 4");
            if (component == "SUIIconButton")
                await Expect(Page.Locator(".playground code")).ToContainTextAsync("ColorValue=\"SUIColor.Primary\"");
        }
    }
}
