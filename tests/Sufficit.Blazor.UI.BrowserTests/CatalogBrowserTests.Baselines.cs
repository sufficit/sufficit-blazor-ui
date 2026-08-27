using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Globalization;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// The committed visual-baseline comparisons (Chromium-only) — split from
/// CatalogBrowserTests.cs to respect its frozen line budget.
/// </summary>
public sealed partial class CatalogBrowserTests
{
    [Test]
    public async Task Catalog_MatchesCommittedVisualBaselines()
    {
        if (!string.Equals(BrowserName, "chromium", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("Committed visual baselines are intentionally Chromium-only.");
        }

        var baselineDirectory = Environment.GetEnvironmentVariable("SUI_BASELINE_DIR")
            ?? Path.Combine(AppContext.BaseDirectory, "baselines", "catalog");
        var artifactDirectory = Environment.GetEnvironmentVariable("SUI_VISUAL_ARTIFACT_DIR")
            ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "visual-artifacts", BrowserName);
        var updateBaselines = Environment.GetEnvironmentVariable("SUI_UPDATE_BASELINES") == "1";
        Directory.CreateDirectory(baselineDirectory);
        Directory.CreateDirectory(artifactDirectory);

        var scenarios = new[]
        {
            new VisualScenario("catalog-light-desktop.png", 1440, 1000, false),
            new VisualScenario("catalog-light-mobile.png", 390, 844, false),
            new VisualScenario("catalog-dark-desktop.png", 1440, 1000, true),
            new VisualScenario("catalog-dark-mobile.png", 390, 844, true),
        };

        foreach (var scenario in scenarios)
        {
            await Page.SetViewportSizeAsync(scenario.Width, scenario.Height);
            await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
            if (scenario.Dark)
            {
                await Page.Locator("[data-testid='theme-toggle']").ClickAsync();
                await Expect(Page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync("data-theme", "dark");
            }

            await Page.EvaluateAsync("document.fonts.ready");
            var actualPath = Path.Combine(artifactDirectory, scenario.FileName);
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = actualPath,
                FullPage = true,
                Animations = ScreenshotAnimations.Disabled,
                Caret = ScreenshotCaret.Hide,
            });

            var baselinePath = Path.Combine(baselineDirectory, scenario.FileName);
            if (updateBaselines)
            {
                File.Copy(actualPath, baselinePath, true);
                continue;
            }

            Assert.That(File.Exists(baselinePath), Is.True,
                $"Missing visual baseline: {baselinePath}. Set SUI_UPDATE_BASELINES=1 for an intentional update.");
            var comparison = await ComparePngsInBrowserAsync(baselinePath, actualPath);
            Assert.That(comparison.DimensionsMatch, Is.True,
                $"Visual dimensions changed for {scenario.FileName}: expected "
                + $"{comparison.ExpectedWidth}x{comparison.ExpectedHeight}, actual "
                + $"{comparison.ActualWidth}x{comparison.ActualHeight}. Actual: {actualPath}");
            Assert.That(comparison.DiffRatio, Is.LessThanOrEqualTo(0.005),
                $"Visual regression in {scenario.FileName}: {comparison.DifferentPixels:N0}/"
                + $"{comparison.TotalPixels:N0} pixels ({comparison.DiffRatio:P3}) differ; "
                + $"changed bounds={comparison.ChangedBounds}. Actual: {actualPath}");
        }
    }
}
