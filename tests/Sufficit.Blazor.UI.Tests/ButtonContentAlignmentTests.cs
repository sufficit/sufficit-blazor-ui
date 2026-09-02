using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ButtonContentAlignmentTests
{
    [Fact]
    public void Icon_receives_the_optical_offset_without_moving_the_text_label()
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
        Assert.DoesNotMatch(
            @"\.sui-btn__label\s*\{[^}]*transform\s*:",
            stylesheet);
        Assert.Matches(
            @"\.sui-btn__icon\s*\{[^}]*transform\s*:\s*translateY\(1px\)",
            stylesheet);
    }
}
