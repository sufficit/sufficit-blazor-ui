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

    /// <summary>Shows a dialog with optional presentation settings. Older providers may ignore these settings.</summary>
    Task<SUIDialogReference> ShowAsync<T>(string title, IDictionary<string, object?>? parameters, SUIDialogOptions options)
        where T : Microsoft.AspNetCore.Components.ComponentBase => ShowAsync<T>(title, parameters);

    /// <summary>Convenience: show a confirm dialog (OK/Cancel) and return true on OK.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>Event raised when a dialog is requested. The host subscribes.</summary>
    event Action<SUIDialogRequest>? OnShow;
}

/// <summary>A request to show a dialog, consumed by <c>SUIDialogHost</c>.</summary>
public sealed record SUIDialogRequest(Guid Id, Type ComponentType, string Title, IReadOnlyDictionary<string, object?> Parameters, SUIDialogReference Reference)
{
    /// <summary>Optional presentation settings for the host header.</summary>
    public SUIDialogOptions? Options { get; init; }
}

/// <summary>Optional visual treatment of the dialog header; does not affect its result.</summary>
public sealed record SUIDialogOptions
{
    /// <summary>Icon name or SUIIcons SVG accepted by SUIIcon.</summary>
    public string? Icon { get; init; }
}

/// <summary>A reference to a shown dialog. <see cref="Result"/> completes when closed.</summary>
public sealed class SUIDialogReference
{
    private readonly TaskCompletionSource<object?> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Unique id of this dialog instance, matched by the host to the <see cref="SUIDialogRequest"/>.</summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>Completes with the value passed to <see cref="Complete"/> when the dialog closes; null when dismissed without a result.</summary>
    public Task<object?> Result => _tcs.Task;

    /// <summary>Closes the dialog with a result. Idempotent.</summary>
    public void Complete(object? result) => _tcs.TrySetResult(result);
}
