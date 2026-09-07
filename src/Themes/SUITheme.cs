namespace Sufficit.Blazor.UI.Themes;

/// <summary>Immutable theme configuration with ready-to-use light and dark presets.</summary>
public sealed record SUITheme : ISUITheme
{
    public SUIPalette Palette { get; init; } = SUIPalette.Default;
    public SUITypography Typography { get; init; } = SUITypography.Default;
    public SUILayout Layout { get; init; } = SUILayout.Default;
    public bool IsDark { get; init; }

    public static SUITheme Light { get; } = new();
    public static SUITheme Dark { get; } = new()
    {
        IsDark = true,
        Palette = new()
        {
            Primary = "#93c5fd", PrimaryContrast = "#0f172a",
            Surface = "#0f172a", Surface2 = "#1e293b", Surface3 = "#334155",
            TextPrimary = "#f1f5f9", TextSecondary = "#cbd5e1", TextDisabled = "#94a3b8",
            Border = "#334155", BorderStrong = "#64748b", Overlay = "rgba(0,0,0,.6)",
            Info = "#7dd3fc", Success = "#4ade80", Warning = "#fbbf24", Error = "#fca5a5",
            InfoContrast = "#0f172a", SuccessContrast = "#0f172a",
            WarningContrast = "#0f172a", ErrorContrast = "#0f172a",
        },
    };
}
