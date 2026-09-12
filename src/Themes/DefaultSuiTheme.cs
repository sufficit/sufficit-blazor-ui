namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Theme implementation that mirrors the original hardcoded SUI tokens (light,
/// blue primary). Used as a fallback when a consumer does not supply its own
/// <see cref="ISUITheme"/>.
/// </summary>
public sealed class DefaultSUITheme : ISUITheme
{
    /// <summary>Shared singleton used by <c>AddSufficitUI</c> and <c>SUIThemeProvider</c> when no theme is configured.</summary>
    public static DefaultSUITheme Instance { get; } = new();

    /// <summary>Always <see cref="SUIPalette.Default"/>.</summary>
    public SUIPalette Palette => SUIPalette.Default;
    /// <summary>Always <see cref="SUITypography.Default"/>.</summary>
    public SUITypography Typography => SUITypography.Default;
    /// <summary>Always <see cref="SUILayout.Default"/>.</summary>
    public SUILayout Layout => SUILayout.Default;
    /// <summary>Always false; the default theme is light.</summary>
    public bool IsDark => false;
}
