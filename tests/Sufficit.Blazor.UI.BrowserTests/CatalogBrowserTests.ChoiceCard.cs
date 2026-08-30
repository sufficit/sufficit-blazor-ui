using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class CatalogBrowserTests
{
    [Test]
    public async Task ChoiceCard_CustomTrailingContent_ReflowsWithoutHorizontalOverflow()
    {
        foreach (var (width, height) in new[] { (1440, 900), (390, 844) })
        {
            await Page.SetViewportSizeAsync(width, height);
            await Page.GotoAsync(BaseUrl, new() { WaitUntil = Microsoft.Playwright.WaitUntilState.NetworkIdle });
            await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();

            var reportJson = await Page.EvaluateAsync<string>(
                """
                () => {
                    const card = document.querySelector('.sui-choice-card');
                    const content = card.querySelector('.sui-choice-card__content');
                    const trailing = card.querySelector('.sui-choice-card__indicator');
                    card.classList.add('sui-choice-card--has-custom-trailing');
                    card.style.setProperty('--_choice-trailing-track', 'minmax(min-content,15rem)');
                    trailing.className = 'sui-choice-card__trailing sui-choice-card__trailing--custom';
                    trailing.textContent = 'Nenhuma oferta compatível agora';

                    const box = element => element.getBoundingClientRect();
                    const cardBox = box(card);
                    const contentBox = box(content);
                    const trailingBox = box(trailing);
                    return JSON.stringify({
                        cardWidth: cardBox.width,
                        contentWidth: contentBox.width,
                        trailingWidth: trailingBox.width,
                        stacked: trailingBox.top >= contentBox.bottom - 1,
                        cardOverflow: card.scrollWidth - card.clientWidth,
                        pageOverflow: document.documentElement.scrollWidth
                            - document.documentElement.clientWidth,
                    });
                }
                """);

            using var report = JsonDocument.Parse(reportJson);
            var root = report.RootElement;
            Assert.That(root.GetProperty("cardOverflow").GetDouble(), Is.LessThanOrEqualTo(0));
            Assert.That(root.GetProperty("pageOverflow").GetDouble(), Is.LessThanOrEqualTo(0));
            Assert.That(root.GetProperty("trailingWidth").GetDouble(), Is.GreaterThan(100));

            if (width < 600)
            {
                Assert.That(root.GetProperty("stacked").GetBoolean(), Is.True);
                Assert.That(
                    root.GetProperty("contentWidth").GetDouble(),
                    Is.GreaterThan(root.GetProperty("cardWidth").GetDouble() * .75));
            }
            else
            {
                Assert.That(root.GetProperty("stacked").GetBoolean(), Is.False);
            }
        }
    }
}
