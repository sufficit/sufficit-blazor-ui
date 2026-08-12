namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Configuration for <c>AddSufficitUI</c>. Set <see cref="Theme"/> to supply
/// the consuming application's palette/typography/layout.
/// </summary>
public sealed class SUIThemeOptions
{
    /// <summary>
    /// The theme to register. When null, <see cref="DefaultSUITheme"/> is used.
    /// </summary>
    public ISUITheme? Theme { get; set; }
}
