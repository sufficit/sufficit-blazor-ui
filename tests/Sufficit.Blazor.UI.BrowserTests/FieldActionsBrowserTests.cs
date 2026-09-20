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
        // Both boxes are measured, so both have to be laid out first. Waiting for
        // visibility is necessary but not sufficient: the circuit can re-render
        // and replace the element between the assertion and the measurement, and
        // a single read then answers null. That failed on WebKit while Chromium
        // and Firefox passed, and passed on re-run with no code change, so the
        // reads poll until the box has area.
        await Expect(input).ToBeVisibleAsync();
        await Expect(button).ToBeVisibleAsync();
        var fieldBox = await input.RequireBoundingBoxAsync("The field");
        var buttonBox = await button.RequireBoundingBoxAsync("The action");
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
