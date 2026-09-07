using Microsoft.Playwright;
using System.Text.Json;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [Test]
    public async Task Playground_ExportsTheVisibleVariantAndDisabledState()
    {
        await Page.GotoAsync(BaseUrl + "?component=SUIButton");
        await ChooseAsync("Variante da demonstração", "Contorno");
        await Page.GetByLabel("Desabilitado", new() { Exact = true }).PressAsync("Space");
        await Expect(Page.Locator(".playground-result button")).ToBeDisabledAsync();
        await Expect(Page.Locator(".playground code")).ToContainTextAsync("SUIVariant.Outlined");
        await Expect(Page.Locator(".playground code")).ToContainTextAsync("Disabled=\"true\"");
    }

    [Test]
    public async Task AdvancedTheme_PersistsAndComparisonKeepsItsOwnPalette()
    {
        await Page.GotoAsync(BaseUrl + "?view=themes");
        await Page.GetByLabel("Raio das bordas (px)").FillAsync("12");
        await Page.GetByLabel("Raio das bordas (px)").PressAsync("Tab");
        await Expect(Page.Locator(".source-panel code")).ToContainTextAsync("Radius = \"12px\"");
        await Page.ReloadAsync();
        await Expect(Page.GetByLabel("Raio das bordas (px)")).ToHaveValueAsync("12");
        await Page.GetByLabel("Abrir comparação interativa").PressAsync("Space");
        var light = Page.FrameLocator("iframe[title='Prévia interativa do tema claro']");
        var dark = Page.FrameLocator("iframe[title='Prévia interativa do tema escuro']");
        await Expect(light.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "light");
        await Expect(dark.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "dark");
        await dark.GetByLabel("Nome da operação").FillAsync("Comparação escura");
        await Expect(light.GetByLabel("Nome da operação")).ToHaveValueAsync("Operação Sudeste");
    }

    [Test]
    public async Task CompleteWorkspace_HandlesErrorFilteringAndValidatedEditing()
    {
        await Page.GotoAsync(BaseUrl + "?view=patterns");
        await ChooseAsync("Estado da demonstração", "Erro");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Tentar novamente", Exact = true })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Tentar novamente", Exact = true }).ClickAsync();
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Serviços", Exact = true }).ClickAsync();
        await Page.GetByLabel("Buscar serviço", new() { Exact = true }).FillAsync("demonstração 03");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Editar Serviço de demonstração 03", Exact = true }).ClickAsync();
        await Page.GetByLabel("Nome do serviço", new() { Exact = true }).FillAsync("");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar serviço", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Informe o nome do serviço.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByLabel("Nome do serviço", new() { Exact = true }).FillAsync("Serviço de demonstração 03 revisado");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar serviço", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Serviço atualizado nesta demonstração.", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task ThemeValidationAndPendingChangesReflectRealState()
    {
        await Page.GotoAsync(BaseUrl + "?view=themes");
        await Expect(Page.GetByText("_colorError", new() { Exact = true })).ToHaveCountAsync(0);
        await Page.GetByLabel("Cor de erro", new() { Exact = true }).FillAsync("invalid");
        await Page.GetByLabel("Cor de erro", new() { Exact = true }).PressAsync("Tab");
        await Expect(Page.GetByLabel("Cor de erro", new() { Exact = true })).ToHaveAttributeAsync("aria-invalid", "true");
        await Expect(Page.GetByLabel("Cor de sucesso", new() { Exact = true })).Not.ToHaveAttributeAsync("aria-invalid", "true");
        await Page.GetByLabel("Cor de erro", new() { Exact = true }).FillAsync("#991b1b");
        await Page.GetByLabel("Cor de erro", new() { Exact = true }).PressAsync("Tab");
        await Expect(Page.GetByText("Use # seguido de seis dígitos hexadecimais.", new() { Exact = true })).ToHaveCountAsync(0);
        await Page.GotoAsync(BaseUrl + "?view=patterns");
        await Expect(Page.GetByLabel("Nome da seção", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.Locator(".sui-pending-changes-bar")).ToHaveCountAsync(0);
        await Page.GetByLabel("Nome da seção", new() { Exact = true }).FillAsync("Preferência revisada");
        await Page.GetByLabel("Nome da seção", new() { Exact = true }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name="Salvar tudo",Exact=true }).ClickAsync();
        await Expect(Page.Locator(".sui-pending-changes-bar")).ToHaveCountAsync(0);
        await Page.GetByLabel("Nome da seção", new() { Exact = true }).FillAsync("Rascunho");
        await Page.GetByLabel("Nome da seção", new() { Exact = true }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name="Cancelar alterações",Exact=true }).ClickAsync();
        await Expect(Page.GetByLabel("Nome da seção", new() { Exact = true })).ToHaveValueAsync("Preferência revisada");
    }

    [Test]
    public async Task CopyButtonsSendTheDisplayedSourceToClipboard()
    {
        foreach (var (query, button, selector) in new[] {
            ("?view=themes", "Copiar C#", ".source-panel code"),
            ("?component=SUIButton", "Copiar código", ".component-source code"),
            ("?component=SUIButton", "Copiar configuração", ".playground code"),
            ("?view=patterns", "Copiar tela", ".source-panel code") })
        {
            await Page.GotoAsync(BaseUrl + query);
            await Expect(Page.Locator(".site-header")).ToBeVisibleAsync();
            if (query.Contains("patterns")) await Page.GetByText("Código da tela completa", new() { Exact=true }).ClickAsync();
            var source = await Page.Locator(selector).InnerTextAsync();
            await Page.EvaluateAsync("Object.defineProperty(navigator, 'clipboard', { configurable: true, value: { writeText: async text => { window.copiedSource = text; } } })");
            await Page.GetByRole(AriaRole.Button, new() { Name=button,Exact=true }).ClickAsync();
            await Page.WaitForFunctionAsync("window.copiedSource !== undefined");
            Assert.That(await Page.EvaluateAsync<string>("window.copiedSource"), Is.EqualTo(source));
        }
    }

    [Test]
    public async Task ColdWasmLoad_RecordsPayloadAndTimeToInteraction()
    {
        if (BrowserName != "chromium") Assert.Ignore("Cold resource timing baseline uses Chromium.");
        await using var fresh = await Browser.NewContextAsync();
        var page = await fresh.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForFunctionAsync("performance.getEntriesByName('sui-interactive').length > 0");
        await page.GetByLabel("Nome", new() { Exact = true }).FillAsync("Medição de interação");
        await page.GetByRole(AriaRole.Button, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        await Expect(page.GetByLabel("Nome", new() { Exact = true })).ToHaveValueAsync("Operação Sudeste");
        var metrics = await page.EvaluateAsync<JsonElement>("""
            () => {
                const entries = performance.getEntriesByType('resource');
                return { interactiveMs: performance.getEntriesByName('sui-interactive')[0].startTime,
                    verifiedInteractionMs: performance.now(),
                    transferBytes: entries.reduce((sum, entry) => sum + entry.transferSize, 0),
                    decodedBytes: entries.reduce((sum, entry) => sum + entry.decodedBodySize, 0),
                    frameworkBytes: entries.filter(e => e.name.includes('/_framework/')).reduce((sum, e) => sum + e.decodedBodySize, 0),
                    requests: entries.length };
            }
            """);
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "showcase-wasm-performance.json");
        File.WriteAllText(path, metrics.ToString());
        TestContext.AddTestAttachment(path);
        TestContext.Out.WriteLine(metrics);
        Assert.That(metrics.GetProperty("interactiveMs").GetDouble(), Is.LessThan(5000));
        Assert.That(metrics.GetProperty("decodedBytes").GetInt64(), Is.LessThan(11 * 1024 * 1024));
    }
}
