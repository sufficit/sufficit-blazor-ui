using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Services;

/// <summary>
/// Single normalizer for transient-feedback tones (snackbar/toast). Bridges the
/// legacy string severity surface onto <see cref="SUITone"/> so the
/// "error"→"danger" synonym and the lowercase slug rules live in exactly one
/// place, shared by <see cref="ISUISnackbar"/> and <see cref="ISUIToast"/>.
/// </summary>
internal static class SUIToneNormalizer
{
    /// <summary>
    /// Parses a legacy severity string. "error" is accepted as a synonym of
    /// "danger"; neutral, empty and unrecognized values fall back to
    /// <see cref="SUITone.Info"/> because the feedback surfaces define no
    /// neutral variant (no <c>sui-snackbar--neutral</c> / <c>sui-toast--neutral</c>
    /// exists in the stylesheets).
    /// </summary>
    public static SUITone Parse(string? severity)
        => severity?.Trim().ToLowerInvariant() switch
        {
            "success" => SUITone.Success,
            "warning" => SUITone.Warning,
            "danger" or "error" => SUITone.Danger,
            _ => SUITone.Info,
        };

    /// <summary>
    /// Renders the CSS slug for a tone (<c>info</c>/<c>success</c>/<c>warning</c>/
    /// <c>danger</c>), mapping <see cref="SUITone.Neutral"/> to <c>info</c> for
    /// the same reason as <see cref="Parse"/>.
    /// </summary>
    public static string Slug(SUITone tone)
        => (tone == SUITone.Neutral ? SUITone.Info : tone).ToString().ToLowerInvariant();
}
