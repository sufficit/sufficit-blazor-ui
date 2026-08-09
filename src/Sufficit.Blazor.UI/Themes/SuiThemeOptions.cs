namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Configuration for <c>AddSufficitUI</c>. Set <see cref="Theme"/> to supply
/// the consuming application's palette/typography/layout.
/// </summary>
public sealed class SuiThemeOptions
{
    /// <summary>
    /// The theme to register. When null, <see cref="DefaultSuiTheme"/> is used.
    /// </summary>
    public ISuiTheme? Theme { get; set; }
}
