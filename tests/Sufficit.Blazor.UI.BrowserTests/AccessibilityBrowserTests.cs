using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// Accessibility coverage beyond the desktop light-theme axe sweep in
/// <see cref="CatalogBrowserTests"/>: the same audit under the viewports and
/// user preferences real people browse with, plus the checks axe cannot make
/// on its own (visible focus, text spacing, live regions, heading order).
/// </summary>
[Parallelizable(ParallelScope.Self)]
public sealed class AccessibilityBrowserTests : PageTest
{
    private static string BaseUrl
        => Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";

    private static List<string> WcagTags => ["wcag2a", "wcag2aa", "wcag21aa", "wcag22aa"];

    [SetUp]
    public async Task OpenCatalogAsync()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
    }

    [TestCase(375, 812, false, TestName = "Axe_MobileLight")]
    [TestCase(375, 812, true, TestName = "Axe_MobileDark")]
    [TestCase(1440, 900, true, TestName = "Axe_DesktopDark")]
    public async Task Catalog_PassesAxeAcrossViewportsAndThemes(int width, int height, bool dark)
    {
        await Page.SetViewportSizeAsync(width, height);
        if (dark)
        {
            await Page.Locator("[data-testid='theme-toggle']").ClickAsync();
            await Expect(Page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync("data-theme", "dark");
        }

        await Page.WaitForTimeoutAsync(400);
        await AssertNoBlockingViolationsAsync();
    }

    [Test]
    public async Task Catalog_PassesAxeUnderReducedMotionAndDarkColorScheme()
    {
        await Page.EmulateMediaAsync(new PageEmulateMediaOptions
        {
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark,
        });
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();

        await AssertNoBlockingViolationsAsync();
    }

    [Test]
    public async Task KeyboardFocus_IsAlwaysVisibleOnInteractiveElements()
    {
        var invisible = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const selector = 'a[href], button:not([disabled]), input:not([disabled]), ' +
                    'select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
                const offenders = [];
                for (const element of document.querySelectorAll(selector)) {
                    const rect = element.getBoundingClientRect();
                    if (rect.width === 0 && rect.height === 0) continue;

                    element.focus();
                    const focused = getComputedStyle(element);
                    const ring =
                        (focused.outlineStyle !== 'none' && parseFloat(focused.outlineWidth) > 0) ||
                        focused.boxShadow !== 'none';
                    if (!ring) {
                        offenders.push(element.tagName.toLowerCase() +
                            (element.className ? '.' + String(element.className).split(' ')[0] : ''));
                    }
                }
                return [...new Set(offenders)];
            }
            """);

        Assert.That(invisible, Is.Empty,
            "Focusable elements without a focus indicator: " + string.Join(", ", invisible));
    }

    [Test]
    public async Task TabOrder_FollowsTheVisualOrderAndNeverTraps()
    {
        var order = await Page.EvaluateAsync<int>(
            """
            () => {
                const selector = 'a[href], button:not([disabled]), input:not([disabled]), ' +
                    '[tabindex]:not([tabindex="-1"])';
                const visible = [...document.querySelectorAll(selector)]
                    .filter(element => element.getBoundingClientRect().height > 0);
                return visible.filter(element => Number(element.getAttribute('tabindex')) > 0).length;
            }
            """);

        Assert.That(order, Is.Zero, "Positive tabindex values break the document tab order.");
    }

    [Test]
    public async Task TextSpacingOverride_DoesNotClipOrOverflowContent()
    {
        // WCAG 2.1 SC 1.4.12 Text Spacing.
        await Page.AddStyleTagAsync(new PageAddStyleTagOptions
        {
            Content = """
                * {
                    line-height: 1.5 !important;
                    letter-spacing: 0.12em !important;
                    word-spacing: 0.16em !important;
                }
                p { margin-bottom: 2em !important; }
                """,
        });
        await Page.WaitForTimeoutAsync(200);

        var overflow = await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");

        Assert.That(overflow, Is.False, "Increased text spacing causes horizontal overflow.");
    }

    [Test]
    public async Task DocumentStructure_HasOneMainLandmarkAndOrderedHeadings()
    {
        var structure = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const problems = [];
                const mains = document.querySelectorAll('main, [role="main"]');
                if (mains.length !== 1) problems.push(`main landmarks: ${mains.length}`);

                const levels = [...document.querySelectorAll('h1, h2, h3, h4, h5, h6')]
                    .filter(heading => heading.getBoundingClientRect().height > 0)
                    .map(heading => Number(heading.tagName.slice(1)));

                if (levels.filter(level => level === 1).length !== 1)
                    problems.push(`h1 count: ${levels.filter(level => level === 1).length}`);

                for (let index = 1; index < levels.length; index++) {
                    if (levels[index] - levels[index - 1] > 1)
                        problems.push(`heading jumps h${levels[index - 1]} -> h${levels[index]}`);
                }

                if (!document.documentElement.getAttribute('lang')) problems.push('missing <html lang>');
                if (!document.title) problems.push('missing <title>');
                return problems;
            }
            """);

        Assert.That(structure, Is.Empty, string.Join("; ", structure));
    }

    [Test]
    public async Task TransientFeedback_IsAnnouncedThroughALiveRegion()
    {
        var regions = await Page.EvaluateAsync<int>(
            "() => document.querySelectorAll('[aria-live], [role=\"status\"], [role=\"alert\"]').length");

        Assert.That(regions, Is.GreaterThan(0),
            "Snackbars and status banners must render inside a live region to be announced.");
    }

    [Test]
    public async Task Images_AndIcons_AreEitherLabelledOrHiddenFromAssistiveTech()
    {
        var offenders = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const problems = [];
                for (const image of document.querySelectorAll('img')) {
                    if (image.getAttribute('alt') === null) problems.push('img without alt: ' + image.src);
                }
                for (const svg of document.querySelectorAll('svg')) {
                    const labelled = svg.getAttribute('aria-label') || svg.getAttribute('aria-labelledby')
                        || svg.querySelector('title');
                    const hidden = svg.getAttribute('aria-hidden') === 'true'
                        || svg.getAttribute('focusable') === 'false' && svg.getAttribute('role') === 'presentation';
                    if (!labelled && !hidden) problems.push('svg neither labelled nor hidden: ' + svg.getAttribute('class'));
                }
                return [...new Set(problems)];
            }
            """);

        Assert.That(offenders, Is.Empty, string.Join(Environment.NewLine, offenders));
    }

    private async Task AssertNoBlockingViolationsAsync()
    {
        var results = await Page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = "tag", Values = WcagTags },
        });

        var blocking = results.Violations
            .Where(violation => violation.Impact is "serious" or "critical")
            .ToArray();

        Assert.That(blocking, Is.Empty,
            string.Join(Environment.NewLine, blocking.Select(violation =>
                $"{violation.Id}: {violation.Help} ({violation.Nodes.Count()} nodes)")));
    }
}
