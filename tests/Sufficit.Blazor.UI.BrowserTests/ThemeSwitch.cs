using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// Waits for the catalog's interactive circuit before acting on it.
/// </summary>
/// <remarks>
/// The catalog is prerendered static HTML that an InteractiveServer circuit
/// replaces as soon as it connects. A click dispatched before that swap lands
/// on the prerendered toggle, which has no handler yet: nothing toggles, the
/// circuit re-renders the page still on the light theme, and the later
/// data-theme wait times out reporting the page as "light". That is how
/// Axe_MobileDark failed on WebKit alone (run 36931168454) while the same
/// code passed on Chromium and Firefox and on re-run with no code change -
/// the same class of race <see cref="LayoutProbe"/> documents for layout
/// reads, except the lost action is the click itself.
///
/// data-interactive, emitted by the Gallery, only becomes "true" on the
/// circuit's first interactive render, so waiting for it gates a click on a
/// live handler instead of on prerendered markup. Once the circuit owns the
/// button the toggle is synchronous, and the trailing assertion still
/// auto-retries while the round-trip settles.
/// </remarks>
internal static class ThemeSwitch
{
    /// <summary>
    /// Waits until the catalog is rendered by a live circuit, not prerendered HTML.
    /// </summary>
    public static Task WaitForInteractiveCatalogAsync(this IPage page)
        => Assertions.Expect(page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync(
            "data-interactive", "true",
            new LocatorAssertionsToHaveAttributeOptions { Timeout = 10_000 });

    /// <summary>
    /// Waits for catalog interactivity, then clicks the theme toggle and
    /// confirms the dark theme took effect.
    /// </summary>
    public static async Task SwitchToDarkThemeAsync(this IPage page)
    {
        await page.WaitForInteractiveCatalogAsync();
        await page.Locator("[data-testid='theme-toggle']").ClickAsync();
        await Assertions.Expect(page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync(
            "data-theme", "dark", new LocatorAssertionsToHaveAttributeOptions { Timeout = 10_000 });
    }
}
