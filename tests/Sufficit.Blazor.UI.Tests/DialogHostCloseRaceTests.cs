using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Closing a dialog awaits a JS call. A dialog requested while that call is
/// in flight must survive it: the host used to clear its current dialog after
/// the await without checking, which erased the new dialog from the screen
/// and left its caller waiting on a Result that never completed.
/// </summary>
public sealed class DialogHostCloseRaceTests
{
    [Fact]
    public async Task A_dialog_shown_while_the_previous_one_closes_stays_open()
    {
        await using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var module = context.JSInterop.SetupModule(
            "./_content/Sufficit.Blazor.UI/Components/Overlays/SUIDialogHost.razor.js");
        module.Mode = JSRuntimeMode.Loose;
        var close = module.SetupVoid("closeDialog", _ => true);
        var service = new SUIDialogService();
        context.Services.AddSingleton<ISUIDialogService>(service);
        var cut = context.Render<SUIDialogHost>();

        var first = await service.ShowAsync<SUIConfirmDialog>("Primeiro");
        cut.WaitForAssertion(() =>
            Assert.Equal("Primeiro", cut.Find(".sui-dialog__title").TextContent));

        // The second request starts closing the first and waits on JS ...
        var second = await service.ShowAsync<SUIConfirmDialog>("Segundo");
        // ... and the first one's own completion starts another close.
        first.Complete(true);
        await cut.InvokeAsync(() => { });

        close.SetVoidResult();

        cut.WaitForAssertion(() =>
            Assert.Equal("Segundo", cut.Find(".sui-dialog__title").TextContent));
        Assert.False(second.Result.IsCompleted);
    }

    [Fact]
    public void The_close_button_name_can_follow_the_host_language()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var service = new SUIDialogService();
        context.Services.AddSingleton<ISUIDialogService>(service);
        var cut = context.Render<SUIDialogHost>(p => p.Add(h => h.CloseLabel, "Close"));

        _ = service.ShowAsync<SUIConfirmDialog>("Title");

        cut.WaitForAssertion(() =>
            Assert.Equal("Close", cut.Find(".sui-dialog__close").GetAttribute("aria-label")));
    }
}
