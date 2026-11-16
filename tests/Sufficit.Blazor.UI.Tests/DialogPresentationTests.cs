using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;
namespace Sufficit.Blazor.UI.Tests;

public sealed class DialogPresentationTests
{
    [Fact]
    public async Task ExplicitCancellationPolicyKeepsTheEditorOpenUntilItCompletes()
    {
        using var context = new BunitContext(); context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIDialogHost.razor.js");
        var service = new SUIDialogService(); context.Services.AddSingleton<ISUIDialogService>(service);
        var host = context.Render<SUIDialogHost>();
        var reference = await service.ShowAsync<SUIConfirmDialog>("Revisar", null, new SUIDialogOptions {
            MaxWidth = 960, CloseOnBackdropClick = false, CloseOnEscape = false, ShowCloseButton = false });
        host.WaitForElement(".sui-dialog");
        Assert.Empty(host.FindAll(".sui-dialog__close"));
        Assert.Contains("960px", host.Find(".sui-dialog").GetAttribute("data-sui-style"));
        await host.Find(".sui-dialog-overlay").ClickAsync(new());
        await host.InvokeAsync(() => host.Instance.DismissFromKeyboardAsync());
        Assert.False(reference.Result.IsCompleted);
        await host.InvokeAsync(() => reference.Complete(true));
        Assert.Equal(true, await reference.Result);
        host.WaitForAssertion(() => Assert.Empty(host.FindAll(".sui-dialog")));
    }

    [Fact]
    public async Task DefaultDialogStillAllowsKeyboardDismissalAndClampsOversizedPanels()
    {
        using var context = new BunitContext(); context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Overlays/SUIDialogHost.razor.js");
        var service = new SUIDialogService(); context.Services.AddSingleton<ISUIDialogService>(service);
        var host = context.Render<SUIDialogHost>();
        var reference = await service.ShowAsync<SUIConfirmDialog>("Consultar", null, new SUIDialogOptions { MaxWidth = int.MaxValue });
        host.WaitForElement(".sui-dialog__close");
        Assert.Contains("1600px", host.Find(".sui-dialog").GetAttribute("data-sui-style"));
        await host.InvokeAsync(() => host.Instance.DismissFromKeyboardAsync());
        Assert.Null(await reference.Result);
    }
    [Fact]
    public async Task OlderProvidersCannotSilentlyIgnoreExplicitDismissalPolicies()
    {
        ISUIDialogService provider = new BasicProvider();
        await Assert.ThrowsAsync<NotSupportedException>(() => provider.ShowAsync<SUIConfirmDialog>("Editar", null,
            new SUIDialogOptions { CloseOnBackdropClick = false }));
    }

    private sealed class BasicProvider : ISUIDialogService
    {
        public event Action<SUIDialogRequest>? OnShow { add { } remove { } }
        public Task<SUIDialogReference> ShowAsync<T>(string title, IDictionary<string, object?>? parameters = null)
            where T : Microsoft.AspNetCore.Components.ComponentBase => Task.FromResult(new SUIDialogReference());
        public Task<bool> ConfirmAsync(string title, string message) => Task.FromResult(false);
    }

}
