using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed class ContextMenuBrowserTests : PageTest
{
    [Test]
    public async Task PointerAndKeyboardMenusStayInViewportAndRestoreFocus()
    {
        const string relativePath = "src/Components/Overlays/SUIContextMenu.razor.js";
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, relativePath))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        var source = await File.ReadAllTextAsync(Path.Combine(root!.FullName, relativePath));
        await Page.SetViewportSizeAsync(380, 260);
        await Page.SetContentAsync("""
            <div style="overflow:hidden;height:40px">
              <button id="anchor">Open menu</button>
              <div id="panel" role="menu" popover="auto" tabindex="-1" style="position:fixed;inset:auto;margin:0;width:180px">
                <button id="one" role="menuitem">One</button>
                <button role="menuitem" disabled>Disabled</button>
                <button id="two" role="menuitem">Two</button>
              </div>
            </div>
            <button id="outside">Outside</button>
            """);
        await Page.EvaluateAsync("""
            async source => {
                window.contextMenu = await import('data:text/javascript;base64,' + btoa(source));
                window.dismissed = [];
                const anchor = document.querySelector('#anchor');
                const panel = document.querySelector('#panel');
                const target = { invokeMethodAsync: (_, generation) => { dismissed.push(generation); return Promise.resolve(); } };
                window.openAt = (x, y, generation) => contextMenu.open(panel, anchor.id, x, y, target, generation);
                anchor.addEventListener('contextmenu', e => { e.preventDefault(); openAt(e.clientX, e.clientY, 1); });
                anchor.addEventListener('keydown', e => {
                    if (e.shiftKey && e.key === 'F10') {
                        e.preventDefault();
                        openAt(null, null, 2);
                    }
                });
            }
            """, source);
        var anchor = Page.Locator("#anchor");
        var panel = Page.Locator("#panel");
        await anchor.ClickAsync(new() { Button = Microsoft.Playwright.MouseButton.Right });
        await Expect(Page.Locator("#one")).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.Locator("#two")).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Escape");
        await Expect(panel).ToBeHiddenAsync();
        await Expect(anchor).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Shift+F10");
        await Expect(panel).ToBeVisibleAsync();
        var keyboardBounds = await panel.BoundingBoxAsync();
        var anchorBounds = await anchor.BoundingBoxAsync();
        Assert.That(keyboardBounds!.Y, Is.EqualTo(anchorBounds!.Y + anchorBounds.Height).Within(1));
        await Page.EvaluateAsync("openAt(379, 259, 3)");
        var pointerBounds = await panel.BoundingBoxAsync();
        Assert.That(pointerBounds!.X + pointerBounds.Width, Is.LessThanOrEqualTo(373));
        Assert.That(pointerBounds.Y + pointerBounds.Height, Is.LessThanOrEqualTo(253));
        await Page.Locator("#outside").ClickAsync();
        await Expect(panel).ToBeHiddenAsync();
        await Expect(Page.Locator("#outside")).ToBeFocusedAsync();
        await Page.WaitForFunctionAsync("dismissed.length === 2");
        Assert.That(await Page.EvaluateAsync<int[]>("dismissed"), Is.EqualTo(new[] { 1, 3 }));
        await Page.EvaluateAsync("openAt(null, null, 4); document.querySelector('#panel').remove(); contextMenu.close('panel')");
        await Expect(anchor).ToBeFocusedAsync();
    }
}
