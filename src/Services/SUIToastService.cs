using Sufficit.Blazor.UI.Components;

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
    /// <remarks>The tone slug comes from the shared <c>SUIToneNormalizer</c> (never "error" or "neutral").</remarks>
    public void Add(string message, SUITone tone = SUITone.Info, int durationMs = 6000, string? actionLabel = null, Action? onAction = null)
    {
        var entry = new SUIToastEntry(
            Guid.NewGuid(),
            message,
            SUIToneNormalizer.Slug(tone),
            DateTime.UtcNow.AddMilliseconds(durationMs),
            string.IsNullOrWhiteSpace(actionLabel) ? null : actionLabel,
            onAction);
        OnEnqueue?.Invoke(entry);
    }
}
