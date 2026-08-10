using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Dialog service. Shows a Blazor component as a modal overlay and returns a
/// awaitable result. Register via <c>AddSufficitUI</c> and render
/// <c>&lt;SUIDialogHost&gt;</c> in the app shell.
/// </summary>
public interface ISUIDialogService
{
    /// <summary>Shows a dialog of type <typeparamref name="T"/> with the given
    /// parameters and returns a reference whose <see cref="SUIDialogReference.Result"/>
    /// completes when the dialog closes.</summary>
    Task<SUIDialogReference> ShowAsync<T>(string title, IDictionary<string, object?>? parameters = null) where T : Microsoft.AspNetCore.Components.ComponentBase;

    /// <summary>Convenience: show a confirm dialog (OK/Cancel) and return true on OK.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>Event raised when a dialog is requested. The host subscribes.</summary>
    event Action<SUIDialogRequest>? OnShow;
}

/// <summary>A request to show a dialog, consumed by <c>SUIDialogHost</c>.</summary>
public sealed record SUIDialogRequest(Guid Id, Type ComponentType, string Title, IReadOnlyDictionary<string, object?> Parameters, SUIDialogReference Reference);

/// <summary>A reference to a shown dialog. <see cref="Result"/> completes when closed.</summary>
public sealed class SUIDialogReference
{
    private readonly TaskCompletionSource<object?> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Guid Id { get; } = Guid.NewGuid();
    public Task<object?> Result => _tcs.Task;

    /// <summary>Closes the dialog with a result. Idempotent.</summary>
    public void Complete(object? result) => _tcs.TrySetResult(result);
}
