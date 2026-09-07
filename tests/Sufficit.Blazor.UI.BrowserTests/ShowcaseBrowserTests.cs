using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed class ShowcaseBrowserTests : PageTest
{
    private string BaseUrl => Environment.GetEnvironmentVariable("SUI_SHOWCASE_URL")!;

    [SetUp]
    public async Task OpenAsync()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUI_SHOWCASE_URL")))
            Assert.Ignore("Set SUI_SHOWCASE_URL to a published static showcase.");
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(Page.Locator(".site-header")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Overview_CancelRestoresTheVisibleForm()
    {
        await Page.GetByLabel("Nome", new() { Exact = true }).FillAsync("Equipe alterada");
        await Page.GetByLabel("Tentativas", new() { Exact = true }).FillAsync("7");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancelar", Exact = true }).ClickAsync();
        await Expect(Page.GetByLabel("Nome", new() { Exact = true })).ToHaveValueAsync("Operação Sudeste");
        await Expect(Page.GetByLabel("Tentativas", new() { Exact = true })).ToHaveValueAsync("3");
    }

    [Test]
    public async Task EveryComponent_HasAnExecutablePageAndSurvivesReload()
    {
        var errors = new List<string>();
        Page.PageError += (_, error) => errors.Add(error);
        Page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        var links = await Page.Locator(".component-index a").EvaluateAllAsync<string[]>("links => links.map(link => link.href)");
        Assert.That(links, Has.Length.EqualTo(68));
        foreach (var link in links)
        {
            await Page.GotoAsync(link);
            await Expect(Page.Locator(".component-preview")).ToBeVisibleAsync();
            await Expect(Page.Locator(".source-panel code")).Not.ToBeEmptyAsync();
            Assert.That(await Page.Locator("#blazor-error-ui").IsVisibleAsync(), Is.False, link);
        }
        await Page.ReloadAsync();
        await Expect(Page.Locator(".component-preview")).ToBeVisibleAsync();
        Assert.That(errors, Is.Empty, string.Join(Environment.NewLine, errors));
    }

    [Test]
    public async Task SearchAndQueryNavigation_WorkOnStaticHosting()
    {
        await Page.Locator("#component-search").FillAsync("SUISelect");
        await Expect(Page.Locator(".nav-family a")).ToHaveCountAsync(2);
        await Page.Locator(".nav-family a", new() { HasText = "SUISelect" }).First.ClickAsync();
        await Expect(Page.Locator("main h1")).ToHaveTextAsync("SUISelect");
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Região", Exact = true }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Sudeste", Exact = true }).ClickAsync();
        await Page.ReloadAsync();
        await Expect(Page.Locator("main h1")).ToHaveTextAsync("SUISelect");
    }

    [TestCase(1440, 1000, "light")]
    [TestCase(390, 844, "dark")]
    public async Task Themes_AndVisibleSnackbar_PassAccessibility(int width, int height, string mode)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync(BaseUrl + "?view=themes");
        await Page.GetByLabel("Tema", new() { Exact = true }).SelectOptionAsync(mode);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar preferências", Exact = true }).ClickAsync();
        await Expect(Page.Locator(".sui-snackbar")).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(250);
        var result = await Page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = "tag", Values = ["wcag2a", "wcag2aa", "wcag21aa", "wcag22aa"] },
        });
        Assert.That(result.Violations, Is.Empty, string.Join("\n", result.Violations.Select(v => $"{v.Id}: {v.Help}")));
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        await Page.ReloadAsync();
        await Expect(Page.GetByLabel("Tema", new() { Exact = true })).ToHaveValueAsync(mode);
    }

    [Test]
    public async Task SystemTheme_RespondsToDeviceChanges()
    {
        await Page.GetByLabel("Tema", new() { Exact = true }).SelectOptionAsync("system");
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "dark");
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "light");
    }

    [Test]
    public async Task FormValidation_AndPagination_WorkWithoutServer()
    {
        await Page.GotoAsync(BaseUrl + "?view=patterns");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar formulário", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Informe o nome público.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByLabel("Nome público", new() { Exact = true }).FillAsync("Equipe Sul");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar formulário", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Formulário validado com sucesso.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Próxima página", Exact = true }).ClickAsync();
        await Expect(Page.GetByText("Serviço de demonstração 06", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Serviço de demonstração 01", new() { Exact = true })).ToHaveCountAsync(0);
    }
}
