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
    public event Action<SUIDialogRequest>? OnShow;

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
        OnShow?.Invoke(request);
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
