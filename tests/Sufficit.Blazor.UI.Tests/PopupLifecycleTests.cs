using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class PopupLifecycleTests
{
    [Fact]
    public async Task LateImportIsReleasedWithoutConnectingADisposedPopup()
    {
        await using var context = new BunitContext();
        var runtime = new Runtime(); context.Services.AddSingleton<IJSRuntime>(runtime);
        var popup = context.Render<SUIPopup>();
        await popup.InvokeAsync(async () => await popup.Instance.DisposeAsync());
        var module = new Module(); runtime.Import.SetResult(module);
        popup.WaitForAssertion(() => Assert.Equal(1, module.Disposals));
        Assert.Empty(module.Calls);
    }

    [Fact]
    public async Task PendingSyncCompletesBeforeDisposalDisconnectsListeners()
    {
        await using var context = new BunitContext();
        var module = new Module { Pending = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var runtime = new Runtime(); runtime.Import.SetResult(module);
        context.Services.AddSingleton<IJSRuntime>(runtime);
        var popup = context.Render<SUIPopup>();
        popup.WaitForAssertion(() => Assert.Contains("sync", module.Calls));
        var disposal = popup.InvokeAsync(async () => await popup.Instance.DisposeAsync());
        Assert.Equal(0, module.Disposals);
        module.Pending.SetResult();
        await disposal;
        Assert.Equal(new[] { "sync", "disconnect" }, module.Calls);
        Assert.Equal(1, module.Disposals);
    }

    private sealed class Runtime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async ValueTask<T> InvokeAsync<T>(string id, object?[]? args) => (T)(object)await Import.Task;
        public ValueTask<T> InvokeAsync<T>(string id, CancellationToken token, object?[]? args) => InvokeAsync<T>(id, args);
    }
    private sealed class Module : IJSObjectReference
    {
        public TaskCompletionSource? Pending { get; init; }
        public int Disposals { get; private set; }
        public List<string> Calls { get; } = [];
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
        public async ValueTask<T> InvokeAsync<T>(string id, object?[]? args)
        {
            Calls.Add(id);
            if (id == "sync" && Pending != null) await Pending.Task;
            return default!;
        }
        public ValueTask<T> InvokeAsync<T>(string id, CancellationToken token, object?[]? args) => InvokeAsync<T>(id, args);
    }
}
