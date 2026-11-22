using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Xunit;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>Owner 22/11/2026: switching tabs updates the URL so a refresh or a shared link reopens the same tab.</summary>
public sealed class TabsQueryTests
{
    static IRenderedComponent<SUITabs> Render(BunitContext context) => context.Render<SUITabs>(parameters => parameters
        .Add(c => c.QueryKey, "tab")
        .AddChildContent(builder =>
        {
            foreach (var title in new[] { "Serviço", "Parâmetros (2)", "Pagamentos" })
            {
                builder.OpenComponent<SUITabPanel>(0);
                builder.AddAttribute(1, nameof(SUITabPanel.Text), title);
                builder.AddAttribute(2, nameof(SUITabPanel.ChildContent), (RenderFragment)(b => b.AddContent(0, title)));
                builder.CloseComponent();
            }
        }));

    [Fact]
    public void DeepLinkOpensTheTabAndSwitchingWritesTheUrl()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/pages/sales/services/item?id=1&tab=parametros");
        var cut = Render(context);
        cut.WaitForAssertion(() => Assert.Equal(1, cut.Instance.ActiveIndex));
        cut.FindAll("[role=tab]")[2].Click();
        Assert.EndsWith("?id=1&tab=pagamentos", navigation.Uri);
        navigation.NavigateTo("/pages/sales/services/item?id=1&tab=servico");
        cut.WaitForAssertion(() => Assert.Equal(0, cut.Instance.ActiveIndex));
    }

    [Theory]
    [InlineData("Parâmetros (2)", "parametros")]
    [InlineData("Competências (12)", "competencias")]
    [InlineData("Dados do serviço", "dados-do-servico")]
    public void TitleSlugDropsCountersAndAccents(string title, string slug)
        => Assert.Equal(slug, SUITabPanel.ToSlug(title));
}
