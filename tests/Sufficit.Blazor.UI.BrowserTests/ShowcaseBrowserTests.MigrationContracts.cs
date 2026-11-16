using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    private Task OpenMigrationExample(string name) => Page.GotoAsync(
        new Uri(new Uri(BaseUrl.TrimEnd('/') + "/"), "?component=" + name).ToString());

    [TestCase(1280)]
    [TestCase(390)]
    public async Task PopupEscapesClippingSupportsInputAndDismissesWithFocusRestoration(int width)
    {
        await Page.SetViewportSizeAsync(width, 844);
        await OpenMigrationExample("SUIPopup");
        var preview = Page.Locator(".component-preview");
        await preview.EvaluateAsync("element => { element.style.overflow = 'hidden'; element.style.height = '60px'; }");
        var trigger = preview.GetByRole(AriaRole.Button, new() { Name = "Pesquisar no menu", Exact = true });
        await trigger.ClickAsync();
        var panel = preview.GetByRole(AriaRole.Dialog, new() { Name = "Pesquisar no menu" });
        await Expect(panel).ToBeVisibleAsync();
        await panel.GetByLabel("Pesquisar", new() { Exact = true }).FillAsync("Telefonia");
        await Expect(panel.GetByLabel("Pesquisar", new() { Exact = true })).ToHaveValueAsync("Telefonia");
        var box = await panel.BoundingBoxAsync();
        Assert.That(box!.X, Is.GreaterThanOrEqualTo(0));
        Assert.That(box.X + box.Width, Is.LessThanOrEqualTo(width));
        await Page.Keyboard.PressAsync("Escape");
        await Expect(panel).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await trigger.ClickAsync();
        await Expect(panel).ToBeVisibleAsync();
        await Page.Locator(".site-header").ClickAsync();
        await Expect(panel).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task RegisteredFormRulesBlockAndThenAllowSubmission()
    {
        await OpenMigrationExample("SUIForm");
        var preview = Page.Locator(".component-preview");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Validar e continuar" }).ClickAsync();
        await Expect(preview.GetByLabel("Nome", new() { Exact = true })).ToHaveAttributeAsync("aria-invalid", "true");
        await Expect(preview.GetByText("Formulário validado.")).Not.ToBeVisibleAsync();
        await preview.GetByLabel("Nome", new() { Exact = true }).FillAsync("Equipe Sufficit");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Validar e continuar" }).ClickAsync();
        await Expect(preview.GetByText("Formulário validado.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ServerTablePaginationReplacesTheCurrentPage()
    {
        await OpenMigrationExample("SUIDataTable");
        var preview = Page.Locator(".component-preview");
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 1", Exact = true })).ToBeVisibleAsync();
        await preview.GetByRole(AriaRole.Button, new() { Name = "Próxima página", Exact = true }).ClickAsync();
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 6", Exact = true })).ToBeVisibleAsync();
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 1", Exact = true })).Not.ToBeVisibleAsync();
    }
    [TestCase(1280)]
    [TestCase(390)]
    public async Task GridGapAndWideDialogRespectControlledGeometry(int width)
    {
        await Page.SetViewportSizeAsync(width, 844);
        await OpenMigrationExample("SUIGrid");
        var preview = Page.Locator(".component-preview");
        var field = preview.GetByLabel("Espaçamento do grid");
        await field.FillAsync("8"); await field.PressAsync("Tab");
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.component-preview .sui-grid')).gap === '32px'");
        await field.FillAsync("0"); await field.PressAsync("Tab");
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.component-preview .sui-grid')).gap === '0px'");
        await OpenMigrationExample("SUIDialogHost");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abrir diálogo", Exact = true }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Editor de demonstração", Exact = true });
        await Expect(dialog).ToBeVisibleAsync();
        var box = await dialog.BoundingBoxAsync();
        Assert.That(box!.Width, Is.EqualTo(Math.Min(width * .92, 960)).Within(2));
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        await Expect(dialog).Not.ToBeVisibleAsync();
        await OpenMigrationExample("SUIDivider");
        var vertical = Page.Locator("[role=separator][aria-orientation=vertical]");
        var separatorBox = await vertical.BoundingBoxAsync();
        Assert.That(separatorBox!.Width, Is.EqualTo(1).Within(1));
        Assert.That(separatorBox.Height, Is.GreaterThan(10));
    }

    [Test]
    public async Task UnknownProgressRemainsVisibleWithReducedMotionWithoutClaimingAPercentage()
    {
        await OpenMigrationExample("SUIProgressLinear");
        var indicator = Page.GetByRole(AriaRole.Progressbar, new() { Name = "Carregando registros" });
        await Expect(indicator).ToBeVisibleAsync();
        Assert.That(await indicator.GetAttributeAsync("aria-valuenow"), Is.Null);
        var bar = indicator.Locator(".sui-progress-linear__bar");
        Assert.That(await bar.EvaluateAsync<string>("el => getComputedStyle(el).animationName"), Is.EqualTo("sui-progress-indeterminate"));
        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.sui-progress-linear--indeterminate .sui-progress-linear__bar')).animationName === 'none'");
        var width = (await bar.BoundingBoxAsync())!.Width;
        Assert.That(width, Is.GreaterThan(0));
        Assert.That(width, Is.LessThan((await indicator.BoundingBoxAsync())!.Width));
    }

    [Test]
    public async Task NativeDateClockAndRangeRoundTripInBrazilianCulture()
    {
        await using var context = await Browser.NewContextAsync(new() { Locale = "pt-BR", TimezoneId = "America/Sao_Paulo" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(new Uri(BaseUrl.TrimEnd('/') + "/"), "?component=SUITextField").ToString());
        var date = page.GetByLabel("Data editável", new() { Exact = true });
        await Expect(date).ToHaveValueAsync("2026-08-01");
        await date.FillAsync("2026-08-02"); await date.PressAsync("Tab");
        var clock = page.GetByLabel("Horário editável", new() { Exact = true });
        await clock.FillAsync("10:45:12.250"); await clock.PressAsync("Tab");
        await page.GetByLabel("Fração", new() { Exact = true }).EvaluateAsync("el => { el.value = '0.75'; el.dispatchEvent(new Event('input', { bubbles: true })); }");
        await Expect(page.GetByTestId("native-temporal-values")).ToHaveTextAsync("2026-08-02|10:45:12.2500000|0.75");
        await Expect(date).ToHaveValueAsync("2026-08-02");
        await Expect(clock).ToHaveValueAsync("10:45:12.25");
    }

}
