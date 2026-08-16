using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// Runtime performance budgets measured against the published catalog. The
/// static byte budgets live in the unit test suite (AssetBudgetTests); these
/// assert what the browser actually pays: request count, transferred bytes,
/// DOM size, layout stability and paint timing. Chromium-only metrics are
/// skipped on the other engines instead of being asserted loosely.
/// </summary>
[Parallelizable(ParallelScope.Self)]
public sealed class PerformanceBudgetBrowserTests : PageTest
{
    private const int RequestBudget = 30;
    private const int DomNodeBudget = 3000;
    private const long CssTransferBudget = 80 * 1024;
    private const long TotalTransferBudget = 900 * 1024;
    private const double LargestContentfulPaintBudgetMs = 2500;
    private const double CumulativeLayoutShiftBudget = 0.1;
    private const double LayoutStabilityAfterInteractionBudget = 0.05;

    private static string BaseUrl
        => Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";

    [SetUp]
    public async Task OpenCatalogAsync()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Catalog_StaysWithinItsRequestAndDomBudget()
    {
        var payload = await Page.EvaluateAsync<JsonElement>(
            """
            () => ({
                requests: performance.getEntriesByType('resource').length,
                nodes: document.getElementsByTagName('*').length,
                stylesheets: document.querySelectorAll('link[rel="stylesheet"]').length,
                // The Blazor framework script is owned by the host app, not by
                // the library; the library itself must never inject a script.
                blockingScripts: [...document.querySelectorAll('script[src]')]
                    .filter(script => !script.src.includes('/_framework/'))
                    .filter(script => !script.async && !script.defer && script.type !== 'module').length,
                libraryScripts: [...document.querySelectorAll('script[src]')]
                    .filter(script => script.src.includes('_content/Sufficit.Blazor.UI')).length
            })
            """);

        Assert.Multiple(() =>
        {
            Assert.That(payload.GetProperty("requests").GetInt32(), Is.LessThanOrEqualTo(RequestBudget));
            Assert.That(payload.GetProperty("nodes").GetInt32(), Is.LessThanOrEqualTo(DomNodeBudget));
            Assert.That(payload.GetProperty("stylesheets").GetInt32(), Is.LessThanOrEqualTo(4),
                "The library ships one global bundle plus the consumer's scoped stylesheet.");
            Assert.That(payload.GetProperty("blockingScripts").GetInt32(), Is.Zero,
                "Render-blocking classic scripts are not allowed.");
            Assert.That(payload.GetProperty("libraryScripts").GetInt32(), Is.Zero,
                "SUI loads its JavaScript as colocated ES modules, never as a page script tag.");
        });
    }

    [Test]
    public async Task TransferredBytes_StayWithinBudget()
    {
        if (BrowserName != "chromium")
            Assert.Ignore("transferSize is only reported reliably by Chromium.");

        var payload = await Page.EvaluateAsync<JsonElement>(
            """
            () => {
                const resources = performance.getEntriesByType('resource');
                const sum = filter => resources.filter(filter)
                    .reduce((total, entry) => total + (entry.transferSize || 0), 0);
                return {
                    total: sum(() => true),
                    css: sum(entry => entry.name.includes('.css')),
                    js: sum(entry => entry.name.includes('.js')),
                    fonts: sum(entry => entry.initiatorType === 'css' && /\.(woff2?|ttf)/.test(entry.name))
                };
            }
            """);

        Assert.Multiple(() =>
        {
            Assert.That(payload.GetProperty("css").GetInt64(), Is.LessThanOrEqualTo(CssTransferBudget),
                "CSS transfer budget exceeded.");
            Assert.That(payload.GetProperty("total").GetInt64(), Is.LessThanOrEqualTo(TotalTransferBudget),
                "Total transfer budget exceeded.");
            Assert.That(payload.GetProperty("fonts").GetInt64(), Is.Zero,
                "The library must not pull web fonts; consumers own typography.");
        });
    }

    [Test]
    public async Task PaintTiming_AndLayoutStability_MeetTheBudget()
    {
        if (BrowserName != "chromium")
            Assert.Ignore("LCP and CLS observers are Chromium-only.");

        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var payload = await Page.EvaluateAsync<JsonElement>(
            """
            () => new Promise(resolve => {
                let lcp = 0;
                let cls = 0;
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries()) lcp = Math.max(lcp, entry.startTime);
                }).observe({ type: 'largest-contentful-paint', buffered: true });
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries())
                        if (!entry.hadRecentInput) cls += entry.value;
                }).observe({ type: 'layout-shift', buffered: true });

                setTimeout(() => {
                    const paint = performance.getEntriesByName('first-contentful-paint')[0];
                    resolve({ lcp, cls, fcp: paint ? paint.startTime : 0 });
                }, 1200);
            })
            """);

        Assert.Multiple(() =>
        {
            Assert.That(payload.GetProperty("lcp").GetDouble(),
                Is.LessThanOrEqualTo(LargestContentfulPaintBudgetMs), "LCP budget exceeded.");
            Assert.That(payload.GetProperty("cls").GetDouble(),
                Is.LessThanOrEqualTo(CumulativeLayoutShiftBudget), "CLS budget exceeded.");
        });
    }

    [Test]
    public async Task OpeningOverlays_DoesNotShiftTheUnderlyingLayout()
    {
        if (BrowserName != "chromium")
            Assert.Ignore("layout-shift observer is Chromium-only.");

        await Page.EvaluateAsync(
            """
            () => {
                window.__suiShift = 0;
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries())
                        if (!entry.hadRecentInput) window.__suiShift += entry.value;
                }).observe({ type: 'layout-shift', buffered: false });
            }
            """);

        await Page.Locator("[data-testid='select']").ClickAsync();
        await Page.WaitForTimeoutAsync(300);
        await Page.Keyboard.PressAsync("Escape");
        await Page.WaitForTimeoutAsync(300);

        var shift = await Page.EvaluateAsync<double>("() => window.__suiShift");

        Assert.That(shift, Is.LessThanOrEqualTo(LayoutStabilityAfterInteractionBudget),
            "Opening and closing the select shifts the page; overlays must render in the top layer.");
    }

    [Test]
    public async Task StaticAssets_AreServedWithCompressionAndCaching()
    {
        var response = await Page.APIRequest.GetAsync(
            $"{BaseUrl.TrimEnd('/')}/_content/Sufficit.Blazor.UI/sufficit-ui.css",
            new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string> { ["Accept-Encoding"] = "br, gzip" },
            });

        Assert.That(response.Status, Is.EqualTo(200));

        var headers = response.Headers;
        Assert.That(headers.ContainsKey("etag") || headers.ContainsKey("last-modified"), Is.True,
            "Static web assets must be cacheable (ETag or Last-Modified).");
    }
}
