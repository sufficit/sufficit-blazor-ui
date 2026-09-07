using Microsoft.Playwright;
using Deque.AxeCore.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(1440,1000)]
    [TestCase(390,844)]
    public async Task CaptureNewSurfaces(int width,int height)
    {
        await Page.SetViewportSizeAsync(width,height);
        var directory=Path.Combine(TestContext.CurrentContext.WorkDirectory,"review");
        Directory.CreateDirectory(directory);
        foreach(var (query,name) in new[] { ("?component=SUIButton","playground"), ("?view=themes","themes"), ("?view=patterns","patterns") })
        {
            await Page.GotoAsync(BaseUrl+query);
            await Expect(Page.Locator(".site-header")).ToBeVisibleAsync();
            await Page.WaitForFunctionAsync("performance.getEntriesByName('sui-interactive').length > 0");
            if (name == "themes")
            {
                await Expect(Page.GetByLabel("Raio das bordas (px)")).ToBeVisibleAsync();
                await Page.GetByLabel("Abrir comparação interativa").PressAsync("Space");
                await Expect(Page.FrameLocator("iframe[title='Prévia interativa do tema escuro']").GetByLabel("Nome da operação")).ToBeVisibleAsync();
                await Expect(Page.FrameLocator("iframe[title='Prévia interativa do tema claro']").GetByLabel("Nome da operação")).ToBeVisibleAsync();
            }
            else if (name == "playground") await Expect(Page.GetByRole(AriaRole.Combobox, new() { Name="Variante da demonstração",Exact=true })).ToBeVisibleAsync();
            else await Expect(Page.GetByRole(AriaRole.Combobox, new() { Name="Estado da demonstração",Exact=true })).ToBeVisibleAsync();
            await Page.EvaluateAsync("document.fonts.ready");
            await Page.EvaluateAsync("scrollTo(0,0)");
            await Page.EvaluateAsync("Promise.all(document.getAnimations().filter(a => a.effect?.getTiming().iterations !== Infinity).map(a => a.finished.catch(() => {})))");
            await Page.ScreenshotAsync(new() { Path=Path.Combine(directory,$"{width}-{name}.png"),FullPage=true });
            Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"),Is.True);
            var axe=await Page.RunAxe();
            Assert.That(axe.Violations.Where(v=>v.Impact is "critical" or "serious").Select(v=>v.Id),Is.Empty,name);
        }
    }
}
