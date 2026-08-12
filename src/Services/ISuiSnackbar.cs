namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Snackbar/toast service. Shows a transient message at the bottom of the
/// screen. Register via <c>AddSufficitUI</c> and render <c>&lt;SUISnackbarHost&gt;</c>
/// inside the app shell.
/// </summary>
public interface ISUISnackbar
{
    /// <summary>Shows a message with the given tone (info/success/warning/danger/error).</summary>
    void Add(string message, string severity = "info", int durationMs = 4000);

    /// <summary>Convenience: info message.</summary>
    void Info(string message) => Add(message, "info");

    /// <summary>Convenience: success message.</summary>
    void Success(string message) => Add(message, "success");

    /// <summary>Convenience: warning message.</summary>
    void Warning(string message) => Add(message, "warning");

    /// <summary>Convenience: error/danger message.</summary>
    void Error(string message) => Add(message, "danger");

    /// <summary>Event raised when a new snackbar is queued. The host subscribes.</summary>
    event Action<SUISnackbarEntry>? OnEnqueue;
}

/// <summary>A single snackbar entry shown by the host.</summary>
public sealed record SUISnackbarEntry(Guid Id, string Message, string Severity, DateTime ExpiresAt);
