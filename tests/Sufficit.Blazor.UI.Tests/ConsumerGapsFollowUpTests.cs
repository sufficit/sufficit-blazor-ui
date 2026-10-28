using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Second round of gaps found while migrating the identity management UI:
/// the loading button press marker, icon names on every icon parameter, the
/// empty state tone, switch alignment in a form grid and a CSP-safe select
/// menu width.
/// </summary>
public sealed class ConsumerGapsFollowUpTests
{
    private const string LoadingButtonModule = "./_content/Sufficit.Blazor.UI/Components/Actions/SUILoadingButton.razor.js";
    private const string SelectModule = "./_content/Sufficit.Blazor.UI/Components/Forms/SUISelect.razor.js?v=3";

    private static BunitJSModuleInterop SetupButtonModule(BunitContext context)
    {
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var module = context.JSInterop.SetupModule(LoadingButtonModule);
        module.Mode = JSRuntimeMode.Loose;
        return module;
    }

    [Fact]
    public void LoadingButton_AnsweredClickWithoutLoading_ReleasesThePressMarker()
    {
        using var context = new BunitContext();
        var module = SetupButtonModule(context);
        var clicks = 0;
        var cut = context.Render<SUILoadingButton>(p => p
            .Add(c => c.OnClick, (MouseEventArgs _) => clicks++)
            .AddChildContent("Salvar"));

        cut.Find("button").Click();

        Assert.Equal(1, clicks);
        cut.WaitForAssertion(() => Assert.Single(module.Invocations["release"]));
    }

    [Fact]
    public void LoadingButton_WhileLoading_LeavesTheMarkerToTheClassRewrite()
    {
        using var context = new BunitContext();
        var module = SetupButtonModule(context);
        var cut = context.Render<SUILoadingButton>(p => p
            .Add(c => c.IsLoading, true)
            .Add(c => c.OnClick, (MouseEventArgs _) => { })
            .AddChildContent("Salvar"));

        cut.Find("button").Click();

        Assert.Empty(module.Invocations["release"]);
    }

    [Fact]
    public void LoadingButton_SubmitWithoutHandler_ReleasesWhenTheFormAnswers()
    {
        using var context = new BunitContext();
        var module = SetupButtonModule(context);
        var cut = context.Render<SUILoadingButton>(p => p
            .Add(c => c.ButtonTypeValue, SUIButtonType.Submit)
            .AddChildContent("Enviar"));

        cut.Find("button").Click();
        Assert.Empty(module.Invocations["release"]);

        // The form's submit handler re-renders the parent: new parameters.
        cut.Render(p => p.Add(c => c.IsLoading, false));
        cut.WaitForAssertion(() => Assert.Single(module.Invocations["release"]));
    }

    [Fact]
    public void LoadingButton_WithoutInstantFeedback_NeverTouchesTheScript()
    {
        using var context = new BunitContext();
        var module = SetupButtonModule(context);
        var cut = context.Render<SUILoadingButton>(p => p
            .Add(c => c.InstantFeedback, false)
            .Add(c => c.OnClick, (MouseEventArgs _) => { })
            .AddChildContent("Quiet"));

        cut.Find("button").Click();

        Assert.Empty(module.Invocations["release"]);
    }

    [Fact]
    public void IconParameters_AcceptSUIIconNames()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var loading = context.Render<SUILoadingButton>(p => p
            .Add(c => c.StartIcon, "key")
            .Add(c => c.EndIcon, SUIIcons.Key)
            .AddChildContent("Gerar chave"));
        var icons = loading.FindAll(".sui-btn__icon svg");
        Assert.Equal(2, icons.Count);
        Assert.Equal("currentColor", icons[0].GetAttribute("stroke"));
        Assert.NotEmpty(icons[0].QuerySelectorAll("circle"));
        Assert.NotEmpty(icons[1].QuerySelectorAll("circle"));

        var empty = context.Render<SUITableEmpty>(p => p.Add(c => c.Icon, "users"));
        Assert.NotEmpty(empty.FindAll(".sui-table-empty__icon svg circle"));
    }

    [Fact]
    public void Icons_ExposeCommonNamesAsConstants()
    {
        foreach (var markup in new[]
        {
            SUIIcons.Key, SUIIcons.Lock, SUIIcons.Unlock, SUIIcons.User, SUIIcons.Users,
            SUIIcons.Check, SUIIcons.Info, SUIIcons.Download, SUIIcons.Upload,
            SUIIcons.Eye, SUIIcons.EyeOff, SUIIcons.Calendar, SUIIcons.Bell,
        })
        {
            using var context = new BunitContext();
            var icon = context.Render<SUIIcon>(p => p.Add(c => c.Name, markup));
            Assert.NotEmpty(icon.Find("svg").Children);
        }
    }

    [Theory]
    [InlineData(SUITone.Danger, "sui-empty--danger")]
    [InlineData(SUITone.Warning, "sui-empty--warning")]
    [InlineData(SUITone.Neutral, "sui-empty--neutral")]
    public void EmptyState_ToneValue_ColorsTheIcon(SUITone tone, string expected)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIEmptyState>(p => p
            .Add(c => c.Icon, "alert")
            .Add(c => c.Title, "Falhou")
            .Add(c => c.ToneValue, tone));

        Assert.Contains(expected, cut.Find(".sui-empty").ClassList);
    }

    [Fact]
    public void EmptyState_WithoutTone_KeepsTheMutedIcon()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIEmptyState>(p => p.Add(c => c.Title, "Nada aqui"));

        Assert.Equal("sui-empty", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void Switch_AlignWithFields_ReservesTheFieldLabelSlot()
    {
        using var context = new BunitContext();
        var aligned = context.Render<SUISwitch>(p => p
            .Add(c => c.Label, "Ativo")
            .Add(c => c.AlignWithFields, true));

        var wrapper = aligned.Find(".sui-switch-field");
        Assert.True(wrapper.HasAttribute("data-sui-align-field"));
        var slot = wrapper.QuerySelector(":scope > .sui-field__label[data-sui-field-label]");
        Assert.NotNull(slot);
        Assert.Equal("true", slot!.GetAttribute("aria-hidden"));
        Assert.NotNull(wrapper.QuerySelector(":scope > label.sui-switch"));

        var plain = context.Render<SUISwitch>(p => p.Add(c => c.Label, "Ativo"));
        Assert.Empty(plain.FindAll(".sui-switch-field"));
        Assert.Equal("LABEL", plain.Nodes.OfType<AngleSharp.Dom.IElement>().First().TagName);
    }

    [Fact]
    public void Select_MenuWidths_AreAppliedThroughTheModuleNotAStyleAttribute()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var module = context.JSInterop.SetupModule(SelectModule);
        module.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUISelect<string>>(p => p
            .Add(c => c.Label, "Escopo")
            .Add(c => c.MenuWidth, "20rem")
            .Add(c => c.MenuMaxWidth, "36rem; color: red")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SUISelectItem>(0);
                builder.AddAttribute(1, nameof(SUISelectItem.Value), "openid");
                builder.AddAttribute(2, nameof(SUISelectItem.ChildContent),
                    (RenderFragment)(content => content.AddContent(0, "openid")));
                builder.CloseComponent();
            }));

        Assert.False(cut.Find(".sui-select__menu").HasAttribute("style"));

        cut.Find("button.sui-select__trigger").Click();

        cut.WaitForAssertion(() => Assert.Single(module.Invocations["openSelectMenu"]));
        var arguments = module.Invocations["openSelectMenu"].Single().Arguments;
        Assert.Equal("20rem", arguments[2]);
        // A declaration separator could smuggle other properties: ignored.
        Assert.Null(arguments[3]);
        Assert.False(cut.Find(".sui-select__menu").HasAttribute("style"));
    }
}
