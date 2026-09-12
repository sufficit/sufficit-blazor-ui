using Microsoft.JSInterop;

namespace Sufficit.Blazor.UI.Components;

public partial class SUISelect<T>
{
    private int _lastRevealedIndex = -1;
    /// <summary>Once items exist: loads the browser module, connects key handling on the trigger, opens or closes the top-layer menu only on transitions and reveals the active option.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_items.Count == 0)
        {
            return;
        }

        _module ??= await JS.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Sufficit.Blazor.UI/Components/Forms/SUISelect.razor.js?v=2");
        if (!_keyboardInteropConnected)
        {
            await _module.InvokeVoidAsync("connectSelectTrigger", _triggerElement);
            _keyboardInteropConnected = true;
        }

        // Only on the transition. openSelectMenu ends by realigning the list
        // (scrollTop = 0 + revealActiveOption), which is right when the menu opens
        // and destructive on every later render: a parent that re-renders on a
        // timer — a status poll, a progress bar — snatched the list back to the
        // top while the person was scrolling it, and re-placed the popover
        // under their cursor mid-click. Staying open is not an event; the JS
        // already keeps the position current through its own ResizeObserver
        // and window listeners.
        if (_open && !_interopOpen)
        {
            _interopOpen = true;
            await _module.InvokeVoidAsync(
                "openSelectMenu",
                _triggerElement,
                _menuElement);
        }
        else if (!_open && _interopOpen)
        {
            _interopOpen = false;
            await _module.InvokeVoidAsync("closeSelectMenu", _menuElement);
        }
        if (_open && _lastRevealedIndex != _activeIndex)
        {
            // Record the dispatched state before yielding: a newer render can
            // run while JS is pending and must still reveal its own selection.
            _lastRevealedIndex = _activeIndex;
            await _module.InvokeVoidAsync("revealActiveOption", _menuElement);
        }
        if (!_open) _lastRevealedIndex = -1;
    }

    /// <summary>Releases the form binding, the trigger key handling and the browser module.</summary>
    public async ValueTask DisposeAsync()
    {
        _field.Dispose();
        if (_module is null)
        {
            return;
        }

        try
        {
            if (_keyboardInteropConnected)
            {
                await _module.InvokeVoidAsync("disconnectSelectTrigger", _triggerElement);
            }
            if (_interopOpen)
            {
                await _module.InvokeVoidAsync("closeSelectMenu", _menuElement);
            }
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; browser-owned listeners disappear
            // with the document and there is no remote runtime left to notify.
        }
    }
}
