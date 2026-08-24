using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ButtonContentAlignmentTests
{
    [Fact]
    public void Label_groups_icon_and_text_for_optical_alignment()
    {
        using var context = new BunitContext();

        var rendered = context.Render<SUIButton>(parameters => parameters
            .Add(component => component.StartIcon, SUIIcons.Save)
            .AddChildContent("Salvar tudo"));

        Assert.Equal("Salvar tudo", rendered.Find(".sui-btn__label").TextContent.Trim());
        Assert.NotNull(rendered.Find(".sui-btn__icon"));

        var stylesheet = File.ReadAllText(Path.Combine(
            RepositoryLayout.Styles,
            "sui-foundations.css"));
        Assert.Contains("transform: translateY(1px);", stylesheet, StringComparison.Ordinal);
    }
}
