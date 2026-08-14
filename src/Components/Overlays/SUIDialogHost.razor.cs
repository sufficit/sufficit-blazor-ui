using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Components;

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

    protected override void OnInitialized()
        => DialogService.OnShow += OnShow;

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
