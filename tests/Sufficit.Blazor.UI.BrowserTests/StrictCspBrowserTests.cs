using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// Loads catalog pages with the strict policy identity runs in production
/// (<c>style-src-attr 'none'</c>). Under it the browser ignores every style
/// attribute, prerendered or set later by Blazor, so SUI's computed geometry has
/// to arrive as classes or through data-sui-style (CSSOM). These tests fail if a
/// component falls back to a style attribute: the layout breaks and the page
/// reports a securitypolicyviolation.
/// </summary>
[Parallelizable(ParallelScope.Self)]
public sealed class StrictCspBrowserTests : PageTest
{
    private static string BaseUrl
        => (Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180").TrimEnd('/');

    private const string CollectViolations = """
        window.__suiCsp = [];
        document.addEventListener('securitypolicyviolation', event => {
            const target = event.target && event.target.outerHTML ? event.target.outerHTML.slice(0, 160) : String(event.target);
            window.__suiCsp.push(`${event.violatedDirective || event.effectiveDirective}: ${target}`);
        });
        """;

    [SetUp]
    public async Task CollectViolationsAsync()
        // The catalog serves the policy itself for ?csp=strict. Intercepting the
        // document with a Playwright route instead breaks the circuit: Chromium
        // then blocks the WebSocket to 127.0.0.1 as a local-network request.
        => await Page.AddInitScriptAsync(CollectViolations);

    [Test]
    public async Task ComputedGeometry_HoldsWithoutStyleAttributes()
    {
        await Page.SetViewportSizeAsync(1280, 900);
        await Page.GotoAsync($"{BaseUrl}/fixtures/csp?csp=strict", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await AssertPolicyIsEnforcedAsync();

        // The drawer width is the first CSSOM value to land; the rest share the pass.
        await Page.WaitForFunctionAsync(
            "() => Math.round(document.querySelector('[data-testid=csp-drawer]')?.getBoundingClientRect().width) === 176");

        var grid = Page.GetByTestId("csp-template-grid").Locator(":scope > *");
        var first = await grid.Nth(0).RequireBoundingBoxAsync("first template column");
        var second = await grid.Nth(1).RequireBoundingBoxAsync("second template column");
        Assert.That(first.Width / second.Width, Is.EqualTo(2).Within(0.1), "ColumnTemplate 2fr 1fr must not fall back to equal columns.");

        var metrics = await Page.EvaluateAsync<JsonElement>("""
            () => {
                const css = (selector, read) => read(getComputedStyle(document.querySelector(selector)));
                const tone = document.createElement('span');
                tone.style.color = 'var(--sui-color-error)';
                document.querySelector('main').append(tone);
                const expected = getComputedStyle(tone).color;
                tone.remove();
                return {
                    minWidth: css('[data-testid=csp-template-grid]', s => s.getPropertyValue('--sui-form-grid-min-width').trim()),
                    sectionGap: css('[data-testid=csp-section-gap]', s => s.marginBlockStart),
                    spaceOne: getComputedStyle(document.documentElement).getPropertyValue('--sui-space-1').trim(),
                    textColor: css('[data-testid=csp-text-error]', s => s.color),
                    expectedColor: expected,
                    skeleton: Math.round(document.querySelector('[data-testid=csp-skeleton] .sui-skeleton-circle').getBoundingClientRect().width),
                    statImage: css('[data-testid=csp-stat]', s => s.getPropertyValue('--sui-stat-image')),
                    popoverMaxWidth: css('.sui-popover', s => s.maxWidth),
                    collapseMaxHeight: css('.sui-collapse--capped', s => s.maxHeight),
                    dashOffset: parseFloat(css('[data-testid=csp-progress-circular] circle:last-child', s => s.strokeDashoffset)),
                };
            }
            """);

        Assert.Multiple(() =>
        {
            Assert.That(metrics.GetProperty("minWidth").GetString(), Is.EqualTo("10rem"));
            Assert.That(metrics.GetProperty("sectionGap").GetString(), Is.EqualTo(metrics.GetProperty("spaceOne").GetString()));
            Assert.That(metrics.GetProperty("textColor").GetString(), Is.EqualTo(metrics.GetProperty("expectedColor").GetString()));
            Assert.That(metrics.GetProperty("skeleton").GetInt32(), Is.EqualTo(32));
            Assert.That(metrics.GetProperty("statImage").GetString(), Does.Contain("url("));
            Assert.That(metrics.GetProperty("popoverMaxWidth").GetString(), Is.EqualTo("200px"));
            Assert.That(metrics.GetProperty("collapseMaxHeight").GetString(), Is.EqualTo("40px"));
            Assert.That(metrics.GetProperty("dashOffset").GetDouble(), Is.EqualTo(2 * Math.PI * 10 * 0.35).Within(0.1));
        });

        await AssertProgressScaleAsync("[data-testid=csp-progress-linear]", 0.40);

        await Page.GetByTestId("csp-select").Locator(".sui-select__trigger").ClickAsync();
        var menu = Page.GetByTestId("csp-select").GetByRole(AriaRole.Listbox);
        await Expect(menu).ToBeVisibleAsync();
        var menuBox = await menu.RequireBoundingBoxAsync("select menu");
        Assert.That(menuBox.Width, Is.EqualTo(22 * 16).Within(2), "MenuWidth must reach the menu under a strict CSP.");

        await AssertNoViolationsAsync();
    }

    [Test]
    public async Task Catalog_RendersWithoutStyleAttributeViolations()
    {
        await Page.GotoAsync($"{BaseUrl}/?csp=strict", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-interactive=true]")).ToBeVisibleAsync();
        await AssertPolicyIsEnforcedAsync();
        await AssertProgressScaleAsync(".sui-progress-linear", 0.68);
        await AssertNoViolationsAsync();
    }

    private async Task AssertProgressScaleAsync(string selector, double expected)
    {
        // The bar scales with a transition; wait for it to settle on the value.
        await Page.WaitForFunctionAsync(
            """
            ([selector, expected]) => {
                const track = document.querySelector(selector);
                const bar = track?.querySelector('.sui-progress-linear__bar');
                if (!bar) return false;
                const ratio = bar.getBoundingClientRect().width / track.getBoundingClientRect().width;
                return Math.abs(ratio - expected) < 0.02;
            }
            """,
            new object[] { selector, expected });
    }

    // A positive control: without it, a route that silently failed to add the
    // header would make every assertion here pass vacuously.
    private async Task AssertPolicyIsEnforcedAsync()
    {
        var blocked = await Page.EvaluateAsync<bool>("""
            async () => {
                const before = window.__suiCsp.length;
                const probe = document.createElement('div');
                probe.className = 'csp-control';
                document.body.append(probe);
                probe.setAttribute('style', 'width: 123px');
                const width = probe.getBoundingClientRect().width;
                await new Promise(resolve => setTimeout(resolve, 100));
                probe.remove();
                const reported = window.__suiCsp.length > before;
                window.__suiCsp.length = before;
                return width !== 123 && reported;
            }
            """);
        Assert.That(blocked, Is.True, "The strict policy must be active and reported for this test to mean anything.");
    }

    private async Task AssertNoViolationsAsync()
    {
        await Page.WaitForTimeoutAsync(100);
        var violations = await Page.EvaluateAsync<string[]>("() => window.__suiCsp");
        Assert.That(violations, Is.Empty, "SUI rendered a style attribute that a strict CSP ignores.");
    }
}
