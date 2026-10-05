using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Snackbar/toast service. Shows a transient message at the bottom of the
/// screen. Register via <c>AddSufficitUI</c> and render <c>&lt;SUISnackbarHost&gt;</c>
/// inside the app shell.
/// </summary>
public interface ISUISnackbar
{
    /// <summary>Shows a message with the given tone (info/success/warning/danger). Neutral is rendered as info because the snackbar surface defines no neutral variant.</summary>
    void Add(string message, SUITone tone = SUITone.Info, int durationMs = 4000);

    /// <summary>
    /// Legacy bridge kept for source compatibility: accepts the historical
    /// severity string ("info"/"success"/"warning"/"danger"/"error") and
    /// normalizes it through the shared tone normalizer. Prefer the typed
    /// overload taking <see cref="SUITone"/>.
    /// </summary>
    [Obsolete("Use the typed overload Add(message, SUITone tone, durationMs). The string severity bridge will be removed in a future major version.")]
    void Add(string message, string severity, int durationMs = 4000)
        => Add(message, SUIToneNormalizer.Parse(severity), durationMs);

    /// <summary>Convenience: info message.</summary>
    void Info(string message) => Add(message, SUITone.Info);

    /// <summary>Convenience: success message.</summary>
    void Success(string message) => Add(message, SUITone.Success);

    /// <summary>Convenience: warning message.</summary>
    void Warning(string message) => Add(message, SUITone.Warning);

    /// <summary>Convenience: error/danger message.</summary>
    void Error(string message) => Add(message, SUITone.Danger);

    /// <summary>Event raised when a new snackbar is queued. The host subscribes.</summary>
    event Action<SUISnackbarEntry>? OnEnqueue;
}

/// <summary>A single snackbar entry shown by the host.</summary>
public sealed record SUISnackbarEntry(Guid Id, string Message, string Severity, DateTime ExpiresAt);
