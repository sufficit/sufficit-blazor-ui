using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [Test]
    public async Task TemporaryDrawerIncludesEditableFieldsInItsFocusLoop()
    {
        await OpenMigrationExample("SUIDrawer");
        var opener = Page.GetByRole(AriaRole.Button, new() { Name = "Abrir navegação temporária", Exact = true });
        await opener.ClickAsync();
        var drawer = Page.GetByRole(AriaRole.Dialog, new() { Name = "Navegação de demonstração", Exact = true });
        var close = drawer.GetByRole(AriaRole.Button, new() { Name = "Fechar navegação", Exact = true });
        var last = drawer.GetByLabel("Último campo da navegação", new() { Exact = true });
        await last.FocusAsync(); await Page.Keyboard.PressAsync("Tab");
        await Expect(close).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(last).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(drawer).Not.ToBeVisibleAsync();
        await Expect(opener).ToBeFocusedAsync();
    }

    [Test]
    public async Task EscapingNestedDialogPreservesTheUnderlyingDrawer()
    {
        await OpenMigrationExample("SUIDrawer");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abrir navegação temporária", Exact = true }).ClickAsync();
        var drawer = Page.GetByRole(AriaRole.Dialog, new() { Name = "Navegação de demonstração", Exact = true });
        var trigger = drawer.GetByRole(AriaRole.Button, new() { Name = "Confirmar no diálogo", Exact = true });
        await trigger.ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Confirmação sobre a navegação", Exact = true });
        await Expect(dialog).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).Not.ToBeVisibleAsync();
        await Expect(drawer).ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(drawer).Not.ToBeVisibleAsync();
    }
    [Test]
    public async Task EscapingInteractivePopoverPreservesTheUnderlyingDrawer()
    {
        await OpenMigrationExample("SUIDrawer");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abrir navegação temporária", Exact = true }).ClickAsync();
        var drawer = Page.GetByRole(AriaRole.Dialog, new() { Name = "Navegação de demonstração", Exact = true });
        var trigger = drawer.GetByRole(AriaRole.Button, new() { Name = "Abrir pesquisa sobre o menu", Exact = true });
        await trigger.ClickAsync();
        var popup = Page.GetByRole(AriaRole.Dialog, new() { Name = "Pesquisa sobre a navegação", Exact = true });
        await popup.GetByLabel("Pesquisar em destaque", new() { Exact = true }).FillAsync("Áudios");
        await Page.Keyboard.PressAsync("Escape");
        await Expect(popup).Not.ToBeVisibleAsync();
        await Expect(drawer).ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(drawer).Not.ToBeVisibleAsync();
    }

}
