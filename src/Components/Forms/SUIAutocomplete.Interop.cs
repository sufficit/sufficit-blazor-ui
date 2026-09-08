using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Sufficit.Blazor.UI.Components;

public partial class SUIAutocomplete<T>
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ElementReference _inputElement, _listElement;
    private IJSObjectReference? _module;
    private bool _interopOpen;
    private int _lastRevealedIndex = -1;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed) return;
        if (_module is null)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Sufficit.Blazor.UI/Components/Forms/SUIAutocomplete.razor.js");
            if (_disposed) { await _module.DisposeAsync(); _module = null; return; }
            await _module.InvokeVoidAsync("connect", _inputElement);
        }
        if (_open && !_interopOpen)
        {
            await _module.InvokeVoidAsync("openSelectMenu", _inputElement, _listElement);
            _interopOpen = true;
        }
        else if (!_open && _interopOpen)
        {
            await _module.InvokeVoidAsync("closeSelectMenu", _listElement);
            _interopOpen = false;
        }
        if (_open && _activeIndex != _lastRevealedIndex)
            await _module.InvokeVoidAsync("revealActiveOption", _listElement);
        _lastRevealedIndex = _open ? _activeIndex : -1;
    }

    private async ValueTask DisconnectAsync()
    {
        if (_module is null) return;
        try
        {
            await _module.InvokeVoidAsync("disconnect", _inputElement);
            await _module.InvokeVoidAsync("closeSelectMenu", _listElement);
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }
}
