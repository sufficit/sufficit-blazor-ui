using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Action, navigation and overlay gaps found while migrating the identity
/// management UI: button spinner/focus/icon names, localizable pagination,
/// progress steps and sort labels, tooltip rich content, expansion panel
/// slots, switch button semantics and the choice card action mode.
/// </summary>
public sealed class ConsumerGapsActionTests
{
    private static BunitContext Create()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Buttons_SynchronousClick_NeverShowsTheSpinner()
    {
        using var context = Create();
        var clicks = 0;
        var button = context.Render<SUIButton>(p => p
            .Add(c => c.OnClick, (MouseEventArgs _) => clicks++)
            .AddChildContent("Menu"));
        button.Find("button").Click();
        Assert.Equal(1, clicks);
        Assert.DoesNotContain("sui-btn--loading", button.Find("button").ClassList);
        Assert.Empty(button.FindAll(".sui-progress-circular"));

        var icon = context.Render<SUIIconButton>(p => p
            .Add(c => c.Title, "Menu")
            .Add(c => c.Icon, "menu")
            .Add(c => c.OnClick, (MouseEventArgs _) => clicks++));
        icon.Find("button").Click();
        Assert.Equal(2, clicks);
        Assert.Empty(icon.FindAll(".sui-progress-circular"));
    }

    [Fact]
    public async Task Buttons_LongAsyncClick_ShowsSpinnerAfterTheDelay()
    {
        using var context = Create();
        var gate = new TaskCompletionSource();
        var button = context.Render<SUIIconButton>(p => p
            .Add(c => c.Title, "Salvar")
            .Add(c => c.Icon, "check")
            .Add(c => c.LoadingDelay, 0)
            .Add(c => c.OnClick, (MouseEventArgs _) => gate.Task));

        var click = button.Find("button").ClickAsync(new MouseEventArgs());
        button.WaitForAssertion(() => Assert.NotEmpty(button.FindAll(".sui-progress-circular")));
        gate.SetResult();
        await click;
        button.WaitForAssertion(() => Assert.Empty(button.FindAll(".sui-progress-circular")));
    }

    [Fact]
    public async Task Buttons_AutoLoadingFalse_NeverShowsTheSpinnerForALongHandler()
    {
        using var context = Create();
        var gate = new TaskCompletionSource();
        var button = context.Render<SUIButton>(p => p
            .Add(c => c.AutoLoading, false)
            .Add(c => c.LoadingDelay, 0)
            .Add(c => c.OnClick, (MouseEventArgs _) => gate.Task)
            .AddChildContent("Abrir diálogo"));

        var click = button.Find("button").ClickAsync(new MouseEventArgs());
        await Task.Delay(50);
        Assert.Empty(button.FindAll(".sui-progress-circular"));
        Assert.Null(button.Find("button").GetAttribute("aria-busy"));
        gate.SetResult();
        await click;
    }

    [Fact]
    public void Buttons_SpinnerTakesTheLabelPlaceInsteadOfOverlappingIt()
    {
        var css = File.ReadAllText(Path.Combine(RepositoryLayout.Styles, "sui-buttons.css"));
        Assert.Contains(".sui-btn:is(.sui-btn--pending, .sui-btn--loading) > .sui-btn__label { opacity: 0; }", css);
        var pending = css[css.IndexOf(".sui-btn--pending::after", StringComparison.Ordinal)..];
        pending = pending[..pending.IndexOf('}')];
        Assert.DoesNotContain("inset-inline-end", pending);
        Assert.Contains("margin: auto", pending);
    }

    [Fact]
    public void Td_LabelledCellWrapsSeveralChildrenInOneValueBox()
    {
        using var context = Create();
        var cut = context.Render<SUITd>(p => p
            .Add(c => c.DataLabel, "Cliente")
            .AddChildContent("<strong>Portal</strong><small>portal-web</small>"));
        var td = cut.Find("td");
        Assert.Equal(1, td.ChildElementCount);
        var value = td.FirstElementChild!;
        Assert.Contains("sui-td__value", value.ClassList);
        Assert.Equal(2, value.ChildElementCount);

        var plain = context.Render<SUITd>(p => p.AddChildContent("<b>a</b><i>b</i>"));
        Assert.Equal(2, plain.Find("td").ChildElementCount);
        Assert.Empty(plain.FindAll(".sui-td__value"));
    }

    [Fact]
    public void Buttons_LoadingParameter_ForcesTheSpinner()
    {
        using var context = Create();
        var button = context.Render<SUIButton>(p => p.Add(c => c.Loading, true).AddChildContent("Salvar"));
        Assert.Equal("true", button.Find("button").GetAttribute("aria-busy"));
        Assert.True(button.Find("button").HasAttribute("disabled"));

        var icon = context.Render<SUIIconButton>(p => p.Add(c => c.Title, "x").Add(c => c.Loading, true));
        Assert.Contains("sui-btn--loading", icon.Find("button").ClassList);
    }

    [Fact]
    public void Buttons_AcceptIconNamesBesideSvgMarkup()
    {
        using var context = Create();
        var button = context.Render<SUIButton>(p => p
            .Add(c => c.StartIcon, "arrow-left")
            .Add(c => c.EndIcon, SUIIcons.ChevronRight)
            .AddChildContent("Voltar"));
        var icons = button.FindAll(".sui-btn__icon svg");
        Assert.Equal(2, icons.Count);
        Assert.Contains("path", icons[0].InnerHtml);
        Assert.Equal("currentColor", icons[1].GetAttribute("fill"));

        var icon = context.Render<SUIIconButton>(p => p.Add(c => c.Title, "Excluir").Add(c => c.Icon, "trash"));
        Assert.Contains("<path", icon.Find(".sui-icon svg").InnerHtml);
    }

    [Theory]
    [InlineData("eye")]
    [InlineData("user")]
    [InlineData("unlock")]
    [InlineData("download")]
    [InlineData("upload")]
    [InlineData("more-vertical")]
    [InlineData("close")]
    [InlineData("delete")]
    [InlineData("external-link")]
    public void Icon_NewNames_DoNotFallBackToTheCircle(string name)
    {
        using var context = Create();
        var cut = context.Render<SUIIcon>(p => p.Add(c => c.Name, name));
        Assert.NotEqual("<circle cx=\"12\" cy=\"12\" r=\"9\"></circle>", cut.Find("svg").InnerHtml.Trim());
    }

    [Fact]
    public async Task IconButton_FocusAsync_FocusesTheElement()
    {
        using var context = Create();
        var cut = context.Render<SUIIconButton>(p => p.Add(c => c.Title, "Fechar"));
        await cut.InvokeAsync(() => cut.Instance.FocusAsync(preventScroll: true).AsTask());
        Assert.Contains(context.JSInterop.Invocations, i => i.Identifier.Contains("focus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ProgressSteps_PositionLabel_IsLocalizable()
    {
        using var context = Create();
        var steps = new[] { "Client", "Scopes" };
        var cut = context.Render<SUIProgressSteps>(p => p
            .Add(c => c.Steps, steps)
            .Add(c => c.StepPositionLabel, (n, total) => $"Step {n} of {total}")
            .Add(c => c.CurrentStatusLabel, "current"));
        Assert.Equal("Step 1 of 2: Client, current", cut.FindAll("button")[0].GetAttribute("aria-label"));

        var plain = context.Render<SUIProgressSteps>(p => p.Add(c => c.Steps, steps));
        Assert.Equal("Etapa 1 de 2: Client, atual", plain.FindAll("button")[0].GetAttribute("aria-label"));
    }

    [Fact]
    public void Pagination_Labels_AreLocalizableAndDefaultsUnchanged()
    {
        using var context = Create();
        var cut = context.Render<SUIPagination>(p => p
            .Add(c => c.TotalItems, 30)
            .Add(c => c.PageSize, 10)
            .Add(c => c.PreviousLabel, "Previous page")
            .Add(c => c.NextLabel, "Next page")
            .Add(c => c.PageLabel, (n, total) => $"Page {n} of {total}")
            .Add(c => c.RangeFormatter, (first, last, total) => $"{first}-{last} of {total}"));
        Assert.Equal("Previous page", cut.FindAll("button")[0].GetAttribute("aria-label"));
        Assert.Equal("Next page", cut.FindAll("button")[1].GetAttribute("aria-label"));
        Assert.Equal("Page 1 of 3", cut.Find(".sui-pagination__page").TextContent.Trim());
        Assert.Contains("1-10 of 30", cut.Markup);

        var empty = context.Render<SUIPagination>(p => p.Add(c => c.EmptyText, "No clients"));
        Assert.Contains("No clients", empty.Markup);

        var plain = context.Render<SUIPagination>(p => p.Add(c => c.TotalItems, 30));
        Assert.Equal("Página anterior", plain.FindAll("button")[0].GetAttribute("aria-label"));
        Assert.Equal("Página 1 de 2", plain.Find(".sui-pagination__page").TextContent.Trim());
    }

    [Fact]
    public void Pagination_PageHref_RendersLinksAndDisabledEnds()
    {
        using var context = Create();
        var cut = context.Render<SUIPagination>(p => p
            .Add(c => c.TotalItems, 30)
            .Add(c => c.PageSize, 10)
            .Add(c => c.PageIndex, 1)
            .Add(c => c.PageHref, index => $"?page={index + 1}"));
        var links = cut.FindAll("a");
        Assert.Equal(new[] { "?page=1", "?page=3" }, links.Select(a => a.GetAttribute("href")));

        var first = context.Render<SUIPagination>(p => p
            .Add(c => c.TotalItems, 30)
            .Add(c => c.PageSize, 10)
            .Add(c => c.PageHref, index => $"?page={index + 1}"));
        Assert.Single(first.FindAll("a"));
        Assert.True(first.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void Pagination_Disabled_DisablesEveryControl()
    {
        using var context = Create();
        var cut = context.Render<SUIPagination>(p => p
            .Add(c => c.TotalItems, 30)
            .Add(c => c.PageSize, 10)
            .Add(c => c.PageIndex, 1)
            .Add(c => c.Disabled, true));
        Assert.All(cut.FindAll("button.sui-btn"), b => Assert.True(b.HasAttribute("disabled")));
    }

    [Fact]
    public void SortLabel_StateSuffix_IsLocalizable()
    {
        using var context = Create();
        var cut = context.Render<SUITableSortLabel<object>>(p => p
            .Add(c => c.SortDirection, SUISortDirection.Descending)
            .Add(c => c.DescendingLabel, ", ordem decrescente")
            .AddChildContent("Nome"));
        Assert.Equal(", ordem decrescente", cut.Find(".sui-sr-only").TextContent);

        var plain = context.Render<SUITableSortLabel<object>>(p => p.AddChildContent("Nome"));
        Assert.Equal(", not sorted", plain.Find(".sui-sr-only").TextContent);
    }

    [Fact]
    public void Tooltip_RichContent_RendersHiddenSourceAndPersistentDescription()
    {
        using var context = Create();
        var rich = context.Render<SUITooltip>(p => p
            .Add(c => c.Title, "Escopos")
            .Add(c => c.Content, (RenderFragment)(b => b.AddMarkupContent(0, "Use <code>openid</code>.")))
            .AddChildContent("<button>?</button>"));
        var anchor = rich.Find(".sui-tooltip-anchor");
        var sourceId = anchor.GetAttribute("data-sui-tooltip-content");
        Assert.NotNull(sourceId);
        var source = rich.Find($"#{sourceId}");
        Assert.True(source.HasAttribute("hidden"));
        Assert.Equal("Escopos", source.QuerySelector(".sui-tooltip__title")!.TextContent);
        Assert.NotNull(source.QuerySelector(".sui-tooltip__body code"));
        Assert.Null(anchor.GetAttribute("data-sui-tooltip-persistent"));

        var persistent = context.Render<SUITooltip>(p => p
            .Add(c => c.Text, "Ajuda")
            .Add(c => c.PersistentDescription, true));
        Assert.Equal("true", persistent.Find(".sui-tooltip-anchor").GetAttribute("data-sui-tooltip-persistent"));
        var hiddenText = persistent.Find(".sui-sr-only");
        Assert.False(hiddenText.HasAttribute("hidden"));
        Assert.Equal("Ajuda", hiddenText.TextContent.Trim());

        var plain = context.Render<SUITooltip>(p => p.Add(c => c.Text, "Dica"));
        Assert.Null(plain.Find(".sui-tooltip-anchor").GetAttribute("data-sui-tooltip-content"));
        Assert.Empty(plain.FindAll(".sui-sr-only"));
    }

    [Fact]
    public void ExpansionPanel_KeepContentMounted_HidesInsteadOfRemoving()
    {
        using var context = Create();
        var cut = context.Render<SUIExpansionPanel>(p => p
            .Add(c => c.Title, "Avançado")
            .Add(c => c.KeepContentMounted, true)
            .Add(c => c.HeaderContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span class=\"badge\">3</span>")))
            .AddChildContent("<p>Conteúdo</p>"));

        var content = cut.Find(".sui-expansion__content");
        Assert.True(content.HasAttribute("hidden"));
        Assert.Equal(content.Id, cut.Find("button").GetAttribute("aria-controls"));
        Assert.NotNull(cut.Find("button .sui-expansion__extra .badge"));
        cut.Find("button").Click();
        Assert.False(cut.Find(".sui-expansion__content").HasAttribute("hidden"));
    }

    [Fact]
    public void SwitchButton_ExposesPressedStateAndOptionalAutoWidth()
    {
        using var context = Create();
        var cut = context.Render<SUISwitchButton>(p => p
            .Add(c => c.Checked, true)
            .Add(c => c.FullWidth, false)
            .AddChildContent("Ativo"));
        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-pressed"));
        Assert.DoesNotContain("sui-btn--full-width", button.ClassList);

        var plain = context.Render<SUISwitchButton>(p => p.AddChildContent("Ativo"));
        Assert.Equal("false", plain.Find("button").GetAttribute("aria-pressed"));
        Assert.Contains("sui-btn--full-width", plain.Find("button").ClassList);
    }

    [Fact]
    public void ChoiceCard_ActionMode_RendersButtonOrLinkInsteadOfRadio()
    {
        using var context = Create();
        string? picked = null;
        var button = context.Render<SUIChoiceCard<string>>(p => p
            .Add(c => c.Value, "admin")
            .Add(c => c.Title, "Administrador")
            .Add(c => c.OnClick, (string value) => picked = value));
        Assert.Empty(button.FindAll("input[type=radio]"));
        Assert.Contains("sui-choice-card--action", button.Find("button").ClassList);
        button.Find("button").Click();
        Assert.Equal("admin", picked);

        var link = context.Render<SUIChoiceCard<string>>(p => p
            .Add(c => c.Value, "ops")
            .Add(c => c.Title, "Operação")
            .Add(c => c.Href, "/perfil/ops"));
        Assert.Equal("/perfil/ops", link.Find("a.sui-choice-card").GetAttribute("href"));

        var radio = context.Render<SUIChoiceCard<string>>(p => p
            .Add(c => c.Value, "a")
            .Add(c => c.Title, "A"));
        Assert.NotNull(radio.Find("label input[type=radio]"));
    }
}
