using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed class MenuLifecycleBrowserTests : PageTest
{
    [Test]
    public async Task MissingDetachedAndRepeatedConnectionsAreSafeAndKeyboardStillWorks()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "src/Components/Overlays/SUIMenu.razor.js"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        var script = await File.ReadAllTextAsync(Path.Combine(root!.FullName, "src/Components/Overlays/SUIMenu.razor.js"));
        await Page.SetContentAsync("<button id='trigger' popovertarget='panel'>Menu</button><div id='panel' popover='auto'><button id='one'>One</button><button id='two'>Two</button></div>");
        await Page.EvaluateAsync("""
            async source => {
                const module = await import('data:text/javascript;base64,' + btoa(source));
                window.menuModule = module;
                const t = document.querySelector('#trigger'), p = document.querySelector('#panel');
                module.connect(null, null);
                module.connect(t, null);
                module.connect(document.createElement('button'), document.createElement('div'));
                module.disconnect(null);
                module.connect(t, p);
                module.connect(t, p);
            }
            """, script);
        await Page.Locator("#trigger").ClickAsync();
        await Expect(Page.Locator("#one")).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("ArrowDown");
        // Duplicate key listeners would move twice and return focus to One.
        await Expect(Page.Locator("#two")).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.Locator("#trigger")).ToBeFocusedAsync();
        await Page.EvaluateAsync("menuModule.disconnect(document.querySelector('#panel'))");
        await Page.EvaluateAsync("document.querySelector('#panel').showPopover()");
        await Page.Locator("#one").FocusAsync();
        await Page.EvaluateAsync("document.querySelector('#panel').dispatchEvent(new KeyboardEvent('keydown', {key:'ArrowDown',bubbles:true}))");
        await Expect(Page.Locator("#one")).ToBeFocusedAsync();
    }
}
