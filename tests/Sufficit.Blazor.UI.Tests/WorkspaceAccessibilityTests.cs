using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

public sealed class WorkspaceAccessibilityTests
{
    [Theory]
    [InlineData("#000000", "#ffffff", 21)]
    [InlineData("#ffffff", "#ffffff", 1)]
    [InlineData("#ff0000", "#ffffff", 3.99847677075)]
    public void Contrast_UsesLinearSrgbAndIsSymmetric(string first, string second, double expected)
    {
        Assert.True(SUIColorContrast.TryGetRatio(first, second, out var forward));
        Assert.True(SUIColorContrast.TryGetRatio(second, first, out var reverse));
        Assert.Equal(expected, forward, 8);
        Assert.Equal(forward, reverse);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("#fff")]
    [InlineData("var(--sui-surface)")]
    [InlineData("#ffffff00")]
    [InlineData("#12zz45")]
    public void Contrast_DoesNotPretendToResolveUnsupportedColors(string? color)
        => Assert.False(SUIColorContrast.TryGetRatio(color, "#ffffff", out _));

    [Fact]
    public void VerticalTabs_MoveWithUpDownAndKeepHorizontalKeysForThePage()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<SUITabs>(parameters => parameters
            .Add(c => c.Vertical, true)
            .AddChildContent(builder =>
            {
                foreach (var title in new[] { "Resumo", "Configurações", "Atividade" })
                {
                    builder.OpenComponent<SUITabPanel>(0);
                    builder.AddAttribute(1, nameof(SUITabPanel.Text), title);
                    builder.AddAttribute(2, nameof(SUITabPanel.ChildContent), (RenderFragment)(b => b.AddContent(0, title)));
                    builder.CloseComponent();
                }
            }));
        cut.WaitForAssertion(() => Assert.Equal(3, cut.FindAll("[role=tab]").Count));
        Assert.Equal("vertical", cut.Find("[role=tablist]").GetAttribute("aria-orientation"));
        cut.FindAll("[role=tab]")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal(0, cut.Instance.ActiveIndex);
        cut.FindAll("[role=tab]")[0].KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        cut.WaitForAssertion(() => Assert.Equal(2, cut.Instance.ActiveIndex));
        cut.FindAll("[role=tab]")[2].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        cut.WaitForAssertion(() => Assert.Equal(0, cut.Instance.ActiveIndex));
        Assert.Equal("0", cut.FindAll("[role=tab]")[0].GetAttribute("tabindex"));
        Assert.Contains("Resumo", cut.Find("[role=tabpanel]").TextContent);
    }
}
