namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Theme implementation that mirrors the original hardcoded SUI tokens (light,
/// blue primary). Used as a fallback when a consumer does not supply its own
/// <see cref="ISuiTheme"/>.
/// </summary>
public sealed class DefaultSuiTheme : ISuiTheme
{
    public static DefaultSuiTheme Instance { get; } = new();

    public SuiPalette Palette => SuiPalette.Default;
    public SuiTypography Typography => SuiTypography.Default;
    public SuiLayout Layout => SuiLayout.Default;
    public bool IsDark => false;
}
