using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

// A synthetic host exercises the shipped module with a real permissions policy,
// without authenticating, touching customer credentials or requiring the catalog.
[Parallelizable(ParallelScope.Self)]
public sealed class ClipboardBrowserTests : PageTest
{
    private const string Value = "Fixture-Only<&> 42";

    private async Task OpenAsync(bool blockClipboard = false, string setup = "")
    {
        if (BrowserName != "chromium")
            Assert.Ignore("Clipboard permissions and Permissions-Policy probes require Chromium.");

        await Context.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
        var module = await File.ReadAllTextAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "clipboard-module.js"));
        await Page.RouteAsync("http://localhost/clipboard/**", route => route.FulfillAsync(new()
        {
            ContentType = route.Request.Url.EndsWith("module.js") ? "text/javascript" : "text/html",
            Headers = new Dictionary<string, string>
            {
                ["Permissions-Policy"] = blockClipboard ? "clipboard-write=()" : "clipboard-write=(self)"
            },
            Body = route.Request.Url.EndsWith("module.js") ? module : """
                <!doctype html><html><body>
                <input aria-label="Draft" value="preserve this draft">
                <button type="button">Copy</button><output aria-live="polite"></output>
                <script type="module">
                import { copyText } from './module.js';
                window.copyText = copyText;
                document.querySelector('button').onclick = async () => {
                    try { await copyText('Fixture-Only<&> 42'); document.querySelector('output').textContent = 'Copied'; }
                    catch (error) { document.querySelector('output').textContent = error.message; }
                };
                </script></body></html>
                """
        }));
        await Page.GotoAsync("http://localhost/clipboard/");
        await Page.WaitForFunctionAsync("() => typeof window.copyText === 'function'");
        if (setup.Length > 0) await Page.EvaluateAsync(setup);
    }

    [Test]
    public async Task ModernApi_CopiesWithoutLegacyFallback()
    {
        await OpenAsync(setup: "() => { document.execCommand = () => { throw new Error('Unexpected fallback'); }; }");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Copy" }).ClickAsync();
        await Expect(Page.Locator("output")).ToHaveTextAsync("Copied");
        Assert.That(await Page.EvaluateAsync<string>("navigator.clipboard.readText()"), Is.EqualTo(Value));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task PermissionsPolicyDenial_CopiesByClickOrKeyboard(bool keyboard)
    {
        await OpenAsync(blockClipboard: true);
        var denial = await Page.EvaluateAsync<string>("""
            async () => {
                try { await navigator.clipboard.writeText('probe'); return 'allowed'; }
                catch (error) { return error.name; }
            }
            """);
        Assert.That(denial, Is.EqualTo("NotAllowedError"), "Positive control: the real policy must deny writeText.");
        var button = Page.GetByRole(AriaRole.Button, new() { Name = "Copy" });
        if (keyboard) { await button.FocusAsync(); await button.PressAsync("Enter"); }
        else await button.ClickAsync();
        await Expect(Page.Locator("output")).ToHaveTextAsync("Copied");
        Assert.That(await Page.EvaluateAsync<string>("navigator.clipboard.readText()"), Is.EqualTo(Value));
        await Expect(button).ToBeFocusedAsync();
        Assert.That(await Page.Locator("textarea").CountAsync(), Is.Zero);
    }

    [TestCase("missing")]
    [TestCase("false")]
    [TestCase("throw")]
    public async Task LegacyPath_RestoresDraftFocusAndSelection_AndReportsFailure(string mode)
    {
        await OpenAsync(setup: """
            () => {
                Object.defineProperty(navigator, 'clipboard', { value: undefined });
                const input = document.querySelector('input');
                input.focus(); input.setSelectionRange(2, 8, 'backward');
            }
            """);
        if (mode == "missing") await Page.EvaluateAsync("document.execCommand = undefined");
        else if (mode == "false") await Page.EvaluateAsync("() => { document.execCommand = () => false; }");
        else await Page.EvaluateAsync("() => { document.execCommand = () => { throw new Error('denied'); }; }");
        var error = await Page.EvaluateAsync<string>("""
            async () => { try { await copyText('test'); return 'success'; } catch (error) { return error.message; } }
            """);
        Assert.That(error, Does.Contain("Não foi possível copiar"));
        Assert.That(await Page.Locator("textarea").CountAsync(), Is.Zero);
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Draft" })).ToBeFocusedAsync();
        Assert.That(await Page.EvaluateAsync<int[]>("[document.activeElement.selectionStart, document.activeElement.selectionEnd]"), Is.EqualTo(new[] { 2, 8 }));
        Assert.That(await Page.EvaluateAsync<string>("document.activeElement.selectionDirection"), Is.EqualTo("backward"));
    }

    [Test]
    public async Task MissingModernApi_LegacyPathCopiesAndPreservesDraft()
    {
        await OpenAsync(setup: "Object.defineProperty(navigator, 'clipboard', { value: undefined })");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Copy" }).ClickAsync();
        await Expect(Page.Locator("output")).ToHaveTextAsync("Copied");
        Assert.That(await Page.GetByRole(AriaRole.Textbox).InputValueAsync(), Is.EqualTo("preserve this draft"));
        Assert.That(await Page.Locator("textarea").CountAsync(), Is.Zero);
    }
}
