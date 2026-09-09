using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ButtonContentAlignmentTests
{
    [Fact]
    public void Large_target_keeps_a_compact_default_icon_but_honors_explicit_icon_size()
    {
        using var context = new BunitContext();
        var button = context.Render<SUIButton>(p => p
            .Add(c => c.SizeValue, SUISize.Large)
            .Add(c => c.StartIcon, SUIIcons.Add).AddChildContent("Criar item"));
        Assert.Contains("sui-icon--small", button.Find(".sui-btn__icon").ClassName);
        button.Render(p => p.Add(c => c.IconSizeValue, SUISize.Large));
        Assert.Contains("sui-icon--large", button.Find(".sui-btn__icon").ClassName);
    }

    [Fact]
    public void Soft_buttons_preserve_labels_icons_and_existing_variant_values()
    {
        using var context = new BunitContext();
        var button = context.Render<SUIButton>(p => p
            .Add(c => c.VariantValue, SUIVariant.Soft)
            .Add(c => c.StartIcon, SUIIcons.Save)
            .Add(c => c.EndIcon, SUIIcons.ChevronRight)
            .AddChildContent("Salvar tudo"));
        Assert.Contains("sui-btn--soft", button.Find("button").ClassName);
        Assert.Equal("Salvar tudo", button.Find(".sui-btn__label").TextContent.Trim());
        Assert.Equal(2, button.FindAll(".sui-btn__icon").Count);
        var loading = context.Render<SUILoadingButton>(p => p
            .Add(c => c.VariantValue, SUIVariant.Soft)
            .Add(c => c.IsLoading, true).AddChildContent("Salvar"));
        Assert.Contains("sui-btn--soft", loading.Find("button").ClassName);
        Assert.True(loading.Find("button").HasAttribute("disabled"));
        var icon = context.Render<SUIIconButton>(p => p
            .Add(c => c.VariantValue, SUIVariant.Soft)
            .Add(c => c.Icon, SUIIcons.Save).Add(c => c.Title, "Salvar"));
        Assert.Contains("sui-btn--soft", icon.Find("button").ClassName);
        Assert.Equal(0, (int)SUIVariant.Text);
        Assert.Equal(1, (int)SUIVariant.Outlined);
        Assert.Equal(2, (int)SUIVariant.Filled);
    }
}
