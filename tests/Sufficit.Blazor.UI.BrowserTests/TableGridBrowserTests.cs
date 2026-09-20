using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// An interactive SUITable is an ARIA grid with row focus: one row in the tab
/// order, arrows/Home/End move between rows without scrolling the page, and
/// Enter/Space activate the focused row.
/// </summary>
public sealed class TableGridBrowserTests : PageTest
{
    private static string BaseUrl
        => Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";

    [Test]
    public async Task InteractiveRows_MoveFocusWithArrowsAndActivateWithSpace()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();

        var rows = Page.Locator("table[role=grid] tbody tr.sui-table__row--interactive");
        Assert.That(await rows.CountAsync(), Is.GreaterThan(1));
        await Expect(rows.First).ToHaveAttributeAsync("tabindex", "0");
        await Expect(rows.Nth(1)).ToHaveAttributeAsync("tabindex", "-1");

        // The keyboard module attaches after the circuit's first render; retry
        // until it answers instead of racing it.
        for (var attempt = 0; ; attempt++)
        {
            await rows.First.FocusAsync();
            await Page.Keyboard.PressAsync("ArrowDown");
            if (await rows.Nth(1).EvaluateAsync<bool>("row => row === document.activeElement") || attempt == 20)
                break;
            await Page.WaitForTimeoutAsync(250);
        }
        await Expect(rows.Nth(1)).ToBeFocusedAsync();
        await Expect(rows.Nth(1)).ToHaveAttributeAsync("tabindex", "0");
        await Expect(rows.First).ToHaveAttributeAsync("tabindex", "-1");

        await Page.Keyboard.PressAsync("End");
        await Expect(rows.Last).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Home");
        await Expect(rows.First).ToBeFocusedAsync();

        // The browser's own reaction (scroll, page down) must be cancelled.
        await Page.EvaluateAsync("() => document.addEventListener('keydown', e => window.__suiPrevented = e.defaultPrevented)");
        await Page.Keyboard.PressAsync("ArrowDown");
        Assert.That(await Page.EvaluateAsync<bool>("() => window.__suiPrevented"), Is.True);
        await Page.Keyboard.PressAsync(" ");
        Assert.That(await Page.EvaluateAsync<bool>("() => window.__suiPrevented"), Is.True);
        await Expect(Page.GetByText("Selecionado:").First).ToBeVisibleAsync();
    }
}
