using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class MenuLifecycleTests
{
    [Fact]
    public async Task DisposalDuringImportReleasesLateModuleWithoutConnecting()
    {
        await using var context = new BunitContext();
        var runtime = new Runtime(); context.Services.AddSingleton<IJSRuntime>(runtime);
        var cut = context.Render<SUIMenu>();
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());
        var module = new Module(); runtime.Import.SetResult(module);
        cut.WaitForAssertion(() => Assert.Equal(1, module.Disposals));
        Assert.Empty(module.Calls);
    }

    [Fact]
    public async Task DisposalDuringConnectWaitsForItsCompletionBeforeReleasingListeners()
    {
        await using var context = new BunitContext();
        var module = new Module { Connect = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var runtime = new Runtime(); runtime.Import.SetResult(module);
        context.Services.AddSingleton<IJSRuntime>(runtime);
        var cut = context.Render<SUIMenu>();
        cut.WaitForAssertion(() => Assert.Contains("connect", module.Calls));
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());
        Assert.Equal(0, module.Disposals);
        module.Connect.SetResult();
        cut.WaitForAssertion(() => Assert.Equal(1, module.Disposals));
        Assert.Equal(new[] { "connect", "disconnect" }, module.Calls);
        await cut.InvokeAsync(async () => await cut.Instance.DisposeAsync());
        Assert.Equal(1, module.Disposals);
    }

    private sealed class Runtime : IJSRuntime
    {
        public TaskCompletionSource<IJSObjectReference> Import { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => (T)(object)await Import.Task;
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
    private sealed class Module : IJSObjectReference
    {
        public TaskCompletionSource? Connect { get; init; }
        public int Disposals { get; private set; }
        public List<string> Calls { get; } = [];
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
        public async ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args)
        {
            Calls.Add(identifier);
            if (identifier == "disconnect") Assert.StartsWith("sui-menu-", Assert.IsType<string>(args![0]));
            if (identifier == "connect" && Connect != null) await Connect.Task;
            return default!;
        }
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
}
