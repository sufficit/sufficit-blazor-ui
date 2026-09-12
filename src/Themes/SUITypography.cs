namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Typography tokens. Each consuming application can set its own font family
/// and the type scale; components reference these via the generated CSS
/// variables (<c>--sui-font</c>, <c>--sui-fs-h1</c>, etc.).
/// </summary>
public sealed record SUITypography
{
    /// <summary>Main UI font stack; feeds <c>--sui-font</c>. Defaults to the platform system stack.</summary>
    public string FontFamily { get; init; } =
        "-apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, Helvetica, Arial, sans-serif";

    /// <summary>Monospace font stack for code and tabular data; feeds <c>--sui-font-mono</c>.</summary>
    public string FontFamilyMono { get; init; } =
        "ui-monospace, SFMono-Regular, \"SF Mono\", Menlo, Consolas, monospace";

    /// <summary>
    /// Optional compact-label family. It falls back to the main UI family so
    /// consumers do not need to load an additional web font.
    /// </summary>
    public string FontFamilyLabel { get; init; } = "var(--sui-font)";

    /// <summary>Font size of h1 headings; feeds <c>--sui-fs-h1</c>. Default 2.5rem.</summary>
    public string FsH1 { get; init; } = "2.5rem";
    /// <summary>Font size of h2 headings; feeds <c>--sui-fs-h2</c>. Default 2rem.</summary>
    public string FsH2 { get; init; } = "2rem";
    /// <summary>Font size of h3 headings; feeds <c>--sui-fs-h3</c>. Default 1.6rem.</summary>
    public string FsH3 { get; init; } = "1.6rem";
    /// <summary>Font size of h4 headings; feeds <c>--sui-fs-h4</c>. Default 1.35rem.</summary>
    public string FsH4 { get; init; } = "1.35rem";
    /// <summary>Font size of h5 headings; feeds <c>--sui-fs-h5</c>. Default 1.15rem.</summary>
    public string FsH5 { get; init; } = "1.15rem";
    /// <summary>Font size of h6 headings; feeds <c>--sui-fs-h6</c>. Default 1rem.</summary>
    public string FsH6 { get; init; } = "1rem";
    /// <summary>Font size of subtitle1 text; feeds <c>--sui-fs-subtitle1</c>. Default 1rem.</summary>
    public string FsSubtitle1 { get; init; } = "1rem";
    /// <summary>Font size of subtitle2 text; feeds <c>--sui-fs-subtitle2</c>. Default .875rem.</summary>
    public string FsSubtitle2 { get; init; } = ".875rem";
    /// <summary>Font size of body1 (default paragraph) text; feeds <c>--sui-fs-body1</c>. Default 1rem.</summary>
    public string FsBody1 { get; init; } = "1rem";
    /// <summary>Font size of body2 (secondary paragraph) text; feeds <c>--sui-fs-body2</c>. Default .875rem.</summary>
    public string FsBody2 { get; init; } = ".875rem";
    /// <summary>Font size of button labels; feeds <c>--sui-fs-button</c>. Default .875rem.</summary>
    public string FsButton { get; init; } = ".875rem";
    /// <summary>Font size of captions and helper text; feeds <c>--sui-fs-caption</c>. Default .75rem.</summary>
    public string FsCaption { get; init; } = ".75rem";
    /// <summary>Font size of overline (uppercase eyebrow) text; feeds <c>--sui-fs-overline</c>. Default .6875rem.</summary>
    public string FsOverline { get; init; } = ".6875rem";

    // Semantic operational ramp. These roles are additive: the legacy h1-h6
    // scale above remains unchanged for existing consumers.
    /// <summary>Font size of the display role (page-level hero text); feeds <c>--sui-fs-display</c>. Defaults to a viewport clamp between 1.55rem and 2.25rem.</summary>
    public string FsDisplay { get; init; } = "clamp(1.55rem, 2.2vw, 2.25rem)";
    /// <summary>Font size of the headline role (section headings, status banners); feeds <c>--sui-fs-headline</c>. Default 1.28rem.</summary>
    public string FsHeadline { get; init; } = "1.28rem";
    /// <summary>Font size of the title role (card and dialog titles); feeds <c>--sui-fs-title</c>. Default 1rem.</summary>
    public string FsTitle { get; init; } = "1rem";
    /// <summary>Font size of the operational body role; feeds <c>--sui-fs-body</c>. Default .875rem.</summary>
    public string FsBody { get; init; } = ".875rem";
    /// <summary>Font size of the label role (compact labels, badges, table headers); feeds <c>--sui-fs-label</c>. Default .75rem.</summary>
    public string FsLabel { get; init; } = ".75rem";
    /// <summary>Font size of the mono role (code, identifiers); feeds <c>--sui-fs-mono</c>. Default .76rem.</summary>
    public string FsMono { get; init; } = ".76rem";

    /// <summary>Unitless line height of the display role; feeds <c>--sui-lh-display</c>. Default 1.2.</summary>
    public string LineHeightDisplay { get; init; } = "1.2";
    /// <summary>Unitless line height of the headline role; feeds <c>--sui-lh-headline</c>. Default 1.2.</summary>
    public string LineHeightHeadline { get; init; } = "1.2";
    /// <summary>Unitless line height of the title role; feeds <c>--sui-lh-title</c>. Default 1.2.</summary>
    public string LineHeightTitle { get; init; } = "1.2";
    /// <summary>Unitless line height of the body role; feeds <c>--sui-lh-body</c>. Default 1.45.</summary>
    public string LineHeightBody { get; init; } = "1.45";
    /// <summary>Unitless line height of the label role; feeds <c>--sui-lh-label</c>. Default 1.2.</summary>
    public string LineHeightLabel { get; init; } = "1.2";
    /// <summary>Unitless line height of the mono role; feeds <c>--sui-lh-mono</c>. Default 1.4.</summary>
    public string LineHeightMono { get; init; } = "1.4";
    /// <summary>Letter spacing of the display role; feeds <c>--sui-ls-display</c>. Default -.025em.</summary>
    public string LetterSpacingDisplay { get; init; } = "-.025em";

    /// <summary>Default type scale matching the original hardcoded SUI tokens.</summary>
    public static SUITypography Default { get; } = new();
}
