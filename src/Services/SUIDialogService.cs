using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Default <see cref="ISUIDialogService"/>. Builds <see cref="SUIDialogRequest"/>s
/// that <c>SUIDialogHost</c> renders, and wires the reference completion.
/// </summary>
public sealed class SUIDialogService : ISUIDialogService
{
    private readonly object _sync = new();
    private readonly Queue<SUIDialogRequest> _pendingRequests = new();
    private Action<SUIDialogRequest>? _onShow;

    public event Action<SUIDialogRequest>? OnShow
    {
        add
        {
            if (value is null)
                return;

            SUIDialogRequest[] pending;
            lock (_sync)
            {
                _onShow += value;
                pending = _pendingRequests.ToArray();
                _pendingRequests.Clear();
            }

            // InteractiveAuto can create the scoped service before the layout's
            // dialog host subscribes during the server-to-WASM transition. Keep
            // requests made in that interval instead of losing them and leaving
            // callers waiting forever for an unrendered dialog.
            foreach (var request in pending)
                value(request);
        }
        remove
        {
            lock (_sync)
                _onShow -= value;
        }
    }

    public Task<SUIDialogReference> ShowAsync<T>(string title, IDictionary<string, object?>? parameters = null)
        where T : ComponentBase
    {
        var reference = new SUIDialogReference();
        var request = new SUIDialogRequest(
            reference.Id,
            typeof(T),
            title,
            (IReadOnlyDictionary<string, object?>?)parameters ?? new Dictionary<string, object?>(),
            reference);
        Action<SUIDialogRequest>? handler;
        lock (_sync)
        {
            handler = _onShow;
            if (handler is null)
                _pendingRequests.Enqueue(request);
        }

        handler?.Invoke(request);
        return Task.FromResult(reference);
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Message"] = message,
        };
        var reference = await ShowAsync<SUIConfirmDialog>(title, parameters);
        var result = await reference.Result;
        return result is true;
    }
}
