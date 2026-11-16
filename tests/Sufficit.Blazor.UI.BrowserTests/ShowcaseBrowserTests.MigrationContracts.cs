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
}
