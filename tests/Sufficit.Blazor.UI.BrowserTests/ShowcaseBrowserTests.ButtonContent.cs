using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(390)]
    [TestCase(1280)]
    public async Task ButtonContentHonorsAlignmentAndWrapsLongLabels(int width)
    {
        await Page.SetViewportSizeAsync(width, 900);
        await Page.GotoAsync(BaseUrl + "?component=SUIButton");
        var button = Page.Locator(".component-preview").GetByRole(AriaRole.Button,
            new() { Name = "Conferir resultado", Exact = true });
        await Expect(button).ToBeVisibleAsync();
        var metrics = await button.EvaluateAsync<double[]>(
            """
            button => {
                const indicator = button.querySelector('[aria-hidden="true"]');
                const bounds = button.getBoundingClientRect();
                const styles = getComputedStyle(button);
                return [bounds.right - indicator.getBoundingClientRect().right,
                    parseFloat(styles.paddingRight) + parseFloat(styles.borderRightWidth)];
            }
            """);
        Assert.That(metrics[0], Is.EqualTo(metrics[1]).Within(1),
            "The trailing indicator must follow the public space-between alignment.");
        var longLabel = Page.Locator(".component-preview [data-testid='button-long-content']");
        Assert.That(await longLabel.EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth"), Is.True);
    }
}
