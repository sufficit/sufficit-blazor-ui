namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Toast service. Shows transient, assertive notifications stacked at the
/// bottom-right of the screen. Unlike <see cref="ISUISnackbar"/> (polite,
/// informational), toasts use <c>role="alert"</c> and are meant for outcomes
/// that demand attention, such as failures. Register via <c>AddSufficitUI</c>
/// and render <c>&lt;SUIToastHost&gt;</c> inside the app shell.
/// </summary>
public interface ISUIToast
{
    /// <summary>Shows a toast with the given tone (info/success/warning/danger/error),
    /// duration and optional action button.</summary>
    void Add(string message, string severity = "info", int durationMs = 6000, string? actionLabel = null, Action? onAction = null);

    /// <summary>Convenience: informational toast.</summary>
    void Info(string message) => Add(message, "info");

    /// <summary>Convenience: positive-outcome toast.</summary>
    void Success(string message) => Add(message, "success");

    /// <summary>Convenience: caution toast.</summary>
    void Warning(string message) => Add(message, "warning");

    /// <summary>Convenience: error/danger toast.</summary>
    void Error(string message) => Add(message, "danger");

    /// <summary>Event raised when a new toast is queued. The host subscribes.</summary>
    event Action<SUIToastEntry>? OnEnqueue;
}

/// <summary>A single toast entry shown by <c>SUIToastHost</c>.</summary>
/// <param name="Id">Unique entry identifier used for dismissal.</param>
/// <param name="Message">Plain text shown in the toast body.</param>
/// <param name="Severity">Tone token: info/success/warning/danger (already normalized).</param>
/// <param name="ExpiresAt">UTC instant after which the host dismisses the entry.</param>
/// <param name="ActionLabel">Optional action button label (e.g. "Tentar de novo").</param>
/// <param name="OnAction">Optional callback invoked when the action button is clicked; the toast is dismissed afterwards.</param>
public sealed record SUIToastEntry(
    Guid Id,
    string Message,
    string Severity,
    DateTime ExpiresAt,
    string? ActionLabel = null,
    Action? OnAction = null);
