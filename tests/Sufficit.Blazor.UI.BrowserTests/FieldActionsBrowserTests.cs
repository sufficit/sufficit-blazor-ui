using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

[Parallelizable(ParallelScope.Self)]
public sealed class FieldActionsBrowserTests : PageTest
{
    [TestCase(1280)]
    [TestCase(390)]
    public async Task LabeledFieldAndAction_AlignAndWrapWithoutLosingInteraction(int width)
    {
        var url = Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";
        await Page.SetViewportSizeAsync(width, 900);
        await Page.GotoAsync($"{url.TrimEnd('/')}/fixtures/field-actions", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var toolbar = Page.GetByTestId("field-actions-toolbar");
        var input = toolbar.GetByLabel("Buscar no console");
        var button = toolbar.GetByRole(AriaRole.Button, new() { Name = "Pausar exibição" });
        await Expect(input).ToBeVisibleAsync();
        var fieldBox = (await input.BoundingBoxAsync())!;
        var buttonBox = (await button.BoundingBoxAsync())!;
        if (width > 600)
        {
            Assert.That(buttonBox.Y + buttonBox.Height,
                Is.EqualTo(fieldBox.Y + fieldBox.Height).Within(1), "Actions align with the input, not its label.");
        }
        else
        {
            Assert.That(buttonBox.Y, Is.GreaterThanOrEqualTo(fieldBox.Y + fieldBox.Height + 8));
        }
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        await input.FillAsync("arquivo");
        await Expect(toolbar.GetByRole(AriaRole.Status)).ToContainTextAsync("Filtro: arquivo");
        await button.ClickAsync();
        await Expect(toolbar.GetByRole(AriaRole.Button, new() { Name = "Retomar exibição" }))
            .ToHaveAttributeAsync("aria-pressed", "true");
    }
}
