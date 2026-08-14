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

    /// <summary>Soft tint of the primary color, used for active backgrounds.</summary>
    public string PrimarySoft { get; init; } = "color-mix(in srgb, #2563eb 14%, transparent)";

    /// <summary>Secondary accent (avatars, neutral emphasis).</summary>
    public string Secondary { get; init; } = "#64748b";

    /// <summary>Contrast color on top of <see cref="Secondary"/>.</summary>
    public string SecondaryContrast { get; init; } = "#ffffff";

    public string Info { get; init; } = "#0369a1";
    public string Success { get; init; } = "#166534";
    public string Warning { get; init; } = "#92400e";
    public string Error { get; init; } = "#b91c1c";
    public string Dark { get; init; } = "#1e293b";
    public string Light { get; init; } = "#f8fafc";

    /// <summary>Base surface (page background, cards).</summary>
    public string Surface { get; init; } = "#ffffff";

    /// <summary>Sunken surface (inputs, hovered rows).</summary>
    public string Surface2 { get; init; } = "#f1f5f9";

    /// <summary>Deeper sunken surface (borders-as-background, dividers).</summary>
    public string Surface3 { get; init; } = "#e2e8f0";

    public string TextPrimary { get; init; } = "#0f172a";
    public string TextSecondary { get; init; } = "#475569";
    public string TextDisabled { get; init; } = "#94a3b8";

    public string Border { get; init; } = "#e2e8f0";
    public string BorderStrong { get; init; } = "#cbd5e1";

    /// <summary>Modal/scrim overlay color.</summary>
    public string Overlay { get; init; } = "rgba(15, 23, 42, .45)";

    /// <summary>Default palette matching the original hardcoded SUI tokens (light, blue).</summary>
    public static SUIPalette Default { get; } = new();
}
