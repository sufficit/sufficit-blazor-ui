using System.Text.Json;
using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// The single-screen conference view (<c>?view=all</c>): every catalogued
/// component rendered at once, the way the final test screenshot shows them.
/// The expectations come from catalog.json, so adding a component never
/// requires editing a number here — it must simply appear on the screen.
/// </summary>
public sealed partial class ShowcaseBrowserTests
{
    private static string[] CataloguedNames()
    {
        using var catalog = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "catalog.json")));
        return catalog.RootElement.EnumerateArray()
            .Select(entry => entry.GetProperty("name").GetString()!)
            .ToArray();
    }

    [TestCase(1440, 1000)]
    [TestCase(390, 844)]
    public async Task AllComponents_RendersEveryCataloguedComponentOnOneScreen(int width, int height)
    {
        var errors = new List<string>();
        Page.PageError += (_, error) => errors.Add(error);
        Page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };

        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync($"{BaseUrl}?view=all", new() { WaitUntil = WaitUntilState.NetworkIdle });
        var screen = Page.Locator("[data-all-components-ready]");
        await Expect(screen).ToBeVisibleAsync();

        var expected = CataloguedNames();
        await Expect(screen).ToHaveAttributeAsync("data-component-count", expected.Length.ToString());

        var rendered = await Page.Locator(".all-components__item h3")
            .EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.textContent.trim())");
        Assert.That(rendered, Is.EquivalentTo(expected),
            "Every catalogued component must appear on the single conference screen.");

        // A preview that rendered nothing is a component silently missing from
        // the screenshot, which is exactly what this view exists to catch.
        var empty = await Page.Locator(".all-components__preview").EvaluateAllAsync<string[]>(
            """
            nodes => nodes
                .filter(node => node.childElementCount === 0 && !node.textContent.trim())
                .map(node => node.closest('.all-components__item').querySelector('h3').textContent.trim())
            """);
        Assert.That(empty, Is.Empty, "Empty preview(s): " + string.Join(", ", empty));

        Assert.That(await Page.Locator("#blazor-error-ui").IsVisibleAsync(), Is.False);
        Assert.That(await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > innerWidth + 1"), Is.False,
            $"horizontal overflow at {width}px");
        Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
    }

    [Test]
    public async Task AllComponents_ContainsViewportFixedPreviewsInsideTheirCard()
    {
        // Drawer, pending-changes bar and toast are position:fixed by design.
        // Left alone they would float over the page instead of appearing in
        // their own card, so the conference screenshot would show them once,
        // stacked, instead of in place.
        await Page.GotoAsync($"{BaseUrl}?view=all", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-all-components-ready]")).ToBeVisibleAsync();

        var escaping = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const bad = new Set();
                for (const preview of document.querySelectorAll('.all-components__preview')) {
                    const box = preview.getBoundingClientRect();
                    for (const child of preview.querySelectorAll('*')) {
                        if (getComputedStyle(child).position !== 'fixed') continue;
                        if (child.getBoundingClientRect().height > box.height + 8)
                            bad.add(preview.closest('.all-components__item').querySelector('h3').textContent.trim());
                    }
                }
                return [...bad];
            }
            """);

        Assert.That(escaping, Is.Empty,
            "Preview(s) escaping their card: " + string.Join(", ", escaping));
    }

    [Test]
    public async Task AllComponents_IsReachableFromTheNavigation()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Todos em uma tela", Exact = true })
            .First.ClickAsync();
        await Expect(Page.Locator("[data-all-components-ready]")).ToBeVisibleAsync();

        // Query-string routing has to survive a reload on static hosting.
        await Page.ReloadAsync();
        await Expect(Page.Locator("[data-all-components-ready]")).ToBeVisibleAsync();
    }
}
