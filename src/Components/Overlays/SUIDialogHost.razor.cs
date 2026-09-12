using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Renders the dialog requested through <c>ISUIDialogService</c> as a modal
/// (<c>role="dialog"</c>, <c>aria-modal</c>, <c>aria-labelledby</c>) with a backdrop,
/// focus trap, Escape dismissal and focus restore handled by <c>SUIDialogHost.razor.js</c>.
/// Only one dialog is shown at a time; a new request completes the previous one with <c>null</c>.
/// </summary>
public partial class SUIDialogHost
{
    private SUIDialogRequest? _current;
    private IDictionary<string, object?> _dialogParameters = new Dictionary<string, object?>();
    private ElementReference _dialogElement;
    private IJSObjectReference? _module;
    private DotNetObjectReference<SUIDialogHost>? _dotNetReference;
    private Guid? _openedRequestId;
    private bool _hostInteropConnected;
    private bool _disposed;

    private string TitleId => $"sui-dialog-{_current?.Id:N}-title";

    /// <summary>Subscribes to <c>ISUIDialogService.OnShow</c>.</summary>
    protected override void OnInitialized()
        => DialogService.OnShow += OnShow;

    /// <summary>Loads the JS module, connects focus tracking once and opens the current request's dialog if it is not open yet.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed)
        {
            return;
        }

        _module ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Sufficit.Blazor.UI/Components/Overlays/SUIDialogHost.razor.js");
        _dotNetReference ??= DotNetObjectReference.Create(this);
        if (!_hostInteropConnected)
        {
            await _module.InvokeVoidAsync("connectDialogHost");
            _hostInteropConnected = true;
        }

        if (_current is null || _openedRequestId == _current.Id)
        {
            return;
        }

        await _module.InvokeVoidAsync("openDialog", _dialogElement, _dotNetReference);
        _openedRequestId = _current.Id;
    }

    private void OnShow(SUIDialogRequest request)
        => _ = InvokeAsync(() => ShowAsync(request));

    private async Task ShowAsync(SUIDialogRequest request)
    {
        if (_disposed)
        {
            request.Reference.Complete(null);
            return;
        }

        if (_current is { } previous)
        {
            await CloseInteropAsync();
            previous.Reference.Complete(null);
        }

        _current = request;
        _dialogParameters = new Dictionary<string, object?>(request.Parameters);
        _ = CloseWhenCompletedAsync(request);
        StateHasChanged();
    }

    private async Task CloseWhenCompletedAsync(SUIDialogRequest request)
    {
        await request.Reference.Result;
        await InvokeAsync(() => CloseRequestAsync(request));
    }

    private async Task CloseRequestAsync(SUIDialogRequest request)
    {
        if (_current?.Reference != request.Reference)
        {
            return;
        }

        await CloseInteropAsync();
        _current = null;
        _dialogParameters = new Dictionary<string, object?>();
        StateHasChanged();
    }

    private async Task DismissAsync()
    {
        if (_current is not { } current)
        {
            return;
        }

        current.Reference.Complete(null);
        await CloseRequestAsync(current);
    }

    /// <summary>Called from JS on Escape; completes the current dialog with a <c>null</c> result.</summary>
    [JSInvokable]
    public Task DismissFromKeyboardAsync() => DismissAsync();

    private async Task CloseInteropAsync()
    {
        if (_module is null || _openedRequestId is null)
        {
            _openedRequestId = null;
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("closeDialog", _dialogElement);
        }
        catch (JSDisconnectedException)
        {
            // The browser circuit is already gone; no focus can be restored there.
        }
        finally
        {
            _openedRequestId = null;
        }
    }

    /// <summary>Unsubscribes, completes any open dialog with <c>null</c> and releases the JS module.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DialogService.OnShow -= OnShow;
        _current?.Reference.Complete(null);
        await CloseInteropAsync();

        if (_module is not null)
        {
            try
            {
                if (_hostInteropConnected)
                {
                    await _module.InvokeVoidAsync("disconnectDialogHost");
                }
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The browser circuit already released the module.
            }
        }

        _dotNetReference?.Dispose();
    }
}
