using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    private Task OpenMigrationExample(string name) => Page.GotoAsync(
        new Uri(new Uri(BaseUrl.TrimEnd('/') + "/"), "?component=" + name).ToString());

    [TestCase(1280)]
    [TestCase(390)]
    public async Task PopupEscapesClippingSupportsInputAndDismissesWithFocusRestoration(int width)
    {
        await Page.SetViewportSizeAsync(width, 844);
        await OpenMigrationExample("SUIPopup");
        var preview = Page.Locator(".component-preview");
        await preview.EvaluateAsync("element => { element.style.overflow = 'hidden'; element.style.height = '60px'; }");
        var trigger = preview.GetByRole(AriaRole.Button, new() { Name = "Pesquisar no menu", Exact = true });
        await trigger.ClickAsync();
        var panel = preview.GetByRole(AriaRole.Dialog, new() { Name = "Pesquisar no menu" });
        await Expect(panel).ToBeVisibleAsync();
        await panel.GetByLabel("Pesquisar", new() { Exact = true }).FillAsync("Telefonia");
        await Expect(panel.GetByLabel("Pesquisar", new() { Exact = true })).ToHaveValueAsync("Telefonia");
        var box = await panel.BoundingBoxAsync();
        Assert.That(box!.X, Is.GreaterThanOrEqualTo(0));
        Assert.That(box.X + box.Width, Is.LessThanOrEqualTo(width));
        await Page.Keyboard.PressAsync("Escape");
        await Expect(panel).Not.ToBeVisibleAsync();
        await Expect(trigger).ToBeFocusedAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await trigger.ClickAsync();
        await Expect(panel).ToBeVisibleAsync();
        await Page.Locator(".site-header").ClickAsync();
        await Expect(panel).Not.ToBeVisibleAsync();
        await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Test]
    public async Task RegisteredFormRulesBlockAndThenAllowSubmission()
    {
        await OpenMigrationExample("SUIForm");
        var preview = Page.Locator(".component-preview");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Validar e continuar" }).ClickAsync();
        await Expect(preview.GetByLabel("Nome", new() { Exact = true })).ToHaveAttributeAsync("aria-invalid", "true");
        await Expect(preview.GetByText("Formulário validado.")).Not.ToBeVisibleAsync();
        await preview.GetByLabel("Nome", new() { Exact = true }).FillAsync("Equipe Sufficit");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Validar e continuar" }).ClickAsync();
        await Expect(preview.GetByText("Formulário validado.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ServerTablePaginationReplacesTheCurrentPage()
    {
        await OpenMigrationExample("SUIDataTable");
        var preview = Page.Locator(".component-preview");
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 1", Exact = true })).ToBeVisibleAsync();
        await preview.GetByRole(AriaRole.Button, new() { Name = "Próxima página", Exact = true }).ClickAsync();
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 6", Exact = true })).ToBeVisibleAsync();
        await Expect(preview.GetByRole(AriaRole.Cell, new() { Name = "Registro 1", Exact = true })).Not.ToBeVisibleAsync();
    }
    [TestCase(1280)]
    [TestCase(390)]
    public async Task GridGapAndWideDialogRespectControlledGeometry(int width)
    {
        await Page.SetViewportSizeAsync(width, 844);
        await OpenMigrationExample("SUIGrid");
        var preview = Page.Locator(".component-preview");
        var field = preview.GetByLabel("Espaçamento do grid");
        await field.FillAsync("8"); await field.PressAsync("Tab");
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.component-preview .sui-grid')).gap === '32px'");
        await field.FillAsync("0"); await field.PressAsync("Tab");
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.component-preview .sui-grid')).gap === '0px'");
        await OpenMigrationExample("SUIDialogHost");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Abrir diálogo", Exact = true }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog, new() { Name = "Editor de demonstração", Exact = true });
        await Expect(dialog).ToBeVisibleAsync();
        var box = await dialog.BoundingBoxAsync();
        Assert.That(box!.Width, Is.EqualTo(Math.Min(width * .92, 960)).Within(2));
        await Page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeVisibleAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        await Expect(dialog).Not.ToBeVisibleAsync();
        await OpenMigrationExample("SUIDivider");
        var vertical = Page.Locator("[role=separator][aria-orientation=vertical]");
        var separatorBox = await vertical.BoundingBoxAsync();
        Assert.That(separatorBox!.Width, Is.EqualTo(1).Within(1));
        Assert.That(separatorBox.Height, Is.GreaterThan(10));
    }

    [Test]
    public async Task UnknownProgressRemainsVisibleWithReducedMotionWithoutClaimingAPercentage()
    {
        await OpenMigrationExample("SUIProgressLinear");
        var indicator = Page.GetByRole(AriaRole.Progressbar, new() { Name = "Carregando registros" });
        await Expect(indicator).ToBeVisibleAsync();
        Assert.That(await indicator.GetAttributeAsync("aria-valuenow"), Is.Null);
        var bar = indicator.Locator(".sui-progress-linear__bar");
        Assert.That(await bar.EvaluateAsync<string>("el => getComputedStyle(el).animationName"), Is.EqualTo("sui-progress-indeterminate"));
        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await Page.WaitForFunctionAsync("getComputedStyle(document.querySelector('.sui-progress-linear--indeterminate .sui-progress-linear__bar')).animationName === 'none'");
        var width = (await bar.BoundingBoxAsync())!.Width;
        Assert.That(width, Is.GreaterThan(0));
        Assert.That(width, Is.LessThan((await indicator.BoundingBoxAsync())!.Width));
    }

    [Test]
    public async Task NativeDateClockAndRangeRoundTripInBrazilianCulture()
    {
        await using var context = await Browser.NewContextAsync(new() { Locale = "pt-BR", TimezoneId = "America/Sao_Paulo" });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(new Uri(BaseUrl.TrimEnd('/') + "/"), "?component=SUITextField").ToString());
        var date = page.GetByLabel("Data editável", new() { Exact = true });
        await Expect(date).ToHaveValueAsync("2026-08-01");
        await date.FillAsync("2026-08-02"); await date.PressAsync("Tab");
        var clock = page.GetByLabel("Horário editável", new() { Exact = true });
        await clock.FillAsync("10:45:12.250"); await clock.PressAsync("Tab");
        await page.GetByLabel("Fração", new() { Exact = true }).EvaluateAsync("el => { el.value = '0.75'; el.dispatchEvent(new Event('input', { bubbles: true })); }");
        await Expect(page.GetByTestId("native-temporal-values")).ToHaveTextAsync("2026-08-02|10:45:12.2500000|0.75");
        await Expect(date).ToHaveValueAsync("2026-08-02");
        await Expect(clock).ToHaveValueAsync("10:45:12.25");
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task MultilineParentRestoreAndClearReplaceTheUsersDirtyValue(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUITextField");
        var preview = Page.Locator(".component-preview");
        var notes = preview.GetByLabel("Observações", new() { Exact = true });
        await notes.FillAsync("Texto digitado pelo usuário");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Restaurar observações", Exact = true }).ClickAsync();
        await Expect(notes).ToHaveValueAsync("Texto recuperado");
        await notes.FillAsync("Outra edição");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Limpar observações", Exact = true }).ClickAsync();
        await Expect(notes).ToHaveValueAsync(string.Empty);
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task MobileTableRulesDoNotChangeAnUnstackedNestedTable(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUITable");
        var owner = Page.GetByTestId("nested-table-owner");
        var nested = Page.GetByTestId("nested-table-child");
        Assert.That(await nested.Locator("tbody td").First.EvaluateAsync<string>("cell => getComputedStyle(cell).display"), Is.EqualTo("table-cell"));
        Assert.That(await nested.Locator("thead").EvaluateAsync<string>("head => getComputedStyle(head).position"), Is.EqualTo("static"));
        Assert.That(await owner.Locator(":scope > tbody > tr > td").First.EvaluateAsync<string>("cell => getComputedStyle(cell).display"), Is.EqualTo(width < 600 ? "grid" : "table-cell"));
        var cell = await nested.Locator("tbody td").First.BoundingBoxAsync();
        Assert.That(cell!.Height, Is.LessThan(100));
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task MultipleFileSelectionClearsAndRejectsCountOverflowWithoutUploading(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUIFileUpload");
        var preview = Page.Locator(".component-preview");
        var input = preview.GetByLabel("Arquivos de áudio", new() { Exact = true });
        FilePayload File(string name) => new() { Name = name, MimeType = "audio/wav", Buffer = [0, 1] };
        await input.SetInputFilesAsync(new[] { File("a.wav"), File("b.wav") });
        await Expect(preview.GetByTestId("selected-file-names")).ToHaveTextAsync("a.wav, b.wav");
        await preview.GetByRole(AriaRole.Button, new() { Name = "Descartar arquivos", Exact = true }).ClickAsync();
        await Expect(preview.GetByTestId("selected-file-names")).ToBeEmptyAsync();
        Assert.That(await input.EvaluateAsync<int>("input => input.files.length"), Is.Zero);
        await input.SetInputFilesAsync(new[] { File("a.wav"), File("b.wav"), File("c.wav"), File("d.wav") });
        await Expect(preview.GetByRole(AriaRole.Alert)).ToHaveTextAsync("O número de arquivos excede o limite permitido.");
        await Expect(preview.GetByTestId("selected-file-names")).ToBeEmptyAsync();
        await input.SetInputFilesAsync(File("c.wav"));
        await Expect(preview.GetByTestId("selected-file-names")).ToHaveTextAsync("c.wav");
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task WideMaterializedTableHasNativeKeyboardHorizontalScroll(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUITable");
        var region = Page.GetByRole(AriaRole.Region, new() { Name = "Serviços com rolagem horizontal", Exact = true });
        await Expect(region).ToHaveAttributeAsync("tabindex", "0");
        Assert.That(await region.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth"), Is.True);
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        var rows = await region.Locator("tbody tr").CountAsync();
        await region.FocusAsync();
        await Page.Keyboard.PressAsync("ArrowRight");
        await Page.WaitForFunctionAsync("element => element.scrollLeft > 0", await region.ElementHandleAsync());
        await Expect(region).ToBeFocusedAsync();
        await Expect(region).ToHaveCSSAsync("outline-style", "solid");
        Assert.That(await region.Locator("tbody tr").CountAsync(), Is.EqualTo(rows));
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task DisabledNativeLinksCannotNavigateAndEnabledIconHrefStillWorks(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUIIconButton");
        var preview = Page.Locator(".component-preview");
        var icon = preview.GetByRole(AriaRole.Link, new() { Name = "Referência do exemplo", Exact = true });
        var text = preview.GetByRole(AriaRole.Link, new() { Name = "Abrir registro de exemplo", Exact = true });
        var originalUrl = Page.Url;
        foreach (var link in new[] { icon, text })
        {
            await Expect(link).ToHaveAttributeAsync("aria-disabled", "true");
            Assert.That(await link.GetAttributeAsync("href"), Is.Null);
            await Expect(link).ToHaveAttributeAsync("tabindex", "-1");
            await link.FocusAsync();
            await Page.Keyboard.PressAsync("Enter");
            Assert.That(Page.Url, Is.EqualTo(originalUrl));
        }
        await preview.GetByRole(AriaRole.Button, new() { Name = "Atualizar exemplo", Exact = true }).FocusAsync();
        await Page.Keyboard.PressAsync("Tab");
        await Expect(preview.GetByRole(AriaRole.Button, new() { Name = "Alternar links do exemplo", Exact = true })).ToBeFocusedAsync();
        await Page.Keyboard.PressAsync("Space");
        await Expect(icon).ToHaveAttributeAsync("href", "#native-link-target");
        await icon.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(Page).ToHaveURLAsync(new Uri(new Uri(BaseUrl), "#native-link-target").ToString());
    }

    [TestCase(390)]
    [TestCase(1280)]
    public async Task WrappedFilterChipsKeepCompleteLabelsAndIndependentKeyboardActions(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await OpenMigrationExample("SUIChip");
        var host = Page.GetByTestId("wrapped-filter-chips");
        var chips = host.Locator(".sui-chip");
        await Expect(chips).ToHaveCountAsync(2);
        for (var index=0;index<2;index++)
        {
            var chip = chips.Nth(index); var box = await chip.BoundingBoxAsync();
            Assert.That(box!.X + box.Width, Is.LessThanOrEqualTo(width));
            var label = await chip.Locator(".sui-chip__label").BoundingBoxAsync();
            Assert.That(label!.Y + label.Height, Is.LessThanOrEqualTo(box.Y + box.Height + 1));
            Assert.That(await chip.EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth + 1"), Is.True);
        }
        await Expect(chips.First.Locator(".sui-chip__label")).ToHaveTextAsync("Período: 17/11/2026 00:00 até 17/11/2026 23:59 (incluindo as datas)");
        if(width<600) Assert.That((await chips.First.Locator(".sui-chip__label").BoundingBoxAsync())!.Height, Is.GreaterThan(24));
        await host.GetByRole(AriaRole.Button,new(){Name="Editar período do exemplo",Exact=true}).FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(host.GetByTestId("wrapped-chip-action")).ToHaveTextAsync("edit");
        await host.GetByRole(AriaRole.Button,new(){Name="Remover período do exemplo",Exact=true}).FocusAsync();
        await Page.Keyboard.PressAsync("Enter");
        await Expect(host.GetByTestId("wrapped-chip-action")).ToHaveTextAsync("remove");
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
    }

}
