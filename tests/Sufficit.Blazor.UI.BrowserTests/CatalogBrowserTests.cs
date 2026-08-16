using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Globalization;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

[Parallelizable(ParallelScope.Self)]
public sealed class CatalogBrowserTests : PageTest
{
    private static string BaseUrl
        => Environment.GetEnvironmentVariable("SUI_CATALOG_URL") ?? "http://127.0.0.1:5180";

    [SetUp]
    public async Task OpenCatalogAsync()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Catalog_HasNoSeriousOrCriticalAxeViolations()
        => await AssertNoBlockingAxeViolationsAsync();

    [Test]
    public async Task CatalogDarkTheme_HasNoSeriousOrCriticalAxeViolations()
    {
        await Page.Locator("[data-testid='theme-toggle']").ClickAsync();
        await Expect(Page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync("data-theme", "dark");
        await Page.WaitForTimeoutAsync(400);
        await AssertNoBlockingAxeViolationsAsync();
    }

    [Test]
    public async Task LegacyThemeDarkClass_ActivatesDarkFoundationTokens()
    {
        var values = await Page.EvaluateAsync<string[]>(
            """
            () => {
                document.documentElement.classList.add('theme-dark');
                const styles = getComputedStyle(document.documentElement);
                return [
                    styles.getPropertyValue('--sui-surface').trim(),
                    styles.colorScheme
                ];
            }
            """);

        Assert.That(values[0], Is.EqualTo("#0f172a"));
        Assert.That(values[1], Does.Contain("dark"));
    }

    [Test]
    public async Task BundledCss_LoadsFoundationsPortalsAndIsolatedComponents()
    {
        var cssResources = await Page.EvaluateAsync<string[]>(
            "performance.getEntriesByType('resource').map(entry => entry.name).filter(name => name.includes('.css'))");

        Assert.That(cssResources.Any(url => url.Contains("/sufficit-ui.css", StringComparison.Ordinal)), Is.True);
        Assert.That(cssResources.Any(url => url.Contains("/styles/sui-foundations.css", StringComparison.Ordinal)), Is.False);
        Assert.That(cssResources.Any(url => url.Contains("/styles/sui-portals.css", StringComparison.Ordinal)), Is.False);
        Assert.That(cssResources.Any(url => url.Contains("Sufficit.Blazor.UI.Catalog", StringComparison.Ordinal)
            && url.EndsWith(".styles.css", StringComparison.Ordinal)), Is.True);

        var styles = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const read = (selector, property) =>
                    getComputedStyle(document.querySelector(selector)).getPropertyValue(property).trim();
                return [
                    read(':root', '--sui-surface'),
                    read('.sui-choice-card', 'display'),
                    read('.sui-status-badge', 'display'),
                    read('.sui-timeline', 'display'),
                    read('.sui-timeline__item', 'position'),
                    read('.sui-empty', 'display'),
                    read('.sui-page-header', 'display'),
                    read('.sui-table', 'border-collapse'),
                    read('.sui-tabs__list', 'display')
                ];
            }
            """);

        Assert.That(styles[0], Is.Not.Empty);
        Assert.That(styles[1], Is.EqualTo("grid"));
        // The first badge is a flex item, so CSS Display blockifies
        // `inline-flex` to the computed value `flex`.
        Assert.That(styles[2], Is.EqualTo("flex"));
        Assert.That(styles[3], Is.EqualTo("flex"));
        Assert.That(styles[4], Is.EqualTo("relative"));
        Assert.That(styles[5], Is.EqualTo("flex"));
        Assert.That(styles[6], Is.EqualTo("flex"));
        Assert.That(styles[7], Is.EqualTo("collapse"));
        Assert.That(styles[8], Is.EqualTo("flex"));

        var scopedRoots = await Page.Locator(
            ".sui-choice-card, .sui-status-badge, .sui-timeline, .sui-empty, .sui-page-header, .sui-table, .sui-tabs")
            .EvaluateAllAsync<bool>(
                "elements => elements.every(element => [...element.attributes].some(attribute => attribute.name.startsWith('b-')))");
        Assert.That(scopedRoots, Is.True);
    }

    [Test]
    public async Task TimelineMarkers_AreVerticallyCentredOnTheirText()
    {
        // The marker used to be positioned with a fixed `top: 2px`, tuned by eye
        // against one font size, so it sat about 4-5px below the optical centre
        // of its label — visible in the committed catalog baselines. Assert the
        // relationship instead of the offset, so this holds at any font or
        // marker size.
        var offsets = await Page.EvaluateAsync<double[]>(
            """
            () => Array.from(document.querySelectorAll('.sui-timeline__item')).map(item => {
                const marker = item.querySelector('.sui-timeline__marker');
                const body = item.querySelector('.sui-timeline__body');
                if (!marker || !body) return 0;

                const markerBox = marker.getBoundingClientRect();

                // Measure the first line box of the label, not the whole body:
                // a multi-line item would otherwise report a centre far below
                // the line the marker is meant to align with.
                const range = document.createRange();
                range.selectNodeContents(body);
                const firstLine = range.getClientRects()[0] ?? body.getBoundingClientRect();

                const markerCentre = markerBox.top + markerBox.height / 2;
                const lineCentre = firstLine.top + firstLine.height / 2;
                return markerCentre - lineCentre;
            })
            """);

        Assert.That(offsets, Is.Not.Empty, "Catalog rendered no timeline items to check.");
        foreach (var offset in offsets)
        {
            // One pixel of slack for sub-pixel layout and font rounding.
            Assert.That(Math.Abs(offset), Is.LessThanOrEqualTo(1.0),
                $"Timeline marker is {offset:0.##}px off the centre of its first line of text.");
        }
    }

    [Test]
    public async Task HorizontalFormFields_KeepLabelsAndControlsAligned()
    {
        await Page.AddScriptTagAsync(new PageAddScriptTagOptions
        {
            Path = Path.Combine(AppContext.BaseDirectory, "audit-field-alignment.js"),
        });

        var scenarios = new[]
        {
            (Width: 1280, Language: "pt-BR", Label: "Campo indisponível", WithError: false),
            (Width: 705, Language: "en-US", Label: "International retention and operational availability policy", WithError: true),
        };

        foreach (var scenario in scenarios)
        {
            await Page.SetViewportSizeAsync(scenario.Width, 1000);
            await Page.EvaluateAsync<object?>(
                """
                args => {
                    document.documentElement.lang = args.language;
                    const field = document.querySelector('.catalog__form-grid > .sui-field:nth-child(2)');
                    field.querySelector('.sui-field__label').textContent = args.label;
                    field.querySelector('[data-alignment-error]')?.remove();
                    if (args.withError) {
                        const error = document.createElement('span');
                        error.className = 'sui-field__error';
                        error.dataset.alignmentError = '';
                        error.textContent = 'The translated value is invalid.';
                        field.append(error);
                    }
                }
                """,
                new
                {
                    language = scenario.Language,
                    label = scenario.Label,
                    withError = scenario.WithError,
                });

            var reportJson = await Page.EvaluateAsync<string>(
                "() => JSON.stringify(SUIAlignmentAudit({ requireLabels: true }))");
            using var report = JsonDocument.Parse(reportJson);
            var root = report.RootElement;
            var comparisons = root.GetProperty("comparisons").GetInt32();
            var pairs = root.GetProperty("pairs");

            Assert.That(root.GetProperty("pass").GetBoolean(), Is.True,
                $"viewport {scenario.Width}px ({scenario.Language}): {reportJson}");
            Assert.That(root.GetProperty("measuredFields").GetInt32(), Is.EqualTo(5));
            Assert.That(comparisons, Is.GreaterThanOrEqualTo(2));
            Assert.That(pairs.GetArrayLength(), Is.EqualTo(comparisons));
            Assert.That(pairs.EnumerateArray().All(pair =>
                pair.GetProperty("left").GetString()?.StartsWith('#') is true
                && pair.GetProperty("right").GetString()?.StartsWith('#') is true), Is.True);
            Assert.That(root.GetProperty("diagnostics").GetArrayLength(), Is.Zero);
        }

        await Page.Locator(".catalog__form-grid > .sui-field:nth-child(2) .sui-field__input")
            .EvaluateAsync("control => control.style.transform = 'translateY(8px)'");
        var rejectedReportJson = await Page.EvaluateAsync<string>(
            "() => JSON.stringify(SUIAlignmentAudit({ requireLabels: true }))");
        using var rejectedReport = JsonDocument.Parse(rejectedReportJson);
        var rejectedRoot = rejectedReport.RootElement;
        var rejectedFailures = rejectedRoot.GetProperty("failures").EnumerateArray();

        Assert.That(rejectedRoot.GetProperty("pass").GetBoolean(), Is.False,
            "The auditor accepted a visibly displaced control.");
        Assert.That(rejectedFailures.Any(failure =>
            failure.GetProperty("reason").GetString() == "misaligned"
            && failure.GetProperty("dimension").GetString() == "control"), Is.True);
    }

    [Test]
    public async Task ChoiceCardsAndHeaders_KeepAConsistentVerticalRhythm()
    {
        foreach (var width in new[] { 1440, 390 })
        {
            await Page.SetViewportSizeAsync(width, 1000);
            await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Expect(Page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();

            var reportJson = await Page.EvaluateAsync<string>(
                """
                () => {
                    const box = element => element.getBoundingClientRect();
                    const centerY = rectangle => rectangle.top + rectangle.height / 2;
                    const cards = [...document.querySelectorAll('.sui-choice-card')].map(card => {
                        const cardBox = box(card);
                        const content = card.querySelector('.sui-choice-card__content');
                        const title = card.querySelector('.sui-choice-card__title');
                        const description = card.querySelector('.sui-choice-card__description');
                        const indicator = card.querySelector('.sui-choice-card__indicator');
                        const titleBox = box(title);
                        const indicatorBox = box(indicator);
                        const detailed = card.classList.contains('sui-choice-card--has-description');
                        return {
                            contentRatio: box(content).width / cardBox.width,
                            titleOverflow: title.scrollWidth - title.clientWidth,
                            descriptionOverflow: description
                                ? description.scrollWidth - description.clientWidth
                                : 0,
                            indicatorDelta: detailed
                                ? Math.abs(indicatorBox.top - titleBox.top)
                                : Math.abs(centerY(indicatorBox) - centerY(titleBox)),
                        };
                    });

                    const pageHeader = box(document.querySelector('.sui-page-header'));
                    const pageTitle = document.querySelector('.sui-page-header__title');
                    const jump = box(document.querySelector('.catalog__jump'));
                    const badgeRow = box(document.querySelector('#data > .sui-stack'));
                    const typeGroup = document.querySelector('.catalog__type-sample');
                    const typeNodes = [...typeGroup.children].map(box);
                    const sectionGap = selector => {
                        const heading = document.querySelector(`${selector} > h2`);
                        return box(heading.nextElementSibling).top - box(heading).bottom;
                    };
                    const description = document.querySelector('#actions > .catalog__description');
                    const actionRow = document.querySelector('#actions > .sui-stack');

                    return JSON.stringify({
                        cards,
                        pageHeaderGap: jump.top - pageHeader.bottom,
                        pageTitleOutline: getComputedStyle(pageTitle).outlineStyle,
                        typeGroupGap: box(typeGroup).top - badgeRow.bottom,
                        typeInnerGaps: [
                            typeNodes[1].top - typeNodes[0].bottom,
                            typeNodes[2].top - typeNodes[1].bottom,
                        ],
                        directSectionGaps: ['#forms', '#navigation', '#data', '#feedback', '#layout']
                            .map(sectionGap),
                        stressGap: sectionGap('.catalog__stress'),
                        actionDescriptionGap:
                            box(description).top - box(document.querySelector('#actions > h2')).bottom,
                        actionContentGap: box(actionRow).top - box(description).bottom,
                    });
                }
                """);
            using var report = JsonDocument.Parse(reportJson);
            var root = report.RootElement;

            foreach (var card in root.GetProperty("cards").EnumerateArray())
            {
                Assert.That(card.GetProperty("contentRatio").GetDouble(), Is.GreaterThan(0.78),
                    $"choice content remained squeezed at {width}px: {reportJson}");
                Assert.That(card.GetProperty("titleOverflow").GetDouble(), Is.LessThanOrEqualTo(1));
                Assert.That(card.GetProperty("descriptionOverflow").GetDouble(), Is.LessThanOrEqualTo(1));
                Assert.That(card.GetProperty("indicatorDelta").GetDouble(), Is.LessThanOrEqualTo(1.1),
                    $"choice indicator is vertically displaced at {width}px: {reportJson}");
            }

            Assert.That(root.GetProperty("pageHeaderGap").GetDouble(), Is.InRange(23, 25));
            Assert.That(root.GetProperty("pageTitleOutline").GetString(), Is.EqualTo("none"));
            Assert.That(root.GetProperty("typeGroupGap").GetDouble(), Is.InRange(23, 25));
            Assert.That(root.GetProperty("typeInnerGaps").EnumerateArray()
                .Select(value => value.GetDouble()), Is.All.InRange(3, 5));
            Assert.That(root.GetProperty("directSectionGaps").EnumerateArray()
                .Select(value => value.GetDouble()), Is.All.InRange(23, 25));
            Assert.That(root.GetProperty("stressGap").GetDouble(), Is.InRange(23, 25));
            Assert.That(root.GetProperty("actionDescriptionGap").GetDouble(), Is.InRange(7, 9));
            Assert.That(root.GetProperty("actionContentGap").GetDouble(), Is.InRange(23, 25));
        }
    }

    private async Task AssertNoBlockingAxeViolationsAsync()
    {
        var results = await Page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions
            {
                Type = "tag",
                Values = ["wcag2a", "wcag2aa", "wcag21aa", "wcag22aa"],
            },
        });
        var blocking = results.Violations
            .Where(violation => violation.Impact is "serious" or "critical")
            .ToArray();

        Assert.That(blocking, Is.Empty,
            string.Join(Environment.NewLine, blocking.Select(violation =>
                $"{violation.Id}: {violation.Help} ({violation.Nodes.Count()} nodes)")));
    }

    [Test]
    public async Task Select_AllowsTabToReachTheNextControl()
    {
        var trigger = Page.Locator("[data-testid='select']");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("Tab");

        await Expect(Page.Locator("[data-testid='autocomplete'] input")).ToBeFocusedAsync();
    }

    [Test]
    public async Task Select_SupportsListboxKeyboardNavigation()
    {
        var trigger = Page.Locator("[data-testid='select']");
        await trigger.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");

        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        var activeId = await trigger.GetAttributeAsync("aria-activedescendant");
        Assert.That(activeId, Is.Not.Null.And.Not.Empty);
        await Expect(Page.Locator($"#{activeId}")).ToHaveAttributeAsync("role", "option");

        await Page.Keyboard.PressAsync("Enter");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(trigger).ToContainTextAsync("Sudeste");

        await Page.Keyboard.PressAsync("ArrowDown");
        await Page.Keyboard.PressAsync("Escape");
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Tabs_UseAssociatedPanelsRovingTabindexAndArrowKeys()
    {
        var tabs = Page.GetByRole(AriaRole.Tab);
        await Expect(tabs).ToHaveCountAsync(3);
        var first = tabs.Nth(0);
        var second = tabs.Nth(1);

        var firstId = await first.GetAttributeAsync("id");
        var firstPanelId = await first.GetAttributeAsync("aria-controls");
        Assert.That(firstId, Is.Not.Null.And.Not.Empty);
        Assert.That(firstPanelId, Is.Not.Null.And.Not.Empty);
        await Expect(Page.Locator($"#{firstPanelId}")).ToHaveAttributeAsync("role", "tabpanel");
        await Expect(Page.Locator($"#{firstPanelId}")).ToHaveAttributeAsync("aria-labelledby", firstId!);
        await Expect(first).ToHaveAttributeAsync("tabindex", "0");
        await Expect(second).ToHaveAttributeAsync("tabindex", "-1");

        await first.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(second).ToBeFocusedAsync();
        await Expect(second).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(second).ToHaveAttributeAsync("tabindex", "0");
        await Expect(first).ToHaveAttributeAsync("tabindex", "-1");
        await Expect(Page.GetByRole(AriaRole.Tabpanel)).ToContainTextAsync("Konfigurationseinstellungen");

        await Page.Keyboard.PressAsync("End");
        await Expect(tabs.Nth(2)).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Home");
        await Expect(first).ToBeFocusedAsync();
    }

    [Test]
    public async Task Autocomplete_SupportsComboboxKeyboardNavigation()
    {
        var input = Page.Locator("[data-testid='autocomplete'] input");
        await input.ClickAsync(); // Fill's input event is unreliable on WebKit.
        await input.PressSequentiallyAsync("s");

        await Expect(input).ToHaveAttributeAsync("aria-expanded", "true");
        await Expect(Page.Locator("[data-testid='autocomplete'] [role='option']")).Not.ToHaveCountAsync(0);
        var firstActiveId = await input.GetAttributeAsync("aria-activedescendant");
        Assert.That(firstActiveId, Is.Not.Null.And.Not.Empty);

        await Page.Keyboard.PressAsync("ArrowDown"); // End only moves the caret.
        var lastActiveId = await input.GetAttributeAsync("aria-activedescendant");
        Assert.That(lastActiveId, Is.Not.Null.And.Not.Empty);
        await Page.Keyboard.PressAsync("Enter");
        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(input).Not.ToHaveValueAsync(string.Empty);

        await input.PressAsync("ControlOrMeta+a"); // Enter left the pick behind.
        await input.PressSequentiallyAsync("s");
        await Expect(input).ToHaveAttributeAsync("aria-expanded", "true");
        await Page.Keyboard.PressAsync("Escape");
        await Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task Tooltip_WorksWithoutOpeningSelectOrRail()
    {
        var anchor = Page.Locator("[data-testid='tooltip']");
        await anchor.ScrollIntoViewIfNeededAsync();
        await Page.WaitForTimeoutAsync(100);
        await anchor.HoverAsync();

        await Expect(Page.Locator("[role='tooltip']")).ToContainTextAsync("Descrição disponível");
        await Expect(Page.Locator("[role='tooltip']")).ToHaveAttributeAsync("aria-hidden", "false");
    }

    [Test]
    public async Task InteropModules_HandleMultipleInstancesRemovalAndRecreation()
    {
        await Page.GotoAsync(new Uri(new Uri($"{BaseUrl.TrimEnd('/')}/"), "interop").ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-interop-ready]")).ToBeVisibleAsync();

        var tooltipA = Page.Locator("[data-testid='tooltip-a']");
        var tooltipB = Page.Locator("[data-testid='tooltip-b']");
        await tooltipA.HoverAsync();
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToContainTextAsync("Tooltip A");
        await tooltipB.HoverAsync();
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToContainTextAsync("Tooltip B");
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToHaveCountAsync(1);

        var railTriggers = Page.Locator(".sui-rail-trigger");
        await railTriggers.Nth(0).FocusAsync();
        await Expect(Page.Locator(".sui-rail-flyout").Nth(0)).ToBeVisibleAsync();
        await railTriggers.Nth(1).FocusAsync();
        await Expect(Page.Locator(".sui-rail-flyout").Nth(0)).ToBeHiddenAsync();
        await Expect(Page.Locator(".sui-rail-flyout").Nth(1)).ToBeVisibleAsync();

        await Page.Locator("[data-testid='select-a']").FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.Locator("[data-testid='select-a']")).ToHaveAttributeAsync("aria-expanded", "true");

        var scriptResources = await Page.EvaluateAsync<string[]>(
            "performance.getEntriesByType('resource').map(entry => entry.name).filter(name => name.endsWith('.js') || name.includes('.js?'))");
        Assert.That(scriptResources.Any(url => url.Contains("SUISelect.", StringComparison.Ordinal)
            && url.EndsWith(".razor.js", StringComparison.Ordinal)), Is.True);
        Assert.That(scriptResources.Any(url => url.Contains("SUITooltip.", StringComparison.Ordinal)
            && url.EndsWith(".razor.js", StringComparison.Ordinal)), Is.True);
        Assert.That(scriptResources.Any(url => url.Contains("SUINavGroup.", StringComparison.Ordinal)
            && url.EndsWith(".razor.js", StringComparison.Ordinal)), Is.True);
        Assert.That(scriptResources.Any(url => url.Contains("/sufficit-ui.js", StringComparison.Ordinal)), Is.False);

        await Page.Locator("[data-testid='toggle-instances']").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToHaveCountAsync(0);
        await Expect(Page.Locator(":popover-open")).ToHaveCountAsync(0);

        await Page.Locator("[data-testid='toggle-instances']").ClickAsync();
        await Page.Locator("[data-testid='tooltip-a']").HoverAsync();
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToContainTextAsync("Tooltip A");
    }

    [Test]
    public async Task CollocatedModules_WorkUnderConfiguredBasePath()
    {
        var subpathUrl = Environment.GetEnvironmentVariable("SUI_CATALOG_SUBPATH_URL");
        if (string.IsNullOrWhiteSpace(subpathUrl))
        {
            Assert.Ignore("SUI_CATALOG_SUBPATH_URL is not configured.");
        }

        await Page.GotoAsync($"{subpathUrl!.TrimEnd('/')}/",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var firstTab = Page.GetByRole(AriaRole.Tab).Nth(0);
        await firstTab.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect(Page.GetByRole(AriaRole.Tab).Nth(1)).ToBeFocusedAsync();
        var tabsModuleUrls = await Page.EvaluateAsync<string[]>(
            "performance.getEntriesByType('resource').map(entry => entry.name).filter(name => name.includes('SUITabs.') && name.endsWith('.razor.js'))");
        Assert.That(tabsModuleUrls, Is.Not.Empty);
        Assert.That(tabsModuleUrls.All(url => new Uri(url).AbsolutePath.StartsWith("/app/_content/", StringComparison.Ordinal)), Is.True);

        await Page.GotoAsync(new Uri(new Uri($"{subpathUrl!.TrimEnd('/')}/"), "interop").ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator("[data-interop-ready]")).ToBeVisibleAsync();

        var tooltip = Page.Locator("[data-testid='tooltip-a']");
        await tooltip.HoverAsync();
        await Expect(Page.GetByRole(AriaRole.Tooltip)).ToContainTextAsync("Tooltip A");
        await Page.Locator("[data-testid='select-a']").FocusAsync();
        await Page.Keyboard.PressAsync("ArrowDown");
        await Expect(Page.Locator("[data-testid='select-a']")).ToHaveAttributeAsync("aria-expanded", "true");

        var moduleUrls = await Page.EvaluateAsync<string[]>(
            "performance.getEntriesByType('resource').map(entry => entry.name).filter(name => name.includes('_content/Sufficit.Blazor.UI/') && name.endsWith('.razor.js'))");
        Assert.That(moduleUrls, Is.Not.Empty);
        Assert.That(moduleUrls.All(url => new Uri(url).AbsolutePath.StartsWith("/app/_content/", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public async Task Dialog_MovesFocusInsideAndRestoresTheTrigger()
    {
        var trigger = Page.Locator("[data-testid='open-dialog']");
        await trigger.ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await Expect(dialog).ToBeVisibleAsync();

        Assert.That(await dialog.EvaluateAsync<bool>("dialog => dialog.contains(document.activeElement)"), Is.True);
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(trigger).ToBeFocusedAsync();
    }

    [Test]
    public async Task Dialog_TrapsForwardAndBackwardTab()
    {
        await Page.Locator("[data-testid='open-dialog']").ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        var close = dialog.GetByRole(AriaRole.Button, new() { Name = "Fechar" });
        var confirm = dialog.GetByRole(AriaRole.Button, new() { Name = "Confirmar" });

        await confirm.FocusAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(close).ToBeFocusedAsync();

        await Page.Keyboard.PressAsync("Shift+Tab");
        await Expect(confirm).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Escape");
    }

    [Test]
    public async Task Table_UsesScopedHeadersFullEmptyColspanAndKeyboardRows()
    {
        var dataSection = Page.Locator("#data");
        var headers = dataSection.Locator(".sui-table").Nth(0).Locator("thead th");
        await Expect(headers).ToHaveCountAsync(3);
        Assert.That(await headers.EvaluateAllAsync<bool>(
            "elements => elements.every(element => element.getAttribute('scope') === 'col')"), Is.True);

        await Expect(dataSection.Locator(".sui-table").Nth(1).Locator("tbody td"))
            .ToHaveAttributeAsync("colspan", "3");

        var row = dataSection.Locator(".sui-table").Nth(0).Locator("tbody tr").Nth(0);
        await Expect(row).ToHaveAttributeAsync("role", "button");
        await Expect(row).ToHaveAttributeAsync("tabindex", "0");
        await row.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page.Locator(".sui-snackbar")).ToContainTextAsync("Selecionado: API de autenticação");
    }

    [Test]
    public async Task ThemeToggle_UpdatesGlobalColorScheme()
    {
        await Page.Locator("[data-testid='theme-toggle']").ClickAsync();

        await Expect(Page.Locator("[data-catalog-ready]")).ToHaveAttributeAsync("data-theme", "dark");
        var colorScheme = await Page.EvaluateAsync<string>("getComputedStyle(document.documentElement).colorScheme");
        Assert.That(colorScheme, Does.Contain("dark"));
    }

    [Test]
    public async Task PrimaryActions_UseDedicatedAccessibleFillAcrossThemes()
    {
        var root = Page.Locator("[data-catalog-ready]");
        var button = Page.Locator(".sui-btn.sui-btn--filled.sui-btn--color-primary:not(:disabled)").First;

        foreach (var theme in new[] { "light", "dark" })
        {
            await Expect(root).ToHaveAttributeAsync("data-theme", theme);
            await button.ScrollIntoViewIfNeededAsync();
            await Page.Mouse.MoveAsync(0, 0);
            await Page.WaitForTimeoutAsync(220);

            var resting = await ReadPrimaryActionAppearanceAsync(button);
            Assert.That(resting[0], Is.EqualTo("183,68,14"), $"unexpected {theme} action surface");
            Assert.That(resting[1], Is.EqualTo("255,247,237"), $"unexpected {theme} action foreground");
            Assert.That(resting[4], Is.Not.EqualTo(resting[5]),
                $"{theme} filled action reused the bright accent surface");
            Assert.That(ParseRatio(resting[2]), Is.GreaterThanOrEqualTo(4.5),
                $"insufficient resting text contrast in {theme}");
            Assert.That(ParseRatio(resting[3]), Is.GreaterThanOrEqualTo(3),
                $"insufficient action boundary contrast in {theme}");

            await button.FocusAsync();
            await Page.Keyboard.PressAsync("Shift+Tab");
            await Page.Keyboard.PressAsync("Tab");
            await Expect(button).ToBeFocusedAsync();
            var focused = await ReadPrimaryActionAppearanceAsync(button);
            Assert.That(focused[6], Is.Not.EqualTo("none"), $"missing focus outline in {theme}");

            await button.HoverAsync();
            await Page.WaitForTimeoutAsync(220);
            var hovered = await ReadPrimaryActionAppearanceAsync(button);
            Assert.That(hovered[0], Is.Not.EqualTo(resting[0]), $"missing hover feedback in {theme}");
            Assert.That(ParseRatio(hovered[2]), Is.GreaterThanOrEqualTo(4.5),
                $"insufficient hover text contrast in {theme}");

            var bounds = await button.BoundingBoxAsync();
            Assert.That(bounds, Is.Not.Null);
            await Page.Mouse.MoveAsync(bounds!.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            await Page.Mouse.DownAsync();
            try
            {
                await Page.WaitForTimeoutAsync(220);
                var pressed = await ReadPrimaryActionAppearanceAsync(button);
                Assert.That(pressed[0], Is.Not.EqualTo(hovered[0]), $"missing pressed feedback in {theme}");
                Assert.That(ParseRatio(pressed[2]), Is.GreaterThanOrEqualTo(4.5),
                    $"insufficient pressed text contrast in {theme}");
            }
            finally
            {
                await Page.Mouse.UpAsync();
            }

            if (theme == "light")
            {
                await Page.Locator("[data-testid='theme-toggle']").ClickAsync();
            }
        }
    }

    [Test]
    public async Task Catalog_DoesNotOverflowAt320PixelsOrTwoHundredPercentZoom()
    {
        await Page.SetViewportSizeAsync(320, 800);
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        Assert.That(await HasHorizontalOverflowAsync(), Is.False, "horizontal overflow at 320px");

        await Page.SetViewportSizeAsync(640, 900);
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.EvaluateAsync("document.documentElement.style.zoom = '2'");
        Assert.That(await HasHorizontalOverflowAsync(), Is.False, "horizontal overflow at 200% zoom");
    }

    [Test]
    public async Task Catalog_SupportsRtlWithoutHorizontalOverflow()
    {
        await Page.SetViewportSizeAsync(320, 800);
        await Page.EvaluateAsync("document.documentElement.dir = 'rtl'");
        Assert.That(await HasHorizontalOverflowAsync(), Is.False, "horizontal overflow in RTL at 320px");

        var markerNearLogicalStart = await Page.Locator(".sui-timeline__item").Nth(0)
            .EvaluateAsync<bool>(
                "item => { const marker = item.querySelector('.sui-timeline__marker').getBoundingClientRect(); const row = item.getBoundingClientRect(); return Math.abs(row.right - marker.right) < 2; }");
        Assert.That(markerNearLogicalStart, Is.True);
    }

    [Test]
    public async Task TouchViewport_ProvidesFortyFourPixelCriticalTargets()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var selectors = new[]
        {
            "[data-testid='theme-toggle']",
            ".sui-field__input",
            ".sui-select__trigger",
            ".sui-switch",
            ".sui-tab",
            ".sui-alert__close",
        };
        foreach (var selector in selectors)
        {
            var box = await Page.Locator(selector).Nth(0).BoundingBoxAsync();
            Assert.That(box, Is.Not.Null, $"missing touch target: {selector}");
            Assert.That(box!.Height, Is.GreaterThanOrEqualTo(44), $"short touch target: {selector}");
        }

        await Page.Locator("[data-testid='open-dialog']").ClickAsync();
        var closeBox = await Page.GetByRole(AriaRole.Dialog)
            .GetByRole(AriaRole.Button, new() { Name = "Fechar" }).BoundingBoxAsync();
        Assert.That(closeBox!.Width, Is.GreaterThanOrEqualTo(44));
        Assert.That(closeBox.Height, Is.GreaterThanOrEqualTo(44));
        await Page.Keyboard.PressAsync("Escape");
    }

    [Test]
    public async Task ReducedMotion_DisablesComponentAnimationsAndTransitions()
    {
        await Page.EmulateMediaAsync(new PageEmulateMediaOptions { ReducedMotion = ReducedMotion.Reduce });
        await Page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var motion = await Page.EvaluateAsync<string[]>(
            """
            () => {
                const style = selector => getComputedStyle(document.querySelector(selector));
                return [
                    style('.sui-progress-circular').animationName,
                    style('.sui-progress-linear__bar').transitionDuration,
                    style('.sui-field__input').transitionDuration,
                    style('.sui-select__trigger').transitionDuration,
                    style('.sui-switch__track').transitionDuration,
                    style('.sui-tab').transitionDuration
                ];
            }
            """);
        Assert.That(motion[0], Is.EqualTo("none"));
        Assert.That(motion.Skip(1), Is.All.EqualTo("0s"));

        await Page.Locator("[data-testid='open-dialog']").ClickAsync();
        Assert.That(await Page.Locator(".sui-dialog-overlay")
            .EvaluateAsync<string>("element => getComputedStyle(element).animationName"), Is.EqualTo("none"));
        await Page.Keyboard.PressAsync("Escape");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mostrar snackbar" }).ClickAsync();
        Assert.That(await Page.Locator(".sui-snackbar")
            .EvaluateAsync<string>("element => getComputedStyle(element).animationName"), Is.EqualTo("none"));
    }

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

    [Test]
    public async Task Catalog_RemainsOperableInForcedColors()
    {
        if (!string.Equals(BrowserName, "chromium", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("Forced-colors contract is exercised once in Chromium.");
        }

        await using var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ForcedColors = ForcedColors.Active,
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(page.Locator("[data-catalog-ready]")).ToBeVisibleAsync();

        Assert.That(await page.EvaluateAsync<bool>("matchMedia('(forced-colors: active)').matches"), Is.True);
        foreach (var selector in new[]
        {
            ".sui-btn",
            ".sui-field__input",
            ".sui-select__trigger",
            ".sui-tab",
            ".sui-alert",
        })
        {
            await Expect(page.Locator(selector).First).ToBeVisibleAsync();
        }

        var themeToggle = page.Locator("[data-testid='theme-toggle']");
        await themeToggle.FocusAsync();
        var focus = await themeToggle.EvaluateAsync<string[]>(
            "element => { const style = getComputedStyle(element); return [style.outlineStyle, style.outlineWidth]; }");
        Assert.That(focus[0], Is.Not.EqualTo("none"));
        Assert.That(focus[1], Is.Not.EqualTo("0px"));
    }

    private async Task<bool> HasHorizontalOverflowAsync()
        => await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");

    private static double ParseRatio(string value)
        => double.Parse(value, CultureInfo.InvariantCulture);

    private static async Task<string[]> ReadPrimaryActionAppearanceAsync(ILocator button)
        => await button.EvaluateAsync<string[]>(
            """
            element => {
                const canvas = document.createElement('canvas');
                canvas.width = canvas.height = 1;
                const context = canvas.getContext('2d', { willReadFrequently: true });
                const rgb = value => {
                    context.clearRect(0, 0, 1, 1);
                    context.fillStyle = '#000';
                    context.fillStyle = value;
                    context.fillRect(0, 0, 1, 1);
                    return [...context.getImageData(0, 0, 1, 1).data.slice(0, 3)];
                };
                const luminance = color => {
                    const channels = color.map(value => value / 255)
                        .map(value => value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4);
                    return .2126 * channels[0] + .7152 * channels[1] + .0722 * channels[2];
                };
                const contrast = (left, right) => {
                    const values = [luminance(left), luminance(right)].sort((a, b) => b - a);
                    return ((values[0] + .05) / (values[1] + .05)).toFixed(3);
                };
                const style = getComputedStyle(element);
                const rootStyle = getComputedStyle(document.documentElement);
                const background = rgb(style.backgroundColor);
                const foreground = rgb(style.color);
                const border = rgb(style.borderTopColor);
                const canvasBackground = rgb(getComputedStyle(document.body).backgroundColor);
                return [
                    background.join(','),
                    foreground.join(','),
                    contrast(background, foreground),
                    contrast(border, canvasBackground),
                    rootStyle.getPropertyValue('--sui-color-primary-action').trim(),
                    rootStyle.getPropertyValue('--sui-color-primary').trim(),
                    style.outlineStyle,
                ];
            }
            """);

    private async Task<VisualComparison> ComparePngsInBrowserAsync(string expectedPath, string actualPath)
    {
        var reportJson = await Page.EvaluateAsync<string>(
            """
            async images => {
                const load = source => new Promise((resolve, reject) => {
                    const image = new Image();
                    image.onload = () => resolve(image);
                    image.onerror = reject;
                    image.src = `data:image/png;base64,${source}`;
                });
                const expected = await load(images.expected);
                const actual = await load(images.actual);
                if (expected.width !== actual.width || expected.height !== actual.height) {
                    return JSON.stringify({
                        dimensionsMatch: false,
                        expectedWidth: expected.width,
                        expectedHeight: expected.height,
                        actualWidth: actual.width,
                        actualHeight: actual.height,
                        differentPixels: 0,
                        totalPixels: Math.max(expected.width * expected.height, actual.width * actual.height),
                        diffRatio: 1,
                        changedBounds: 'dimensions',
                    });
                }

                const canvas = document.createElement('canvas');
                canvas.width = expected.width;
                canvas.height = expected.height;
                const context = canvas.getContext('2d', { willReadFrequently: true });
                context.drawImage(expected, 0, 0);
                const expectedPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                context.clearRect(0, 0, canvas.width, canvas.height);
                context.drawImage(actual, 0, 0);
                const actualPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                let differentPixels = 0;
                let minX = canvas.width;
                let minY = canvas.height;
                let maxX = -1;
                let maxY = -1;
                for (let offset = 0; offset < expectedPixels.length; offset += 4) {
                    const delta = Math.max(
                        Math.abs(expectedPixels[offset] - actualPixels[offset]),
                        Math.abs(expectedPixels[offset + 1] - actualPixels[offset + 1]),
                        Math.abs(expectedPixels[offset + 2] - actualPixels[offset + 2]),
                        Math.abs(expectedPixels[offset + 3] - actualPixels[offset + 3]));
                    if (delta <= 24) continue;
                    differentPixels++;
                    const pixel = offset / 4;
                    const x = pixel % canvas.width;
                    const y = Math.floor(pixel / canvas.width);
                    minX = Math.min(minX, x);
                    minY = Math.min(minY, y);
                    maxX = Math.max(maxX, x);
                    maxY = Math.max(maxY, y);
                }
                const totalPixels = canvas.width * canvas.height;
                return JSON.stringify({
                    dimensionsMatch: true,
                    expectedWidth: expected.width,
                    expectedHeight: expected.height,
                    actualWidth: actual.width,
                    actualHeight: actual.height,
                    differentPixels,
                    totalPixels,
                    diffRatio: differentPixels / totalPixels,
                    changedBounds: differentPixels === 0 ? 'none' : `${minX},${minY}-${maxX},${maxY}`,
                });
            }
            """,
            new
            {
                expected = Convert.ToBase64String(await File.ReadAllBytesAsync(expectedPath)),
                actual = Convert.ToBase64String(await File.ReadAllBytesAsync(actualPath)),
            });
        using var report = JsonDocument.Parse(reportJson);
        var root = report.RootElement;
        return new VisualComparison(
            root.GetProperty("dimensionsMatch").GetBoolean(),
            root.GetProperty("expectedWidth").GetInt32(),
            root.GetProperty("expectedHeight").GetInt32(),
            root.GetProperty("actualWidth").GetInt32(),
            root.GetProperty("actualHeight").GetInt32(),
            root.GetProperty("differentPixels").GetInt32(),
            root.GetProperty("totalPixels").GetInt32(),
            root.GetProperty("diffRatio").GetDouble(),
            root.GetProperty("changedBounds").GetString() ?? "unknown");
    }

    private sealed record VisualScenario(string FileName, int Width, int Height, bool Dark);

    private sealed record VisualComparison(
        bool DimensionsMatch,
        int ExpectedWidth,
        int ExpectedHeight,
        int ActualWidth,
        int ActualHeight,
        int DifferentPixels,
        int TotalPixels,
        double DiffRatio,
        string ChangedBounds);
}
