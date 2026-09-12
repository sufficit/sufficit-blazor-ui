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
    /// <summary>Color tokens, emitted as <c>--sui-color-*</c>, <c>--sui-surface*</c>, <c>--sui-text-*</c> and <c>--sui-border*</c>.</summary>
    SUIPalette Palette { get; }
    /// <summary>Typography tokens, emitted as <c>--sui-font*</c>, <c>--sui-fs-*</c>, <c>--sui-lh-*</c> and <c>--sui-ls-*</c>.</summary>
    SUITypography Typography { get; }
    /// <summary>Shape, spacing, elevation, motion and control-size tokens, emitted as <c>--sui-radius*</c>, <c>--sui-space-*</c>, <c>--sui-shadow-*</c>, <c>--sui-transition*</c> and <c>--sui-control-*</c>.</summary>
    SUILayout Layout { get; }

    /// <summary>Whether dark-mode tokens should apply.</summary>
    bool IsDark { get; }
}
