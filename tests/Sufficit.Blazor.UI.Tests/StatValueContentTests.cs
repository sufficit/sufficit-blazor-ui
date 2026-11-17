using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class StatValueContentTests : BunitContext
{
    [Fact]
    public void RichValueOverridesTextAndRemainsHiddenUntilLoadingCompletes()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = Render<SUIStat>(p => p.Add(c => c.Value, "fallback").Add(c => c.Label, "Availability")
            .Add(c => c.Unit, "%").Add(c => c.Loading, true)
            .Add(c => c.ValueContent, b => b.AddMarkupContent(0, "<strong>Ready</strong>")));
        Assert.Empty(cut.FindAll("strong"));
        cut.Render(p => p.Add(c => c.Loading, false));
        Assert.Equal("Ready", cut.Find(".sui-stat__value strong").TextContent);
        Assert.DoesNotContain("fallback", cut.Find(".sui-stat__value").TextContent);
        Assert.Equal("%", cut.Find(".sui-stat__unit").TextContent);
        Assert.Equal("Availability", cut.Find(".sui-stat__label").TextContent);
        cut.Render(p => p.Add(c => c.ValueContent, (Microsoft.AspNetCore.Components.RenderFragment?)null));
        Assert.Contains("fallback", cut.Find(".sui-stat__value").TextContent);
    }
}
