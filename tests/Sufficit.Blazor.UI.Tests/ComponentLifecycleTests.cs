using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Tests;

public sealed class ComponentLifecycleTests
{
    [Fact]
    public async Task Autocomplete_DisposeCancelsPendingDebounce()
    {
        var context = new BunitContext();
        var cut = context.Render<SUIAutocomplete<string>>(parameters => parameters
            .Add(component => component.DebounceInterval, 5_000)
            .Add(component => component.SearchFunc,
                _ => Task.FromResult<IEnumerable<string>>(["São Paulo"])));

        var inputTask = cut.Find("input").InputAsync("s");
        cut.WaitForAssertion(() =>
            Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded")));

        await context.DisposeAsync();

        await inputTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Autocomplete_ClearButtonResetsControlledValue()
    {
        using var context = new BunitContext();
        string? changedValue = "Destino atual";
        var cut = context.Render<SUIAutocomplete<string>>(parameters => parameters
            .Add(component => component.Value, "Destino atual")
            .Add(component => component.Clearable, true)
            .Add(component => component.ValueChanged, value => changedValue = value));

        cut.Find("button[aria-label='Limpar seleção']").Click();

        Assert.Null(changedValue);
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void CardHeader_SupportsLegacyNamedSlotsDuringSuiMigration()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUICardHeader>(parameters => parameters
            .Add(component => component.CardHeaderAvatar,
                builder => builder.AddMarkupContent(0, "<span data-slot='avatar'>A</span>"))
            .Add(component => component.CardHeaderContent,
                builder => builder.AddMarkupContent(0, "<span data-slot='content'>C</span>"))
            .Add(component => component.CardHeaderActions,
                builder => builder.AddMarkupContent(0, "<span data-slot='actions'>X</span>")));

        Assert.NotNull(cut.Find("[data-slot='avatar']"));
        Assert.NotNull(cut.Find("[data-slot='content']"));
        Assert.NotNull(cut.Find("[data-slot='actions']"));
    }

    [Fact]
    public async Task DialogHost_CompletesReplacedBackdropAndDisposedRequests()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new SUIDialogService();
        context.Services.AddSingleton<ISUIDialogService>(service);
        var cut = context.Render<SUIDialogHost>();

        var first = await service.ShowAsync<SUIConfirmDialog>("Primeiro");
        cut.WaitForElement("[role=dialog]");
        var second = await service.ShowAsync<SUIConfirmDialog>("Segundo");

        await first.Result.WaitAsync(TimeSpan.FromSeconds(1));
        cut.WaitForAssertion(() =>
            Assert.Equal("Segundo", cut.Find(".sui-dialog__title").TextContent));

        cut.Find(".sui-dialog-overlay").Click();
        await second.Result.WaitAsync(TimeSpan.FromSeconds(1));
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("[role=dialog]")));

        var third = await service.ShowAsync<SUIConfirmDialog>("Terceiro");
        cut.WaitForElement("[role=dialog]");
        await context.DisposeAsync();

        await third.Result.WaitAsync(TimeSpan.FromSeconds(1));
    }
}
