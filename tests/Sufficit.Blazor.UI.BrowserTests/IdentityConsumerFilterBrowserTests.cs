using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

[Parallelizable(ParallelScope.Self)]
public sealed class IdentityConsumerFilterBrowserTests : PageTest
{
    private static string BaseUrl
        => Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";

    [SetUp]
    public async Task OpenCatalogAsync()
    {
        await Page.GotoAsync($"{BaseUrl.TrimEnd('/')}/fixtures/identity-users-filter",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
    }

    [Test]
    public async Task IdentityUsersFilterFixture_StaysInsideTracksAndUsesStandardButtons()
    {
        await Page.SetViewportSizeAsync(1418, 900);
        await Page.AddScriptTagAsync(new PageAddScriptTagOptions
        {
            Path = Path.Combine(AppContext.BaseDirectory, "audit-field-alignment.js"),
        });

        var reportJson = await Page.EvaluateAsync<string>(
            """
            () => {
                const grid = document.querySelector('[data-testid="identity-filter-grid"]');
                const fields = [...grid.querySelectorAll(':scope > .sui-field')];
                const controlFor = field => field.querySelector(
                    ':scope > [data-sui-field-control], :scope > input, :scope > textarea, '
                    + ':scope > .sui-text-field__control > input, .sui-select__trigger, .sui-date-field__trigger');
                const measures = fields.map(field => {
                    const fieldBox = field.getBoundingClientRect();
                    const control = controlFor(field);
                    if (!control) {
                        throw new Error(`No measurable control found for ${field.className}`);
                    }
                    const controlBox = control.getBoundingClientRect();
                    return {
                        name: field.dataset.suiAlignName || control.id,
                        fieldLeft: fieldBox.left,
                        fieldRight: fieldBox.right,
                        fieldWidth: fieldBox.width,
                        controlLeft: controlBox.left,
                        controlRight: controlBox.right,
                        controlWidth: controlBox.width,
                        controlHeight: controlBox.height,
                        overflowLeft: Math.max(0, fieldBox.left - controlBox.left),
                        overflowRight: Math.max(0, controlBox.right - fieldBox.right),
                        boxSizing: getComputedStyle(control).boxSizing,
                    };
                });
                const buttons = [...document.querySelectorAll('[data-testid="identity-filter-actions"] button')]
                    .map(button => {
                        const box = button.getBoundingClientRect();
                        const style = getComputedStyle(button);
                        return {
                            text: button.textContent.trim(),
                            width: box.width,
                            height: box.height,
                            fontSize: style.fontSize,
                            boxSizing: style.boxSizing,
                        };
                    });
                return JSON.stringify({
                    gridColumns: getComputedStyle(grid).gridTemplateColumns,
                    measures,
                    buttons,
                });
            }
            """);

        using var report = JsonDocument.Parse(reportJson);
        var root = report.RootElement;
        var fields = root.GetProperty("measures").EnumerateArray().ToArray();
        var buttons = root.GetProperty("buttons").EnumerateArray().ToArray();

        Assert.That(fields, Has.Length.EqualTo(9), reportJson);
        Assert.That(fields.Max(field => field.GetProperty("overflowLeft").GetDouble()),
            Is.LessThanOrEqualTo(1), reportJson);
        Assert.That(fields.Max(field => field.GetProperty("overflowRight").GetDouble()),
            Is.LessThanOrEqualTo(1), reportJson);
        Assert.That(fields.Select(field => field.GetProperty("boxSizing").GetString()),
            Is.All.EqualTo("border-box"), reportJson);
        Assert.That(fields.Select(field => field.GetProperty("controlHeight").GetDouble()),
            Is.All.EqualTo(36).Within(1), reportJson);
        Assert.That(buttons.Select(button => button.GetProperty("height").GetDouble()),
            Is.All.EqualTo(36).Within(1), reportJson);
        Assert.That(buttons.Select(button => button.GetProperty("fontSize").GetString()),
            Is.All.EqualTo("14px"), reportJson);

        var alignmentJson = await Page.EvaluateAsync<string>(
            """
            () => JSON.stringify(SUIAlignmentAudit({
                containerSelector: '[data-testid="identity-filter-grid"]',
                fieldSelector: ':scope > .sui-field',
                requireLabels: true,
            }))
            """);
        using var alignment = JsonDocument.Parse(alignmentJson);
        Assert.That(alignment.RootElement.GetProperty("pass").GetBoolean(), Is.True, alignmentJson);
        Assert.That(alignment.RootElement.GetProperty("comparisons").GetInt32(),
            Is.GreaterThan(0), alignmentJson);
    }

    [TestCase("pt-BR", "01/08/2026", "agosto de 2026", "Hoje", "Limpar", "2026-08-02", "02/08/2026")]
    [TestCase("en-US", "8/1/2026", "August 2026", "Today", "Clear", "2026-08-02", "8/2/2026")]
    public async Task DateField_UsesSuiThemeCultureAndKeyboard(
        string culture,
        string initialValue,
        string month,
        string today,
        string clear,
        string nextIsoDate,
        string nextValue)
    {
        await Page.GotoAsync(
            $"{BaseUrl.TrimEnd('/')}/fixtures/identity-users-filter?culture={culture}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
        await Page.SetViewportSizeAsync(1418, 900);

        var trigger = Page.Locator("#fixture-registered-from");
        await Expect(trigger).ToContainTextAsync(initialValue);
        await Expect(Page.Locator("input[type=date]")).ToHaveCountAsync(0);
        await trigger.ClickAsync();

        var dialog = Page.Locator("#fixture-registered-from-calendar");
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog.Locator(".sui-date-field__month")).ToHaveTextAsync(month);
        await Expect(dialog.Locator(".sui-date-field__action").Nth(0)).ToHaveTextAsync(today);
        await Expect(dialog.Locator(".sui-date-field__action").Nth(1)).ToHaveTextAsync(clear);

        var styleJson = await dialog.EvaluateAsync<string>(
            """
            element => {
                const box = element.getBoundingClientRect();
                const style = getComputedStyle(element);
                const selectStyle = getComputedStyle(document.querySelector('.sui-select__menu:not(.sui-date-field__popover)'));
                return JSON.stringify({
                    position: style.position,
                    background: style.backgroundColor,
                    selectBackground: selectStyle.backgroundColor,
                    borderRadius: style.borderRadius,
                    width: box.width,
                    insideViewport: box.left >= 0 && box.top >= 0
                        && box.right <= innerWidth && box.bottom <= innerHeight,
                });
            }
            """);
        using var style = JsonDocument.Parse(styleJson);
        Assert.That(style.RootElement.GetProperty("position").GetString(), Is.EqualTo("fixed"), styleJson);
        Assert.That(style.RootElement.GetProperty("background").GetString(),
            Is.EqualTo(style.RootElement.GetProperty("selectBackground").GetString()), styleJson);
        Assert.That(style.RootElement.GetProperty("borderRadius").GetString(), Is.EqualTo("14px"), styleJson);
        Assert.That(style.RootElement.GetProperty("width").GetDouble(), Is.EqualTo(304).Within(1), styleJson);
        Assert.That(style.RootElement.GetProperty("insideViewport").GetBoolean(), Is.True, styleJson);

        var selectedDay = dialog.Locator("[data-sui-date='2026-08-01']");
        await Expect(selectedDay).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        var nextDay = dialog.Locator($"[data-sui-date='{nextIsoDate}']");
        await Expect(nextDay).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(trigger).ToContainTextAsync(nextValue);
        await Expect(trigger).ToBeFocusedAsync();
    }
}
