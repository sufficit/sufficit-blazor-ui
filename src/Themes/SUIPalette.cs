namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Color tokens consumed by every SUI component. A consuming application
/// supplies its own palette so the components match the app's visual identity
/// (brand color, surface tones, text colors) rather than the library default.
/// </summary>
public sealed record SUIPalette
{
    /// <summary>Brand / accent color. Drives primary buttons, active nav, focus rings.</summary>
    public string Primary { get; init; } = "#2563eb";

    /// <summary>Text/icon color on top of <see cref="Primary"/>.</summary>
    public string PrimaryContrast { get; init; } = "#ffffff";

    /// <summary>
    /// Optional deeper surface used by filled primary actions. When omitted,
    /// filled actions use <see cref="Primary"/> for backward compatibility.
    /// Accent treatments such as focus rings, links and outlined buttons keep
    /// using <see cref="Primary"/>.
    /// </summary>
    public string? PrimaryAction { get; init; }

    /// <summary>
    /// Text/icon color on top of <see cref="PrimaryAction"/>. Falls back to
    /// <see cref="PrimaryContrast"/> when the action surface is not supplied.
    /// </summary>
    public string? PrimaryActionContrast { get; init; }

    /// <summary>Soft tint of the primary color, used for active backgrounds.</summary>
    public string PrimarySoft { get; init; } = "color-mix(in srgb, var(--sui-color-primary) 14%, transparent)";

    /// <summary>Secondary accent (avatars, neutral emphasis).</summary>
    public string Secondary { get; init; } = "#64748b";

    /// <summary>Contrast color on top of <see cref="Secondary"/>.</summary>
    public string SecondaryContrast { get; init; } = "#ffffff";

    /// <summary>Informational semantic color (info alerts, chips, snackbars); feeds <c>--sui-color-info</c>.</summary>
    public string Info { get; init; } = "#0369a1";
    /// <summary>Success semantic color; feeds <c>--sui-color-success</c>.</summary>
    public string Success { get; init; } = "#166534";
    /// <summary>Warning semantic color; feeds <c>--sui-color-warning</c>.</summary>
    public string Warning { get; init; } = "#92400e";
    /// <summary>Error/danger semantic color (validation, destructive actions); feeds <c>--sui-color-error</c>.</summary>
    public string Error { get; init; } = "#b91c1c";
    /// <summary>Foreground colors on filled semantic surfaces.</summary>
    public string InfoContrast { get; init; } = "#ffffff";
    /// <summary>Text/icon color on top of <see cref="Success"/>; feeds <c>--sui-color-success-contrast</c>.</summary>
    public string SuccessContrast { get; init; } = "#ffffff";
    /// <summary>Text/icon color on top of <see cref="Warning"/>; feeds <c>--sui-color-warning-contrast</c>.</summary>
    public string WarningContrast { get; init; } = "#ffffff";
    /// <summary>Text/icon color on top of <see cref="Error"/>; feeds <c>--sui-color-error-contrast</c>.</summary>
    public string ErrorContrast { get; init; } = "#ffffff";

    /// <summary>Neutral dark tone (dark app bar, dark chips, snackbar background); feeds <c>--sui-color-dark</c>.</summary>
    public string Dark { get; init; } = "#1e293b";
    /// <summary>Neutral light tone, the counterpart of <see cref="Dark"/>; feeds <c>--sui-color-light</c>.</summary>
    public string Light { get; init; } = "#f8fafc";

    /// <summary>Base surface (page background, cards).</summary>
    public string Surface { get; init; } = "#ffffff";

    /// <summary>Sunken surface (inputs, hovered rows).</summary>
    public string Surface2 { get; init; } = "#f1f5f9";

    /// <summary>Deeper sunken surface (borders-as-background, dividers).</summary>
    public string Surface3 { get; init; } = "#e2e8f0";

    /// <summary>Main text color on <see cref="Surface"/>; feeds <c>--sui-text-primary</c>.</summary>
    public string TextPrimary { get; init; } = "#0f172a";
    /// <summary>Muted text color (captions, helper text, secondary labels); feeds <c>--sui-text-secondary</c>.</summary>
    public string TextSecondary { get; init; } = "#475569";
    /// <summary>Text color of disabled controls and placeholders; feeds <c>--sui-text-disabled</c>.</summary>
    public string TextDisabled { get; init; } = "#94a3b8";

    /// <summary>Default border and divider color; feeds <c>--sui-border</c>.</summary>
    public string Border { get; init; } = "#e2e8f0";
    /// <summary>Stronger border for hovered inputs and emphasized outlines; feeds <c>--sui-border-strong</c>.</summary>
    public string BorderStrong { get; init; } = "#cbd5e1";

    /// <summary>Modal/scrim overlay color.</summary>
    public string Overlay { get; init; } = "rgba(15, 23, 42, .45)";

    /// <summary>Default palette matching the original hardcoded SUI tokens (light, blue).</summary>
    public static SUIPalette Default { get; } = new();
}
