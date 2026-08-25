using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class AccessibilityContractTests
{
    [Fact]
    public void TextField_AssociatesLabelAndHelperWithInput()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Label, "Nome")
            .Add(component => component.HelperText, "Como será exibido."));

        AssertFieldRelationships(cut, "input");
    }

    [Fact]
    public void TextField_MultilinePreservesAccessibleRelationshipsAndRows()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Label, "Texto ou SSML")
            .Add(component => component.HelperText, "Aceita um documento speak completo.")
            .Add(component => component.Multiline, true)
            .Add(component => component.Rows, 6));

        AssertFieldRelationships(cut, "textarea");
        Assert.Equal("6", cut.Find("textarea").GetAttribute("rows"));
    }

    [Fact]
    public void NumericField_AssociatesLabelAndHelperWithInput()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUINumericField<int>>(parameters => parameters
            .Add(component => component.Label, "Tentativas")
            .Add(component => component.HelperText, "Entre 1 e 10."));

        AssertFieldRelationships(cut, "input");
    }

    [Fact]
    public void TextField_ExposesInvalidErrorRelationship()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(parameters => parameters
            .Add(component => component.Label, "Nome")
            .Add(component => component.Invalid, true)
            .Add(component => component.ErrorText, "Informe um nome válido."));

        AssertInvalidRelationship(cut, "input");
    }

    [Fact]
    public void NumericField_ExposesInvalidErrorRelationship()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUINumericField<int>>(parameters => parameters
            .Add(component => component.Label, "Tentativas")
            .Add(component => component.Invalid, true)
            .Add(component => component.ErrorText, "Valor fora do intervalo."));

        AssertInvalidRelationship(cut, "input");
    }

    [Fact]
    public void Select_AssociatesLabelHelperAndActiveOption()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUISelect<string>>(parameters => parameters
            .Add(component => component.Label, "Região")
            .Add(component => component.HelperText, "Escolha uma região.")
            .AddChildContent<SUISelectItem>(item => item
                .Add(option => option.Value, "sudeste")
                .AddChildContent("Sudeste")));

        AssertFieldRelationships(cut, "button.sui-select__trigger");
        var trigger = cut.Find("button.sui-select__trigger");
        Assert.Null(trigger.GetAttribute("aria-activedescendant"));

        trigger.Click();
        cut.WaitForAssertion(() =>
        {
            var activeId = trigger.GetAttribute("aria-activedescendant");
            Assert.False(string.IsNullOrWhiteSpace(activeId));
            Assert.Equal(activeId, cut.Find("[role=option]").Id);
        });
    }

    [Fact]
    public void Autocomplete_UsesComboboxAndListboxContract()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIAutocomplete<string>>(parameters => parameters
            .Add(component => component.Label, "Cidade")
            .Add(component => component.HelperText, "Digite para pesquisar.")
            .Add(component => component.SearchFunc,
                _ => Task.FromResult<IEnumerable<string>>(["São Paulo"])));

        AssertFieldRelationships(cut, "input");
        var input = cut.Find("input");
        Assert.Equal("combobox", input.GetAttribute("role"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
        Assert.Equal("list", input.GetAttribute("aria-autocomplete"));
        Assert.NotNull(input.GetAttribute("aria-controls"));
        Assert.NotNull(input.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Checkbox_UsesNativeInputAndAccessibleName()
    {
        using var context = new BunitContext();
        var changed = false;
        var cut = context.Render<SUICheckbox>(parameters => parameters
            .Add(component => component.Value, true)
            .Add(component => component.AriaLabel, "Selecionar provider")
            .Add(component => component.ValueChanged, value => changed = value));

        var input = cut.Find("input[type=checkbox]");
        Assert.Equal("Selecionar provider", input.GetAttribute("aria-label"));
        Assert.Contains("sui-checkbox--checked", cut.Find("label").ClassList);

        input.Change(false);
        Assert.False(changed);
    }

    [Fact]
    public void Select_ExposesInvalidErrorRelationship()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISelect<string>>(parameters => parameters
            .Add(component => component.Label, "Região")
            .Add(component => component.Invalid, true)
            .Add(component => component.ErrorText, "Selecione uma região."));

        AssertInvalidRelationship(cut, "button.sui-select__trigger");
    }

    [Fact]
    public void Autocomplete_ExposesInvalidErrorRelationship()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIAutocomplete<string>>(parameters => parameters
            .Add(component => component.Label, "Cidade")
            .Add(component => component.Invalid, true)
            .Add(component => component.ErrorText, "Escolha uma cidade válida."));

        AssertInvalidRelationship(cut, "input");
    }

    [Fact]
    public void Tabs_AssociateTabsAndPanelAndUseRovingTabindex()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUITabs>(parameters => parameters
            .Add(component => component.Id, "catalog-tabs")
            .Add(component => component.AriaLabel, "Visões")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SUITabPanel>(0);
                builder.AddAttribute(1, nameof(SUITabPanel.Text), "Resumo");
                builder.AddAttribute(2, nameof(SUITabPanel.ChildContent),
                    (RenderFragment)(content => content.AddContent(0, "Conteúdo resumo")));
                builder.CloseComponent();
                builder.OpenComponent<SUITabPanel>(3);
                builder.AddAttribute(4, nameof(SUITabPanel.Text), "Detalhes");
                builder.AddAttribute(5, nameof(SUITabPanel.ChildContent),
                    (RenderFragment)(content => content.AddContent(0, "Conteúdo detalhes")));
                builder.CloseComponent();
            }));

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[role=tab]").Count));
        var tabList = cut.Find("[role=tablist]");
        var tabs = cut.FindAll("[role=tab]");
        var panel = cut.Find("[role=tabpanel]");

        Assert.Equal("Visões", tabList.GetAttribute("aria-label"));
        Assert.Equal("horizontal", tabList.GetAttribute("aria-orientation"));
        Assert.Equal("0", tabs[0].GetAttribute("tabindex"));
        Assert.Equal("-1", tabs[1].GetAttribute("tabindex"));
        Assert.Equal(panel.Id, tabs[0].GetAttribute("aria-controls"));
        Assert.Equal(tabs[0].Id, panel.GetAttribute("aria-labelledby"));

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        cut.WaitForAssertion(() =>
        {
            var updatedTabs = cut.FindAll("[role=tab]");
            Assert.Equal("-1", updatedTabs[0].GetAttribute("tabindex"));
            Assert.Equal("0", updatedTabs[1].GetAttribute("tabindex"));
            Assert.Contains("Conteúdo detalhes", cut.Find("[role=tabpanel]").TextContent);
        });
    }

    [Fact]
    public void Tabs_HonorInitialActiveIndexParameter()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUITabs>(parameters => parameters
            .Add(component => component.ActiveIndex, 1)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SUITabPanel>(0);
                builder.AddAttribute(1, nameof(SUITabPanel.Text), "Primeiro");
                builder.AddAttribute(2, nameof(SUITabPanel.ChildContent),
                    (RenderFragment)(content => content.AddContent(0, "Conteúdo inicial")));
                builder.CloseComponent();
                builder.OpenComponent<SUITabPanel>(3);
                builder.AddAttribute(4, nameof(SUITabPanel.Text), "Segundo");
                builder.AddAttribute(5, nameof(SUITabPanel.ChildContent),
                    (RenderFragment)(content => content.AddContent(0, "Conteúdo selecionado")));
                builder.CloseComponent();
            }));

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[role=tab]").Count));
        var tabs = cut.FindAll("[role=tab]");
        Assert.Equal("false", tabs[0].GetAttribute("aria-selected"));
        Assert.Equal("true", tabs[1].GetAttribute("aria-selected"));
        Assert.Contains("Conteúdo selecionado", cut.Find("[role=tabpanel]").TextContent);
    }

    [Fact]
    public void Table_UsesScopedHeadersFullEmptyColspanAndKeyboardRows()
    {
        using var context = new BunitContext();
        var header = context.Render<SUITh>(parameters => parameters.AddChildContent("Serviço"));
        Assert.Equal("col", header.Find("th").GetAttribute("scope"));

        var empty = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, Array.Empty<string>())
            .Add(component => component.ColumnCount, 3)
            .Add(component => component.NoRecordsContent,
                (RenderFragment)(builder => builder.AddContent(0, "Sem registros"))));
        Assert.Equal("3", empty.Find("tbody td").GetAttribute("colspan"));

        var clicks = 0;
        var interactive = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["API"])
            .Add(component => component.RowTemplate,
                item => builder =>
                {
                    builder.OpenElement(0, "td");
                    builder.AddContent(1, item);
                    builder.CloseElement();
                })
            .Add(component => component.OnRowClick, _ => clicks++));
        var row = interactive.Find("tbody tr");

        Assert.Equal("button", row.GetAttribute("role"));
        Assert.Equal("0", row.GetAttribute("tabindex"));
        row.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        row.KeyDown(new KeyboardEventArgs { Key = " " });
        Assert.Equal(2, clicks);
    }

    [Fact]
    public void Table_ExposesLoadingAndCalculatedRowPresentation()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITable<string>>(parameters => parameters
            .Add(component => component.Items, ["Crédito"])
            .Add(component => component.Loading, true)
            .Add(component => component.RowClassFunc, (_, _) => "row-positive")
            .Add(component => component.RowStyleFunc, (_, _) => "color: green;")
            .Add(component => component.RowTemplate,
                item => builder =>
                {
                    builder.OpenElement(0, "td");
                    builder.AddContent(1, item);
                    builder.CloseElement();
                }));

        Assert.Equal("true", cut.Find(".sui-table-wrapper").GetAttribute("aria-busy"));
        Assert.NotNull(cut.Find("[role=progressbar]"));
        var row = cut.Find("tbody tr");
        Assert.Contains("row-positive", row.ClassList);
        Assert.Contains("color: green", row.GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void Pagination_AnnouncesRangeAndMovesByKeyboardButton()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var selectedPage = -1;
        var cut = context.Render<SUIPagination>(parameters => parameters
            .Add(component => component.TotalItems, 60)
            .Add(component => component.PageSize, 25)
            .Add(component => component.PageIndex, 1)
            .Add(component => component.PageIndexChanged, page => selectedPage = page));

        Assert.Equal("Paginação", cut.Find("nav").GetAttribute("aria-label"));
        Assert.Contains("26–50 de 60 itens", cut.Markup, StringComparison.Ordinal);
        cut.Find("button[aria-label='Próxima página']").Click();
        Assert.Equal(2, selectedPage);
    }

    [Fact]
    public void Progress_ClampsValueAndUsesTransformScale()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIProgressLinear>(parameters => parameters
            .Add(component => component.Value, 150));

        Assert.Equal("100", cut.Find("[role=progressbar]").GetAttribute("aria-valuenow"));
        var barStyle = cut.Find(".sui-progress-linear__bar").GetAttribute("style") ?? string.Empty;
        Assert.Contains("--sui-progress-scale:1", barStyle, StringComparison.Ordinal);
        Assert.DoesNotContain("width", barStyle, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Text_UsesSemanticHeadingAndAllowsExplicitTagOverride()
    {
        using var context = new BunitContext();
        var heading = context.Render<SUIText>(parameters => parameters
            .Add(component => component.Typo, SUITypo.h2)
            .AddUnmatched("lang", "pt-BR")
            .AddChildContent("Operação"));
        Assert.Equal("H2", heading.Find("h2").TagName);
        Assert.Equal("pt-BR", heading.Find("h2").GetAttribute("lang"));

        heading.Render(parameters => parameters
            .Add(component => component.Typo, SUITypo.h2)
            .Add(component => component.Tag, SUITextTag.Span)
            .AddChildContent("Operação"));
        Assert.Equal("SPAN", heading.Find("span").TagName);
    }

    [Fact]
    public void Text_DoesNotEmitTrailingClassWhitespaceWithoutAlignment()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIText>(parameters => parameters
            .Add(component => component.Typo, SUITypo.body2)
            .AddChildContent("Conteúdo"));

        var className = cut.Find("div").GetAttribute("class");

        Assert.NotNull(className);
        Assert.False(className!.EndsWith(" ", StringComparison.Ordinal));
    }

    private static void AssertFieldRelationships<TComponent>(
        IRenderedComponent<TComponent> cut,
        string controlSelector)
        where TComponent : IComponent
    {
        var control = cut.Find(controlSelector);
        var label = cut.Find("label");
        var helper = cut.Find(".sui-field__helper");
        var id = control.GetAttribute("id");
        var helperId = helper.GetAttribute("id");

        Assert.False(string.IsNullOrWhiteSpace(id));
        Assert.Equal(id, label.GetAttribute("for"));
        Assert.False(string.IsNullOrWhiteSpace(helperId));
        Assert.Contains(helperId!, control.GetAttribute("aria-describedby") ?? string.Empty, StringComparison.Ordinal);
    }

    private static void AssertInvalidRelationship<TComponent>(
        IRenderedComponent<TComponent> cut,
        string controlSelector)
        where TComponent : IComponent
    {
        var control = cut.Find(controlSelector);
        var error = cut.Find(".sui-field__error");
        var errorId = error.GetAttribute("id");

        Assert.Equal("true", control.GetAttribute("aria-invalid"));
        Assert.False(string.IsNullOrWhiteSpace(errorId));
        Assert.Equal(errorId, control.GetAttribute("aria-errormessage"));
        Assert.Contains(errorId!, control.GetAttribute("aria-describedby") ?? string.Empty, StringComparison.Ordinal);
    }
}
