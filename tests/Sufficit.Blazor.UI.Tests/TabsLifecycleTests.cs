using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class TabsLifecycleTests
{
    [Fact]
    public async Task DisposalDuringModuleImportDoesNotInitializeRemovedElement()
    {
        await using var context = new BunitContext();
        var runtime = new DelayedRuntime();
        context.Services.AddSingleton<IJSRuntime>(runtime);
        var cut = context.Render<SUITabs>();
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());
        var module = new Module();
        runtime.Import.SetResult(module);
        cut.WaitForAssertion(() => Assert.True(module.Disposed));
        Assert.Empty(module.Calls);
    }

    private sealed class DelayedRuntime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => (TValue)(object)await Import.Task;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);
    }

    private sealed class Module : IJSObjectReference
    {
        public bool Disposed { get; private set; }
        public List<string> Calls { get; } = [];
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add(identifier);
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);
    }
}
