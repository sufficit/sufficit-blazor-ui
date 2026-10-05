using Sufficit.Blazor.UI.Components;

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
    /// <remarks>The tone slug comes from the shared <c>SUIToneNormalizer</c>; entries expire <paramref name="durationMs"/> after being added.</remarks>
    public void Add(string message, SUITone tone = SUITone.Info, int durationMs = 4000)
    {
        var entry = new SUISnackbarEntry(
            Guid.NewGuid(),
            message,
            SUIToneNormalizer.Slug(tone),
            DateTime.UtcNow.AddMilliseconds(durationMs));
        OnEnqueue?.Invoke(entry);
    }
}
