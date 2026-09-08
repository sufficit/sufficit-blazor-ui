using Microsoft.Playwright;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(1440, "Claro")]
    [TestCase(390, "Escuro")]
    public async Task SoftButtonsKeepContrastAlignmentAndKeyboardActions(int width, string theme)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        foreach (var palette in new[] { "Sufficit · Âmbar", "Azul", "Vermelho" })
        {
            await Page.GotoAsync(BaseUrl + "?view=themes");
            await ChooseAsync("Tema", theme);
            await ChooseAsync("Paleta de demonstração", palette);
            await Page.GotoAsync(BaseUrl + "?component=SUIButton");
            await ChooseAsync("Variante da demonstração", "Suave");
            var button = Page.Locator(".playground-result button.sui-btn");
            foreach (var size in new[] { "Pequeno", "Médio", "Grande" })
            {
                await ChooseAsync("Tamanho da demonstração", size);
                await Page.Mouse.MoveAsync(0, 0);
                var normal = await ButtonAppearanceAsync(button);
                Assert.That(normal.GetProperty("contrast").GetDouble(), Is.GreaterThanOrEqualTo(4.5), palette);
                Assert.That(normal.GetProperty("delta").GetDouble(), Is.LessThanOrEqualTo(.1), size);
                await button.HoverAsync();
                var hover = await ButtonAppearanceAsync(button);
                Assert.That(hover.GetProperty("contrast").GetDouble(), Is.GreaterThanOrEqualTo(4.5), palette + " hover");
                await Page.Mouse.DownAsync();
                var pressed = await ButtonAppearanceAsync(button);
                await Page.Mouse.UpAsync();
                Assert.That(pressed.GetProperty("contrast").GetDouble(), Is.GreaterThanOrEqualTo(4.5), palette + " pressed");
                Assert.That(normal.GetProperty("background").GetString(), Is.Not.EqualTo(hover.GetProperty("background").GetString()));
                Assert.That(hover.GetProperty("background").GetString(), Is.Not.EqualTo(pressed.GetProperty("background").GetString()));
            }
            await Page.Keyboard.PressAsync("Tab");
            await button.FocusAsync();
            Assert.That(await button.EvaluateAsync<bool>("el => el.matches(':focus-visible') && getComputedStyle(el).outlineStyle !== 'none'"), Is.True);
            await button.PressAsync("Enter");
            await Expect(Page.Locator(".playground-result [role=status]")).ToHaveTextAsync("Ações realizadas: 4");
            await Expect(Page.Locator(".playground code")).ToContainTextAsync("SUIVariant.Soft");
            await Page.GetByLabel("Desabilitado", new() { Exact = true }).PressAsync("Space");
            await Expect(button).ToBeDisabledAsync();
            var disabled = await ButtonAppearanceAsync(button);
            await button.HoverAsync(new() { Force = true });
            Assert.That((await ButtonAppearanceAsync(button)).GetProperty("background").GetString(), Is.EqualTo(disabled.GetProperty("background").GetString()));
            Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        }
    }

    private static Task<JsonElement> ButtonAppearanceAsync(ILocator button) => button.EvaluateAsync<JsonElement>(
        """
        el => {
            const style = getComputedStyle(el), label = el.querySelector('.sui-btn__label');
            const centre = node => { const r = node.getBoundingClientRect(); return r.top + r.height / 2; };
            const canvas = document.createElement('canvas'); canvas.width = canvas.height = 1;
            const ctx = canvas.getContext('2d');
            const luminance = color => {
                ctx.clearRect(0,0,1,1); ctx.fillStyle = color; ctx.fillRect(0,0,1,1);
                const rgb = [...ctx.getImageData(0,0,1,1).data].slice(0,3).map(v => { v /= 255; return v <= .04045 ? v / 12.92 : ((v+.055)/1.055)**2.4; });
                return rgb[0]*.2126 + rgb[1]*.7152 + rgb[2]*.0722;
            };
            const a = luminance(style.color), b = luminance(style.backgroundColor);
            return { contrast: (Math.max(a,b)+.05)/(Math.min(a,b)+.05), background: style.backgroundColor,
                delta: Math.max(...[...el.querySelectorAll('.sui-btn__icon')].map(icon => Math.abs(centre(icon)-centre(label)))) };
        }
        """);
}
