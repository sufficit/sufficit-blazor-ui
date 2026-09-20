using System.Text;

namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Turns an <see cref="ISUITheme"/> into the CSS custom property block that
/// <c>SUIThemeProvider</c> publishes, rejecting any token value that could
/// escape the declaration it is written into.
/// </summary>
/// <remarks>
/// The provider writes this block into an inline <c>&lt;style&gt;</c> element as
/// raw markup, so a theme value is not data: it is code in two languages at
/// once. A palette entry of <c>#fff&lt;/style&gt;&lt;script&gt;…</c> closed the
/// element and ran script; one containing <c>;</c> or <c>}</c> added
/// declarations or whole rules; one containing <c>url(…)</c> made the browser
/// fetch an attacker-controlled address, which leaks that the page rendered.
/// Themes routinely carry per-tenant brand colors from a database, so treating
/// them as trusted was the wrong default.
///
/// Validation is structural rather than a grammar per token kind. The values in
/// real use are varied — <c>color-mix(in srgb, var(--sui-color-primary) 14%,
/// transparent)</c>, <c>clamp(1.55rem, 2.2vw, 2.25rem)</c>, font stacks with
/// quoted family names, multi-layer shadows — and a per-kind grammar would
/// reject valid CSS as fast as the ecosystem invents it. Instead every value
/// must survive the same three rules: no character that can leave the
/// declaration, no construct that fetches or evaluates, and only functions from
/// an allow list. That keeps unknown-but-harmless CSS working while closing the
/// escapes.
/// </remarks>
public static class SUIThemeCssWriter
{
    /// <summary>
    /// Selector the tokens are published under. Public so consumers and tests
    /// can assert it.
    /// </summary>
    /// <remarks>
    /// Both the document root and the provider's own wrapper: the foundations
    /// stylesheet declares a dark fallback on <c>[data-sui-theme="dark"]</c>,
    /// and custom properties resolve from the nearest ancestor, so tokens
    /// published on <c>:root</c> alone would lose to that fallback inside the
    /// wrapper. Portals attached to <c>body</c> keep reading the <c>:root</c>
    /// copy and stay on the same palette.
    /// </remarks>
    public const string TokenSelector = ":root,.sui-root[data-sui-theme]";

    /// <summary>Longest accepted token value. Well past any real shadow stack.</summary>
    private const int MaximumValueLength = 400;

    /// <summary>
    /// CSS functions a theme token may call. Everything here computes a value
    /// from its arguments; nothing fetches a resource or evaluates script.
    /// <c>url</c>, <c>image</c>, <c>image-set</c>, <c>element</c> and
    /// <c>expression</c> are absent on purpose.
    /// </summary>
    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "var", "calc", "clamp", "min", "max", "round", "abs",
        "rgb", "rgba", "hsl", "hsla", "hwb", "lab", "lch", "oklab", "oklch",
        "color", "color-mix", "light-dark", "contrast-color",
        "cubic-bezier", "steps", "linear",
    };

    /// <summary>
    /// Whether a value is safe to write into a CSS declaration. Consumers can
    /// call this to validate a theme before shipping it, instead of discovering
    /// at render time that a token was dropped.
    /// </summary>
    /// <param name="value">Token value, for example <c>#2563eb</c>.</param>
    /// <returns><see langword="true"/> when the value can be published as-is.</returns>
    public static bool IsSafeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length > MaximumValueLength) return false;

        // Characters that end the declaration, end the rule, end the element, or
        // start an at-rule. A value never legitimately needs any of them, and
        // each one alone is enough to turn a token into an injection.
        foreach (var rejected in value)
        {
            if (rejected is '<' or '>' or ';' or '{' or '}' or '@' or '\\' or '\0') return false;
            // Control characters, including the newline that would let a payload
            // hide from a single-line review of the rendered block.
            if (char.IsControl(rejected)) return false;
        }

        // Comment delimiters can hide the rest of a crafted block from review
        // and, unbalanced, swallow the declarations that follow.
        if (value.Contains("/*", StringComparison.Ordinal)
            || value.Contains("*/", StringComparison.Ordinal)) return false;

        return HasBalancedQuotesAndParentheses(value) && CallsOnlyAllowedFunctions(value);
    }

    /// <summary>
    /// Renders the theme as a CSS custom property block under
    /// <see cref="TokenSelector"/>.
    /// </summary>
    /// <param name="theme">Theme to publish.</param>
    /// <returns>
    /// The rule text, without the surrounding <c>&lt;style&gt;</c> element. Any
    /// token whose value fails <see cref="IsSafeValue"/> falls back to the
    /// corresponding default, so a single bad value degrades that one token
    /// instead of dropping the palette or breaking the page.
    /// </returns>
    public static string Write(ISUITheme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        var palette = theme.Palette;
        var typography = theme.Typography;
        var layout = theme.Layout;
        var fallbackPalette = SUIPalette.Default;
        var fallbackTypography = SUITypography.Default;
        var fallbackLayout = SUILayout.Default;

        var css = new StringBuilder(2048);
        css.Append(TokenSelector).Append('{');

        Append(css, "--sui-color-primary", palette.Primary, fallbackPalette.Primary);
        Append(css, "--sui-color-primary-contrast", palette.PrimaryContrast, fallbackPalette.PrimaryContrast);
        Append(css, "--sui-color-primary-action", palette.PrimaryAction ?? palette.Primary, fallbackPalette.Primary);
        Append(css, "--sui-color-primary-action-contrast", palette.PrimaryActionContrast ?? palette.PrimaryContrast, fallbackPalette.PrimaryContrast);
        Append(css, "--sui-color-primary-soft", palette.PrimarySoft, fallbackPalette.PrimarySoft);
        Append(css, "--sui-color-secondary", palette.Secondary, fallbackPalette.Secondary);
        Append(css, "--sui-color-secondary-contrast", palette.SecondaryContrast, fallbackPalette.SecondaryContrast);
        Append(css, "--sui-color-info", palette.Info, fallbackPalette.Info);
        Append(css, "--sui-color-info-contrast", palette.InfoContrast, fallbackPalette.InfoContrast);
        Append(css, "--sui-color-success", palette.Success, fallbackPalette.Success);
        Append(css, "--sui-color-success-contrast", palette.SuccessContrast, fallbackPalette.SuccessContrast);
        Append(css, "--sui-color-warning", palette.Warning, fallbackPalette.Warning);
        Append(css, "--sui-color-warning-contrast", palette.WarningContrast, fallbackPalette.WarningContrast);
        Append(css, "--sui-color-error", palette.Error, fallbackPalette.Error);
        Append(css, "--sui-color-error-contrast", palette.ErrorContrast, fallbackPalette.ErrorContrast);
        Append(css, "--sui-color-dark", palette.Dark, fallbackPalette.Dark);
        Append(css, "--sui-color-light", palette.Light, fallbackPalette.Light);
        Append(css, "--sui-surface", palette.Surface, fallbackPalette.Surface);
        Append(css, "--sui-surface-2", palette.Surface2, fallbackPalette.Surface2);
        Append(css, "--sui-surface-3", palette.Surface3, fallbackPalette.Surface3);
        Append(css, "--sui-text-primary", palette.TextPrimary, fallbackPalette.TextPrimary);
        Append(css, "--sui-text-secondary", palette.TextSecondary, fallbackPalette.TextSecondary);
        Append(css, "--sui-text-disabled", palette.TextDisabled, fallbackPalette.TextDisabled);
        Append(css, "--sui-border", palette.Border, fallbackPalette.Border);
        Append(css, "--sui-border-strong", palette.BorderStrong, fallbackPalette.BorderStrong);
        Append(css, "--sui-overlay", palette.Overlay, fallbackPalette.Overlay);

        Append(css, "--sui-font", typography.FontFamily, fallbackTypography.FontFamily);
        Append(css, "--sui-font-mono", typography.FontFamilyMono, fallbackTypography.FontFamilyMono);
        Append(css, "--sui-font-label", typography.FontFamilyLabel, fallbackTypography.FontFamilyLabel);
        Append(css, "--sui-fs-h1", typography.FsH1, fallbackTypography.FsH1);
        Append(css, "--sui-fs-h2", typography.FsH2, fallbackTypography.FsH2);
        Append(css, "--sui-fs-h3", typography.FsH3, fallbackTypography.FsH3);
        Append(css, "--sui-fs-h4", typography.FsH4, fallbackTypography.FsH4);
        Append(css, "--sui-fs-h5", typography.FsH5, fallbackTypography.FsH5);
        Append(css, "--sui-fs-h6", typography.FsH6, fallbackTypography.FsH6);
        Append(css, "--sui-fs-subtitle1", typography.FsSubtitle1, fallbackTypography.FsSubtitle1);
        Append(css, "--sui-fs-subtitle2", typography.FsSubtitle2, fallbackTypography.FsSubtitle2);
        Append(css, "--sui-fs-body1", typography.FsBody1, fallbackTypography.FsBody1);
        Append(css, "--sui-fs-body2", typography.FsBody2, fallbackTypography.FsBody2);
        Append(css, "--sui-fs-button", typography.FsButton, fallbackTypography.FsButton);
        Append(css, "--sui-fs-caption", typography.FsCaption, fallbackTypography.FsCaption);
        Append(css, "--sui-fs-overline", typography.FsOverline, fallbackTypography.FsOverline);
        Append(css, "--sui-fs-display", typography.FsDisplay, fallbackTypography.FsDisplay);
        Append(css, "--sui-fs-headline", typography.FsHeadline, fallbackTypography.FsHeadline);
        Append(css, "--sui-fs-title", typography.FsTitle, fallbackTypography.FsTitle);
        Append(css, "--sui-fs-body", typography.FsBody, fallbackTypography.FsBody);
        Append(css, "--sui-fs-label", typography.FsLabel, fallbackTypography.FsLabel);
        Append(css, "--sui-fs-mono", typography.FsMono, fallbackTypography.FsMono);
        Append(css, "--sui-lh-display", typography.LineHeightDisplay, fallbackTypography.LineHeightDisplay);
        Append(css, "--sui-lh-headline", typography.LineHeightHeadline, fallbackTypography.LineHeightHeadline);
        Append(css, "--sui-lh-title", typography.LineHeightTitle, fallbackTypography.LineHeightTitle);
        Append(css, "--sui-lh-body", typography.LineHeightBody, fallbackTypography.LineHeightBody);
        Append(css, "--sui-lh-label", typography.LineHeightLabel, fallbackTypography.LineHeightLabel);
        Append(css, "--sui-lh-mono", typography.LineHeightMono, fallbackTypography.LineHeightMono);
        Append(css, "--sui-ls-display", typography.LetterSpacingDisplay, fallbackTypography.LetterSpacingDisplay);

        Append(css, "--sui-radius-sm", layout.RadiusSm, fallbackLayout.RadiusSm);
        Append(css, "--sui-radius", layout.Radius, fallbackLayout.Radius);
        Append(css, "--sui-radius-lg", layout.RadiusLg, fallbackLayout.RadiusLg);
        Append(css, "--sui-radius-full", layout.RadiusFull, fallbackLayout.RadiusFull);
        Append(css, "--sui-space-1", layout.Space1, fallbackLayout.Space1);
        Append(css, "--sui-space-2", layout.Space2, fallbackLayout.Space2);
        Append(css, "--sui-space-3", layout.Space3, fallbackLayout.Space3);
        Append(css, "--sui-space-4", layout.Space4, fallbackLayout.Space4);
        Append(css, "--sui-space-5", layout.Space5, fallbackLayout.Space5);
        Append(css, "--sui-space-6", layout.Space6, fallbackLayout.Space6);
        Append(css, "--sui-shadow-1", layout.Shadow1, fallbackLayout.Shadow1);
        Append(css, "--sui-shadow-2", layout.Shadow2, fallbackLayout.Shadow2);
        Append(css, "--sui-shadow-3", layout.Shadow3, fallbackLayout.Shadow3);
        Append(css, "--sui-transition", layout.Transition, fallbackLayout.Transition);
        Append(css, "--sui-transition-slow", layout.TransitionSlow, fallbackLayout.TransitionSlow);
        Append(css, "--sui-control-h-sm", layout.ControlHSm, fallbackLayout.ControlHSm);
        Append(css, "--sui-control-h-md", layout.ControlHMd, fallbackLayout.ControlHMd);
        Append(css, "--sui-control-h-lg", layout.ControlHLg, fallbackLayout.ControlHLg);
        Append(css, "--sui-control-px-sm", layout.ControlPxSm, fallbackLayout.ControlPxSm);
        Append(css, "--sui-control-px-md", layout.ControlPxMd, fallbackLayout.ControlPxMd);
        Append(css, "--sui-control-px-lg", layout.ControlPxLg, fallbackLayout.ControlPxLg);

        css.Append("color-scheme:").Append(theme.IsDark ? "dark" : "light").Append(';');
        css.Append('}');
        return css.ToString();
    }

    private static void Append(StringBuilder css, string name, string? value, string fallback)
    {
        var accepted = IsSafeValue(value) ? value! : fallback;
        // The fallback comes from the library's own defaults and always passes;
        // the guard is here so a future default cannot silently reopen the hole.
        if (!IsSafeValue(accepted)) return;
        css.Append(name).Append(':').Append(accepted).Append(';');
    }

    private static bool HasBalancedQuotesAndParentheses(string value)
    {
        var depth = 0;
        var quote = '\0';

        foreach (var character in value)
        {
            if (quote != '\0')
            {
                if (character == quote) quote = '\0';
                continue;
            }

            switch (character)
            {
                case '"':
                case '\'':
                    quote = character;
                    break;
                case '(':
                    depth++;
                    // A deeply nested value is more likely to be an attempt to
                    // confuse the parser than a real token.
                    if (depth > 8) return false;
                    break;
                case ')':
                    if (--depth < 0) return false;
                    break;
            }
        }

        return depth == 0 && quote == '\0';
    }

    private static bool CallsOnlyAllowedFunctions(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '(') continue;

            // Walk back over the identifier that precedes the parenthesis. CSS
            // function names are letters, digits and dashes; anything else ends
            // the name.
            var end = index;
            var start = end;
            while (start > 0 && (char.IsLetterOrDigit(value[start - 1]) || value[start - 1] == '-'))
                start--;

            var name = value[start..end];
            // "(" with no identifier in front is a grouping parenthesis, which
            // calc() and friends use legitimately.
            if (name.Length == 0) continue;
            if (!AllowedFunctions.Contains(name)) return false;
        }

        return true;
    }
}
