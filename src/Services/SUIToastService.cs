namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Default <see cref="ISUIToast"/> implementation. Enqueues entries that
/// <c>SUIToastHost</c> renders and auto-dismisses after the duration.
/// </summary>
public sealed class SUIToastService : ISUIToast
{
    /// <inheritdoc />
    public event Action<SUIToastEntry>? OnEnqueue;

    /// <inheritdoc />
    /// <remarks>"error" is normalized to "danger" (same tone); other severities are lower-cased.</remarks>
    public void Add(string message, string severity = "info", int durationMs = 6000, string? actionLabel = null, Action? onAction = null)
    {
        // "error" and "danger" are the same tone.
        var tone = severity.Equals("error", StringComparison.OrdinalIgnoreCase)
            ? "danger" : severity.ToLowerInvariant();
        var entry = new SUIToastEntry(
            Guid.NewGuid(),
            message,
            tone,
            DateTime.UtcNow.AddMilliseconds(durationMs),
            string.IsNullOrWhiteSpace(actionLabel) ? null : actionLabel,
            onAction);
        OnEnqueue?.Invoke(entry);
    }
}
