using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// The virtualized table pattern on the published showcase: with
/// <c>Virtualize</c> on, the DOM holds a window of rows — never the whole
/// collection — and scrolling the page itself trades the window forward while
/// keys and row markup stay stable.
/// </summary>
public sealed partial class ShowcaseBrowserTests
{
    [Test]
    public async Task Patterns_VirtualizedTableKeepsARowWindowWhileScrolling()
    {
        await Page.GotoAsync($"{BaseUrl}?view=patterns", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator(".site-header")).ToBeVisibleAsync();
        // Prerendered markup has no handlers; virtualization refreshes on
        // scroll events, so the interactive circuit must own the page first.
        await Page.WaitForFunctionAsync("performance.getEntriesByName('sui-interactive').length > 0");

        var section = Page.Locator("#patterns-continuous");
        await section.ScrollIntoViewIfNeededAsync();

        var rows = section.Locator("tbody tr.sui-table__row");
        await Expect(rows.First).ToContainTextAsync("Lançamento contínuo 0001");

        // Windowing settles asynchronously: the prerendered DOM (and the first
        // interactive render before measurement) legitimately holds every row,
        // because without JS measurement Virtualize cannot know a range. Wait
        // for the window to establish itself, then hold it to its promise.
        var windowed = false;
        for (var attempt = 0; attempt < 20 && !windowed; attempt++)
        {
            windowed = await rows.CountAsync() < 100;
            if (!windowed) await Page.WaitForTimeoutAsync(250);
        }
        Assert.That(windowed, Is.True, "virtualized body must not materialize every row");
        // Spacers keep the scroll geometry inside tbody as table rows.
        Assert.That(await section.Locator("tbody tr:not(.sui-table__row)").CountAsync(), Is.EqualTo(2));

        // The wrapper is the scroll container: max-height + overflow-y make it
        // a real one, which is what lets Virtualize compute a finite range.
        Assert.That(await section.Locator(".sui-table-wrapper--virtual").CountAsync(), Is.EqualTo(1));

        // Page scroll (through the wrapper) drives the window forward.
        for (var attempt = 0; ; attempt++)
        {
            await section.Locator(".sui-table-wrapper--virtual")
                .EvaluateAsync("w => w.scrollTop = w.scrollHeight");
            try
            {
                await Expect(section.GetByText("Lançamento contínuo 0100").Last)
                    .ToBeVisibleAsync(new() { Timeout = 1_000 });
                break;
            }
            catch (PlaywrightException)
            {
                if (attempt == 10) throw;
            }
        }

        // Even with the last record rendered, the window — not the list — is
        // what lives in the DOM.
        Assert.That(await rows.CountAsync(), Is.LessThan(100), "scrolling must trade the window, not grow the DOM");
    }
}
