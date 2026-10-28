using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Field gaps found while migrating the identity management UI: checkbox
/// description, warning message, monospace text, markup label/helper slots,
/// optional marker, numeric unit, suggestions and unequal form-grid columns.
/// Each test also pins that the default rendering did not change.
/// </summary>
public sealed class ConsumerGapsFieldTests
{
    private static BunitContext Create()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    [Fact]
    public void Checkbox_HelperText_IsLinkedByDescribedByAndUsesFieldHelperStyling()
    {
        using var context = Create();
        var cut = context.Render<SUICheckbox>(p => p
            .Add(c => c.Id, "remember")
            .Add(c => c.Label, "Lembrar")
            .Add(c => c.HelperText, "Mantém a sessão por 30 dias."));

        var helper = cut.Find("#remember-helper");
        Assert.Contains("sui-field__helper", helper.ClassList);
        Assert.Equal("Mantém a sessão por 30 dias.", helper.TextContent);
        Assert.Equal("remember-helper", cut.Find("input").GetAttribute("aria-describedby"));
        Assert.Contains("sui-checkbox--described", cut.Find("label").ClassList);
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void Checkbox_ChildContent_ReplacesLabelAndDefaultsStayUnchanged()
    {
        using var context = Create();
        var rich = context.Render<SUICheckbox>(p => p
            .Add(c => c.Label, "ignored")
            .AddChildContent("Aceito os <a href=\"/termos\">termos</a>"));
        Assert.Equal("Aceito os termos", rich.Find(".sui-checkbox__label").TextContent);
        Assert.NotNull(rich.Find(".sui-checkbox__label a"));

        var plain = context.Render<SUICheckbox>(p => p.Add(c => c.Label, "Ativo"));
        Assert.Null(plain.Find("input").GetAttribute("aria-describedby"));
        Assert.Empty(plain.FindAll(".sui-checkbox__text"));
        Assert.DoesNotContain("sui-checkbox--described", plain.Find("label").ClassList);
    }

    [Fact]
    public void TextField_WarningText_IsAnnouncedButNotInvalid()
    {
        using var context = Create();
        var cut = context.Render<SUITextField<string>>(p => p
            .Add(c => c.Id, "uri")
            .Add(c => c.HelperText, "Ajuda")
            .Add(c => c.WarningText, "URI sem HTTPS."));

        var input = cut.Find("input");
        Assert.Equal("uri-helper uri-warning", input.GetAttribute("aria-describedby"));
        Assert.Null(input.GetAttribute("aria-invalid"));
        var warning = cut.Find("#uri-warning");
        Assert.Contains("sui-field__warning", warning.ClassList);
        Assert.DoesNotContain("sui-field__error", warning.ClassList);
        Assert.Equal("URI sem HTTPS.", warning.TextContent);
    }

    [Fact]
    public void NumericAndSelect_WarningText_AreAnnouncedButNotInvalid()
    {
        using var context = Create();
        var numeric = context.Render<SUINumericField<int>>(p => p
            .Add(c => c.Id, "ttl")
            .Add(c => c.WarningText, "Valor alto."));
        Assert.Equal("ttl-warning", numeric.Find("input").GetAttribute("aria-describedby"));
        Assert.Null(numeric.Find("input").GetAttribute("aria-invalid"));

        var select = context.Render<SUISelect<string>>(p => p
            .Add(c => c.Id, "mode")
            .Add(c => c.WarningText, "Modo legado."));
        Assert.Equal("mode-warning", select.Find("button.sui-select__trigger").GetAttribute("aria-describedby"));
        Assert.Equal("false", select.Find("button.sui-select__trigger").GetAttribute("aria-invalid"));
        Assert.Equal("Modo legado.", select.Find("#mode-warning").TextContent);
    }

    [Fact]
    public void TextField_Monospace_UsesMonoClassAndDisablesSpellcheck()
    {
        using var context = Create();
        var single = context.Render<SUITextField<string>>(p => p.Add(c => c.Monospace, true));
        Assert.Contains("sui-field__input--mono", single.Find("input").ClassList);
        Assert.Equal("false", single.Find("input").GetAttribute("spellcheck"));

        var json = context.Render<SUITextField<string>>(p => p
            .Add(c => c.Monospace, true)
            .Add(c => c.Multiline, true));
        Assert.Contains("sui-field__input--mono", json.Find("textarea").ClassList);

        var plain = context.Render<SUITextField<string>>();
        Assert.Equal("sui-field__input", plain.Find("input").GetAttribute("class"));
        Assert.Null(plain.Find("input").GetAttribute("spellcheck"));
    }

    [Fact]
    public void TextField_LabelAndHelperContent_AcceptMarkup()
    {
        using var context = Create();
        var cut = context.Render<SUITextField<string>>(p => p
            .Add(c => c.Id, "cid")
            .Add(c => c.Label, "ignored")
            .Add(c => c.LabelContent, (RenderFragment)(b => b.AddMarkupContent(0, "Client <code>id</code>")))
            .Add(c => c.HelperContent, (RenderFragment)(b => b.AddMarkupContent(0, "Use <code>kebab-case</code>."))));

        Assert.Equal("Client id", cut.Find("label").TextContent);
        Assert.NotNull(cut.Find("label code"));
        Assert.NotNull(cut.Find("#cid-helper code"));
        Assert.Equal("cid-helper", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Select_LabelContent_StillNamesTheTrigger()
    {
        using var context = Create();
        var cut = context.Render<SUISelect<string>>(p => p
            .Add(c => c.Id, "grant")
            .Add(c => c.LabelContent, (RenderFragment)(b => b.AddMarkupContent(0, "Grant <code>type</code>"))));

        Assert.Equal("grant-label", cut.Find("button.sui-select__trigger").GetAttribute("aria-labelledby"));
        Assert.NotNull(cut.Find("#grant-label code"));
    }

    [Fact]
    public void Optional_AppendsLocalizableMarkerInsideTheLabel()
    {
        using var context = Create();
        var text = context.Render<SUITextField<string>>(p => p
            .Add(c => c.Label, "Descrição")
            .Add(c => c.Optional, true));
        Assert.Equal("(opcional)", text.Find("label .sui-field__optional").TextContent);
        Assert.Equal("Descrição (opcional)", text.Find("label").TextContent);

        var numeric = context.Render<SUINumericField<int>>(p => p
            .Add(c => c.Label, "Limit")
            .Add(c => c.Optional, true)
            .Add(c => c.OptionalText, "(optional)"));
        Assert.Equal("(optional)", numeric.Find("label .sui-field__optional").TextContent);

        var date = context.Render<SUIDateField>(p => p
            .Add(c => c.Label, "Expira em")
            .Add(c => c.Optional, true));
        Assert.NotNull(date.Find("label .sui-field__optional"));

        var plain = context.Render<SUITextField<string>>(p => p.Add(c => c.Label, "Nome"));
        Assert.Equal("Nome", plain.Find("label").InnerHtml);
    }

    [Fact]
    public void Numeric_Suffix_RendersInsideTheControlAndDescribesTheInput()
    {
        using var context = Create();
        var cut = context.Render<SUINumericField<int>>(p => p
            .Add(c => c.Id, "lifetime")
            .Add(c => c.Suffix, "min")
            .Add(c => c.HelperText, "Validade"));

        Assert.Equal("min", cut.Find(".sui-numeric__control > #lifetime-suffix").TextContent);
        Assert.Contains("sui-numeric__control--suffix", cut.Find(".sui-numeric__control").ClassList);
        Assert.Equal("lifetime-suffix lifetime-helper", cut.Find("input").GetAttribute("aria-describedby"));

        var plain = context.Render<SUINumericField<int>>();
        Assert.Equal("sui-numeric__control ", plain.Find(".sui-numeric__control").GetAttribute("class"));
        Assert.Empty(plain.FindAll(".sui-numeric__suffix"));
    }

    [Fact]
    public void TextField_Suggestions_RenderANativeDatalist()
    {
        using var context = Create();
        var cut = context.Render<SUITextField<string>>(p => p
            .Add(c => c.Id, "scope")
            .Add(c => c.Suggestions, new[] { "openid", "profile" }));

        Assert.Equal("scope-suggestions", cut.Find("input").GetAttribute("list"));
        var options = cut.FindAll("datalist#scope-suggestions option");
        Assert.Equal(new[] { "openid", "profile" }, options.Select(o => o.GetAttribute("value")));

        var plain = context.Render<SUITextField<string>>();
        Assert.Null(plain.Find("input").GetAttribute("list"));
        Assert.Empty(plain.FindAll("datalist"));
    }

    [Fact]
    public void FormGrid_ColumnTemplate_SetsUnequalColumnsAndKeepsDefaults()
    {
        using var context = Create();
        var cut = context.Render<SUIFormGrid>(p => p.Add(c => c.ColumnTemplate, "2fr 1fr"));
        var root = cut.Find(".sui-form-grid");
        Assert.Contains("sui-form-grid--template", root.ClassList);
        Assert.Contains("sui-form-grid--stack-mobile", root.ClassList);
        Assert.Equal("--sui-form-grid-template:2fr 1fr;", root.GetAttribute("style"));

        var injected = context.Render<SUIFormGrid>(p => p.Add(c => c.ColumnTemplate, "1fr; color: red"));
        Assert.Null(injected.Find(".sui-form-grid").GetAttribute("style"));

        var plain = context.Render<SUIFormGrid>();
        Assert.DoesNotContain("sui-form-grid--template", plain.Find(".sui-form-grid").ClassList);
        Assert.Null(plain.Find(".sui-form-grid").GetAttribute("style"));
    }
}
