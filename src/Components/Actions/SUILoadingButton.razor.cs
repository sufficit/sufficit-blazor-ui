using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Click handling and the instant-feedback script of <see cref="SUILoadingButton"/>.</summary>
public partial class SUILoadingButton
{
    private IJSObjectReference? _module;
    private ElementReference _element;
    private bool _releasePending;
    private bool _awaitingParent;

    /// <summary>Clears an answered press and, on first render, loads the press-feedback script.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_releasePending)
        {
            await ReleasePendingAsync();
        }

        if (!firstRender || !InstantFeedback)
        {
            return;
        }

        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/Sufficit.Blazor.UI/Components/Actions/SUILoadingButton.razor.js");
            await _module.InvokeVoidAsync("register");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The press feedback is a courtesy: a button that cannot load it —
            // prerendering, a disconnected circuit, a test host without JS —
            // still behaves exactly as it did before.
        }
    }

    /// <summary>Releases the press-feedback script module.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (!IsLoading && !Disabled && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }

        if (!InstantFeedback)
        {
            return;
        }

        if (OnClick.HasDelegate || EffectiveButtonType != SUIButtonType.Submit || !string.IsNullOrWhiteSpace(Href))
        {
            _releasePending = true;
        }
        else
        {
            _awaitingParent = true;
        }
    }

    /// <summary>
    /// Clears the press marker once the click was answered without IsLoading.
    /// </summary>
    /// <remarks>
    /// Blazor only rewrites the class attribute when its value changes, so a
    /// server that answers before IsLoading ever turns on leaves the class
    /// untouched and the script's marker would linger until its timeout.
    /// While IsLoading is on, the class rewrite already removed the marker.
    /// </remarks>
    private async Task ReleasePendingAsync()
    {
        _releasePending = false;
        if (IsLoading || _module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("release", _element);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Same courtesy as register: the timeout still clears the marker.
        }
    }
}
