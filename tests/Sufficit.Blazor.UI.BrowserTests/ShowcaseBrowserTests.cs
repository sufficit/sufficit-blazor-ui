using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests : PageTest
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
            await Expect(Page.Locator(".component-source code")).Not.ToBeEmptyAsync();
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
        await ChooseAsync("Tema", mode == "dark" ? "Escuro" : "Claro");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Salvar preferências", Exact = true }).ClickAsync();
        await Expect(Page.Locator(".sui-snackbar")).ToBeVisibleAsync();
        await Select("Tema").ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listbox, new() { Name = "Tema", Exact = true })).ToBeVisibleAsync();
        await Page.WaitForTimeoutAsync(250);
        await Expect(Select("Tema")).ToHaveAttributeAsync("aria-expanded", "true");
        // Sample the composited color-mix fill; axe may omit this contrast check.
        var selectedOption = Page.GetByRole(AriaRole.Listbox, new() { Name = "Tema", Exact = true })
            .GetByRole(AriaRole.Option, new() { Selected = true });
        var contrast = await selectedOption.EvaluateAsync<double>("""
            element => {
                const canvas = document.createElement('canvas');
                canvas.width = canvas.height = 1;
                const ctx = canvas.getContext('2d');
                const style = getComputedStyle(element);
                ctx.fillStyle = getComputedStyle(element.closest('[role=listbox]')).backgroundColor;
                ctx.fillRect(0, 0, 1, 1);
                ctx.fillStyle = style.backgroundColor;
                ctx.fillRect(0, 0, 1, 1);
                const background = ctx.getImageData(0, 0, 1, 1).data;
                ctx.fillStyle = style.color;
                ctx.fillRect(0, 0, 1, 1);
                const foreground = ctx.getImageData(0, 0, 1, 1).data;
                const luminance = rgb => [0.2126, 0.7152, 0.0722].reduce((sum, weight, i) => {
                    const c = rgb[i] / 255;
                    return sum + weight * (c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4);
                }, 0);
                const a = luminance(background), b = luminance(foreground);
                return (Math.max(a, b) + 0.05) / (Math.min(a, b) + 0.05);
            }
            """);
        Assert.That(contrast, Is.GreaterThanOrEqualTo(4.5), "Selected menu text must remain readable over its composited fill.");
        var result = await Page.RunAxe(new AxeRunOptions
        {
            RunOnly = new RunOnlyOptions { Type = "tag", Values = ["wcag2a", "wcag2aa", "wcag21aa", "wcag22aa"] },
        });
        Assert.That(result.Violations, Is.Empty, string.Join("\n", result.Violations.Select(v => $"{v.Id}: {v.Help}")));
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
        await Page.ReloadAsync();
        await Expect(Select("Tema")).ToHaveTextAsync(mode == "dark" ? "Escuro" : "Claro");
    }

    [Test]
    public async Task SystemTheme_RespondsToDeviceChanges()
    {
        await ChooseAsync("Tema", "Sistema");
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "dark");
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Light });
        await Expect(Page.Locator(".sui-root")).ToHaveAttributeAsync("data-sui-theme", "light");
    }

    [TestCase(1440, 1000)]
    [TestCase(390, 844)]
    public async Task ThemeSelectors_UseThemedMenusAndPreservePreferences(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await Page.GotoAsync(BaseUrl + "?view=themes");
        await Expect(Page.Locator("select")).ToHaveCountAsync(0);
        await Page.GetByLabel("Nome público", new() { Exact = true }).FillAsync("Equipe Sul");
        var theme = Select("Tema");
        await theme.FocusAsync();
        await theme.PressAsync("End");
        await Expect(theme).ToHaveAttributeAsync("aria-expanded", "true");
        await theme.PressAsync("Enter");
        await Expect(theme).ToHaveTextAsync("Escuro");
        await Expect(theme).ToHaveAttributeAsync("aria-expanded", "false");
        await Expect(theme).ToBeFocusedAsync();
        await Expect(Select("Aparência")).ToHaveTextAsync("Escuro");
        await ChooseAsync("Paleta de demonstração", "Azul");
        await ChooseAsync("Densidade", "Compacta");
        await Expect(Page.GetByLabel("Nome público", new() { Exact = true })).ToHaveValueAsync("Equipe Sul");
        await Page.ReloadAsync();
        await Expect(Select("Tema")).ToHaveTextAsync("Escuro");
        await Expect(Select("Paleta de demonstração")).ToHaveTextAsync("Azul");
        await Expect(Select("Densidade")).ToHaveTextAsync("Compacta");
        foreach (var label in new[] { "Tema", "Paleta de demonstração", "Aparência", "Densidade" })
        {
            await Select(label).ClickAsync();
            var menu = Page.GetByRole(AriaRole.Listbox, new() { Name = label, Exact = true });
            await Expect(menu).ToBeVisibleAsync();
            Assert.That(await menu.EvaluateAsync<bool>("element => { const box = element.getBoundingClientRect(); return box.left >= 0 && box.right <= innerWidth && box.top >= 0 && box.bottom <= innerHeight; }"), Is.True, label);
            Assert.That(await menu.EvaluateAsync<bool>("element => getComputedStyle(element).backgroundColor === getComputedStyle(document.querySelector('.site-header')).backgroundColor"), Is.True, label);
            var result = await Page.RunAxe(new AxeRunOptions
            {
                RunOnly = new RunOnlyOptions { Type = "tag", Values = ["wcag2a", "wcag2aa", "wcag21aa", "wcag22aa"] },
            });
            Assert.That(result.Violations, Is.Empty, string.Join("\n", result.Violations.Select(v => $"{v.Id}: {v.Help}")));
            await Select(label).PressAsync("Escape");
            await Expect(menu).Not.ToBeVisibleAsync();
        }
        await ChooseAsync("Aparência", "Claro");
        await Expect(Select("Tema")).ToHaveTextAsync("Claro");
        Assert.That(await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"), Is.True);
    }

    private ILocator Select(string label) => Page.GetByRole(AriaRole.Combobox, new() { Name = label, Exact = true });

    private async Task ChooseAsync(string label, string option)
    {
        await Select(label).ClickAsync();
        await Page.GetByRole(AriaRole.Listbox, new() { Name = label, Exact = true })
            .GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
        await Expect(Select(label)).ToHaveTextAsync(option);
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
