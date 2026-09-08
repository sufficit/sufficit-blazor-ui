using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Deque.AxeCore.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed class InteractionReviewBrowserTests : PageTest
{
    [SetUp]
    public async Task Open()
    {
        var url = Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";
        await Page.GotoAsync(url + "/interaction-review");
        await Expect(Page.Locator("[data-interaction-ready=true]")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Nome para exibição")).ToHaveValueAsync("Equipe Sudeste");
    }

    [Test]
    public async Task ClearingByKeyboardReturnsFocusToTheField()
    {
        var field = Page.GetByLabel("Nome para exibição");
        await field.FocusAsync();
        await field.PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Limpar campo", Exact = true }).PressAsync("Enter");
        await Expect(field).ToHaveValueAsync("");
        await Expect(field).ToBeFocusedAsync();
    }

    [Test]
    public async Task LoadingKeepsGeometryAndLinksNavigate()
    {
        var button = Page.GetByTestId("loading-action");
        var initial = (await button.BoundingBoxAsync())!;
        await button.ClickAsync();
        await Expect(button).ToBeDisabledAsync();
        var busy = (await button.BoundingBoxAsync())!;
        Assert.That(busy.Width, Is.EqualTo(initial.Width).Within(1));
        Assert.That(busy.Height, Is.EqualTo(initial.Height).Within(1));
        await Expect(button).ToHaveAttributeAsync("aria-busy", "true");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Voltar ao início dos exemplos" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("#interaction-patterns$"));
    }

    [TestCase(1440, false)]
    [TestCase(390, true)]
    public async Task NarrowFormsLongActionsAndSemanticTheme(int width, bool dark)
    {
        await Page.SetViewportSizeAsync(width, 950);
        if (dark) await Page.GetByRole(AriaRole.Button, new() { Name = "Alternar tema dos exemplos" }).ClickAsync();
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", dark ? "dark" : "light");
        await Page.EvaluateAsync("Promise.all(document.getAnimations().filter(a => a.effect?.getTiming().iterations !== Infinity).map(a => a.finished.catch(() => {})))");
        var fields = Page.GetByTestId("narrow-form").Locator(".sui-field");
        var first = (await fields.Nth(0).BoundingBoxAsync())!;
        var second = (await fields.Nth(1).BoundingBoxAsync())!;
        if (width == 390)
            Assert.That(await fields.First.Locator("label").EvaluateAsync<string>("el => getComputedStyle(el).minBlockSize"), Is.EqualTo("auto"));
        Assert.That(second.Y - first.Y - first.Height, Is.GreaterThanOrEqualTo(15));
        var semantic = Page.GetByTestId("semantic-action");
        Assert.That(await semantic.EvaluateAsync<string>("el => getComputedStyle(el).color"), Is.EqualTo("rgb(16, 32, 48)"));
        Assert.That(await semantic.Locator("svg").EvaluateAsync<string>("el => getComputedStyle(el).color"), Is.EqualTo("rgb(16, 32, 48)"));
        var longAction = Page.GetByTestId("long-action");
        Assert.That(await longAction.EvaluateAsync<bool>("el => { const a=el.getBoundingClientRect(), b=el.querySelector('.sui-btn__label').getBoundingClientRect(); return b.top>=a.top && b.bottom<=a.bottom; }"), Is.True);
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "review-artifacts");
        Directory.CreateDirectory(dir);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(dir, $"interaction-{width}-{dark}.png"), FullPage = true });
        var axe = await Page.RunAxe();
        File.WriteAllText(Path.Combine(dir, $"interaction-{width}-{dark}.json"), System.Text.Json.JsonSerializer.Serialize(axe.Violations));
        Assert.That(axe.Violations.Where(v => v.Impact is "serious" or "critical").Select(v => v.Id), Is.Empty);
    }

    [TestCase(1440, false)]
    [TestCase(390, true)]
    public async Task ListsEscapeClippingAndRevealKeyboardSelectionWithoutSubmitting(int width, bool reduce)
    {
        await Page.SetViewportSizeAsync(width, 950);
        if (reduce)
        {
            await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
            await Page.GetByRole(AriaRole.Button, new() { Name = "Alternar tema dos exemplos" }).ClickAsync();
            await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "dark");
        }
        var input = Page.GetByRole(AriaRole.Combobox, new() { Name = "Localizar destino" });
        await input.FillAsync("Destino");
        await Expect(Page.GetByRole(AriaRole.Option)).ToHaveCountAsync(30);
        var scroll = await Page.EvaluateAsync<double>("window.scrollY");
        await input.PressAsync("End");
        await Expect(input).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex("-option-29$"));
        var activeId = await input.GetAttributeAsync("aria-activedescendant");
        var option = Page.Locator("#" + activeId);
        await Expect(option).ToContainTextAsync("Destino 30");
        Assert.That(await option.EvaluateAsync<bool>("el => { const r=el.getBoundingClientRect(), list=el.parentElement.getBoundingClientRect(); return r.top>=list.top && r.bottom<=list.bottom+1 && document.elementFromPoint(r.left+10,r.top+10)?.closest('[role=option]')===el; }"), Is.True);
        Assert.That(await Page.EvaluateAsync<double>("window.scrollY"), Is.EqualTo(scroll).Within(1));
        var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "review-artifacts");
        Directory.CreateDirectory(dir);
        await Page.ScreenshotAsync(new() { Path = Path.Combine(dir, $"autocomplete-open-{width}.png") });
        await input.PressAsync("Enter");
        await Expect(input).ToHaveValueAsync("Destino 30 · operação de demonstração");
        await Expect(Page.GetByText("Confirmações: 0", new() { Exact = true })).ToBeVisibleAsync();
        var select = Page.GetByRole(AriaRole.Combobox, new() { Name = "Destino da operação" });
        await select.ClickAsync();
        await select.PressAsync("End");
        await Expect(select).ToHaveAttributeAsync("aria-activedescendant", new System.Text.RegularExpressions.Regex("-option-29$"));
        activeId = await select.GetAttributeAsync("aria-activedescendant");
        // aria-activedescendant renders before OnAfterRenderAsync reveals the option.
        // Wait for that interop effect while preserving the full-visibility contract.
        await Page.WaitForFunctionAsync(
            """
            id => {
                const el = document.getElementById(id);
                if (!el) return false;
                const a = el.getBoundingClientRect(), b = el.parentElement.getBoundingClientRect();
                return a.top >= b.top && a.bottom <= b.bottom + 1;
            }
            """, activeId, new() { Timeout = 5000 });
        await select.PressAsync("Tab");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Depois do seletor" })).ToBeFocusedAsync();
    }
}
