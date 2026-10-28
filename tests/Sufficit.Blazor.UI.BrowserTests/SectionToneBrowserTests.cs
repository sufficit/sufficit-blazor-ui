using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// SUISection.ToneValue as a danger zone under the strict CSP: tone on the
/// border and title only (classes, no style attribute), action at the end of
/// the header row on wide screens and wrapped under the copy on phones, and a
/// gap to the section above so stacked sections never touch.
/// </summary>
[Parallelizable(ParallelScope.Self)]
public sealed class SectionToneBrowserTests : PageTest
{
    private static string BaseUrl
        => (Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180").TrimEnd('/');

    [TestCase(1280, false)]
    [TestCase(390, true)]
    public async Task DangerSection_TintsBorderAndTitle_AndWrapsTheActionOnNarrowScreens(int width, bool wrapped)
    {
        await Page.SetViewportSizeAsync(width, 900);
        await Page.GotoAsync($"{BaseUrl}/fixtures/csp?csp=strict", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var metrics = await Page.EvaluateAsync<JsonElement>("""
            () => {
                const zone = document.querySelector('[data-testid=csp-section-danger]');
                const rect = element => element.getBoundingClientRect();
                const probe = document.createElement('span');
                probe.style.color = 'var(--sui-color-error)';
                document.querySelector('main').append(probe);
                const error = getComputedStyle(probe).color;
                probe.remove();
                const copy = rect(zone.querySelector('.sui-card__header-copy'));
                const actions = rect(zone.querySelector('.sui-card__header-actions'));
                const card = zone.querySelector('.sui-card');
                return {
                    error,
                    title: getComputedStyle(zone.querySelector('.sui-card__header-title')).color,
                    description: getComputedStyle(zone.querySelector('.sui-card__header-description')).color,
                    border: getComputedStyle(card).borderTopColor,
                    plainBorder: getComputedStyle(document.querySelector('[data-testid=csp-section-gap] .sui-card')).borderTopColor,
                    gap: rect(zone).top - rect(zone.previousElementSibling).bottom,
                    sameRow: actions.top < copy.bottom,
                    endGap: rect(card).right - actions.right,
                };
            }
            """);

        Assert.Multiple(() =>
        {
            Assert.That(metrics.GetProperty("title").GetString(), Is.EqualTo(metrics.GetProperty("error").GetString()));
            Assert.That(metrics.GetProperty("description").GetString(), Is.Not.EqualTo(metrics.GetProperty("error").GetString()),
                "The copy stays neutral: the border and title carry the tone.");
            Assert.That(metrics.GetProperty("border").GetString(), Is.Not.EqualTo(metrics.GetProperty("plainBorder").GetString()));
            Assert.That(metrics.GetProperty("gap").GetDouble(), Is.GreaterThan(0), "Stacked sections must never touch.");
            Assert.That(metrics.GetProperty("sameRow").GetBoolean(), Is.EqualTo(!wrapped));
            Assert.That(metrics.GetProperty("endGap").GetDouble(), Is.LessThanOrEqualTo(24), "The action sits at the end of the row.");
        });
    }
}
