namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Theme contract between the SUI component library and a consuming
/// application. Each app implements this to supply its own palette, typography
/// and density so the shared components render with the app's visual identity
/// (e.g. Sufficit Identity is red, Sufficit Blazor is amber).
///
/// Register an implementation via
/// <c>services.AddSufficitUI(opts =&gt; opts.Theme = new MyTheme())</c> and wrap
/// the app root with <c>&lt;SUIThemeProvider&gt;</c>. The provider emits the
/// theme as CSS variables (<c>--sui-color-*</c>, <c>--sui-font</c>, ...) on
/// <c>:root</c> and cascades the <see cref="ISUITheme"/> instance to child
/// components.
/// </summary>
public interface ISUITheme
{
    SUIPalette Palette { get; }
    SUITypography Typography { get; }
    SUILayout Layout { get; }

    /// <summary>Whether dark-mode tokens should apply.</summary>
    bool IsDark { get; }
}
