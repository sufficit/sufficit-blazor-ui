namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Default <see cref="ISUISnackbar"/> implementation. Enqueues entries that
/// <c>SUISnackbarHost</c> renders and auto-dismisses after the duration.
/// </summary>
public sealed class SUISnackbarService : ISUISnackbar
{
    /// <inheritdoc />
    public event Action<SUISnackbarEntry>? OnEnqueue;

    /// <inheritdoc />
    /// <remarks>"error" is normalized to "danger"; other severities are lower-cased. Entries expire <paramref name="durationMs"/> after being added.</remarks>
    public void Add(string message, string severity = "info", int durationMs = 4000)
    {
        // "error" and "danger" are the same tone.
        var tone = severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            ? "danger" : severity.ToLowerInvariant();
        var entry = new SUISnackbarEntry(
            Guid.NewGuid(),
            message,
            tone,
            DateTime.UtcNow.AddMilliseconds(durationMs));
        OnEnqueue?.Invoke(entry);
    }
}
