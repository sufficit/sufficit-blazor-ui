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

    /// <summary>
    /// Animations enabled by default in every SUI component (combine flags, or
    /// <see cref="Components.SUIAnimation.None"/> to turn them all off). Components can override per instance.
    /// </summary>
    public Components.SUIAnimation Animations { get; set; } = Components.SUIAnimation.Default;
}
