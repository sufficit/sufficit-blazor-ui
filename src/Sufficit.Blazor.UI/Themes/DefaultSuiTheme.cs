namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Theme implementation that mirrors the original hardcoded SUI tokens (light,
/// blue primary). Used as a fallback when a consumer does not supply its own
/// <see cref="ISUITheme"/>.
/// </summary>
public sealed class DefaultSUITheme : ISUITheme
{
    public static DefaultSUITheme Instance { get; } = new();

    public SUIPalette Palette => SUIPalette.Default;
    public SUITypography Typography => SUITypography.Default;
    public SUILayout Layout => SUILayout.Default;
    public bool IsDark => false;
}
