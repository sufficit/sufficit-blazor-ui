using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed class FieldHelperBrowserTests : PageTest
{
    [TestCase(1280, "light")]
    [TestCase(1280, "dark")]
    [TestCase(390, "light")]
    [TestCase(390, "dark")]
    public async Task HelpersShareTypographySpacingAndConsumerOverrides(int width, string theme)
    {
        var url = Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.GotoAsync($"{url.TrimEnd('/')}/fixtures/field-helpers?dark={theme == "dark"}");
        await Expect(Page.Locator("[data-helper-ready=true]")).ToBeVisibleAsync();
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", theme);
        await Page.EvaluateAsync("document.fonts.ready");
        var fields = Page.Locator(".sui-field");
        Assert.That(await fields.CountAsync(), Is.EqualTo(6));
        await VerifyHelpers(fields, 11.04, 4);

        // A consumer can change the shared rule once, without fighting isolated
        // TextField styles or specifying separate rules for each field type.
        await Page.AddStyleTagAsync(new()
        {
            Content = ".catalog .sui-field__helper { font-size:14px; margin-top:6px; }"
        });
        await VerifyHelpers(fields, 14, 10);
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
    }

    private async Task VerifyHelpers(ILocator fields, double fontSize, double gap)
    {
        foreach (var field in await fields.AllAsync())
        {
            var helper = field.Locator(".sui-field__helper");
            var control = field.Locator("input, textarea, button[aria-haspopup]").First;
            var helperBox = (await helper.BoundingBoxAsync())!;
            var controlBox = (await control.BoundingBoxAsync())!;
            Assert.That(helperBox.Y - controlBox.Y - controlBox.Height, Is.EqualTo(gap).Within(.1));
            Assert.That(await helper.EvaluateAsync<double>("el => parseFloat(getComputedStyle(el).fontSize)"), Is.EqualTo(fontSize).Within(.01));
            Assert.That(await helper.EvaluateAsync<string>("el => getComputedStyle(el).fontStyle"), Is.EqualTo("italic"));
            var id = await helper.GetAttributeAsync("id");
            Assert.That((await control.GetAttributeAsync("aria-describedby"))?.Split(' '), Does.Contain(id));
        }
    }
}
