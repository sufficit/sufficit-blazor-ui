namespace Sufficit.Blazor.UI.Themes;

/// <summary>Immutable theme configuration with ready-to-use light and dark presets.</summary>
public sealed record SUITheme : ISUITheme
{
    /// <summary>Color tokens; defaults to <see cref="SUIPalette.Default"/>.</summary>
    public SUIPalette Palette { get; init; } = SUIPalette.Default;
    /// <summary>Typography tokens; defaults to <see cref="SUITypography.Default"/>.</summary>
    public SUITypography Typography { get; init; } = SUITypography.Default;
    /// <summary>Shape, spacing, elevation and motion tokens; defaults to <see cref="SUILayout.Default"/>.</summary>
    public SUILayout Layout { get; init; } = SUILayout.Default;
    /// <summary>Whether dark-mode tokens should apply. Default false.</summary>
    public bool IsDark { get; init; }

    /// <summary>Light preset: all default tokens, <see cref="IsDark"/> false.</summary>
    public static SUITheme Light { get; } = new();
    /// <summary>Dark preset: slate surfaces, light text and lightened semantic colors with dark contrast foregrounds.</summary>
    public static SUITheme Dark { get; } = new()
    {
        IsDark = true,
        Palette = new()
        {
            Primary = "#93c5fd", PrimaryContrast = "#0f172a",
            Secondary = "#cbd5e1", SecondaryContrast = "#0f172a",
            Surface = "#0f172a", Surface2 = "#1e293b", Surface3 = "#334155",
            TextPrimary = "#f1f5f9", TextSecondary = "#cbd5e1", TextDisabled = "#94a3b8",
            Border = "#334155", BorderStrong = "#64748b", Overlay = "rgba(0,0,0,.6)",
            Info = "#7dd3fc", Success = "#4ade80", Warning = "#fbbf24", Error = "#fca5a5",
            InfoContrast = "#0f172a", SuccessContrast = "#0f172a",
            WarningContrast = "#0f172a", ErrorContrast = "#0f172a",
        },
    };

    /// <summary>Quiet light preset inspired by Linear's restrained structure; opt-in.</summary>
    public static SUITheme LinearInspiredLight { get; } = Light with
    {
        Palette = Light.Palette with
        {
            Primary = "#495f7d", PrimaryContrast = "#ffffff",
            PrimaryAction = "#35435a", PrimaryActionContrast = "#ffffff",
            Surface = "#ffffff", Surface2 = "#f6f7f8", Surface3 = "#e9ebee",
            TextPrimary = "#202630", TextSecondary = "#555e6b",
            Border = "#e3e6ea", BorderStrong = "#cdd3da",
            Focus = "oklch(55% .055 262)",
        },
        Layout = Light.Layout with
        {
            FocusShadow = "inset 0 0 0 1px var(--sui-focus-color)",
            Shadow1 = "0 1px 2px rgba(20,25,35,.035)",
            Shadow2 = "0 4px 10px rgba(20,25,35,.06)",
            Shadow3 = "0 12px 24px rgba(20,25,35,.1)",
        },
    };

    /// <summary>Dark companion to <see cref="LinearInspiredLight"/>; opt-in.</summary>
    public static SUITheme LinearInspiredDark { get; } = Dark with
    {
        Palette = Dark.Palette with
        {
            Primary = "#aebbd0", PrimaryContrast = "#17202d",
            PrimaryAction = "#c7d0df", PrimaryActionContrast = "#17202d",
            Surface = "#1f2329", Surface2 = "#191d23", Surface3 = "#2c323a",
            TextPrimary = "#f0f2f5", TextSecondary = "#b9c0c9",
            Border = "#343b45", BorderStrong = "#4b5460",
            Focus = "oklch(72% .06 262)",
        },
        Layout = Dark.Layout with
        {
            FocusShadow = "inset 0 0 0 1px var(--sui-focus-color)",
            Shadow1 = "0 1px 2px rgba(0,0,0,.15)",
            Shadow2 = "0 4px 10px rgba(0,0,0,.2)",
            Shadow3 = "0 12px 24px rgba(0,0,0,.35)",
        },
    };
}
