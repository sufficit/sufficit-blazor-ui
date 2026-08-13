namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Typography tokens. Each consuming application can set its own font family
/// and the type scale; components reference these via the generated CSS
/// variables (<c>--sui-font</c>, <c>--sui-fs-h1</c>, etc.).
/// </summary>
public sealed record SUITypography
{
    public string FontFamily { get; init; } =
        "-apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, Helvetica, Arial, sans-serif";

    public string FontFamilyMono { get; init; } =
        "ui-monospace, SFMono-Regular, \"SF Mono\", Menlo, Consolas, monospace";

    /// <summary>
    /// Optional compact-label family. It falls back to the main UI family so
    /// consumers do not need to load an additional web font.
    /// </summary>
    public string FontFamilyLabel { get; init; } = "var(--sui-font)";

    public string FsH1 { get; init; } = "2.5rem";
    public string FsH2 { get; init; } = "2rem";
    public string FsH3 { get; init; } = "1.6rem";
    public string FsH4 { get; init; } = "1.35rem";
    public string FsH5 { get; init; } = "1.15rem";
    public string FsH6 { get; init; } = "1rem";
    public string FsSubtitle1 { get; init; } = "1rem";
    public string FsSubtitle2 { get; init; } = ".875rem";
    public string FsBody1 { get; init; } = "1rem";
    public string FsBody2 { get; init; } = ".875rem";
    public string FsButton { get; init; } = ".875rem";
    public string FsCaption { get; init; } = ".75rem";
    public string FsOverline { get; init; } = ".6875rem";

    // Semantic operational ramp. These roles are additive: the legacy h1-h6
    // scale above remains unchanged for existing consumers.
    public string FsDisplay { get; init; } = "clamp(1.55rem, 2.2vw, 2.25rem)";
    public string FsHeadline { get; init; } = "1.28rem";
    public string FsTitle { get; init; } = "1rem";
    public string FsBody { get; init; } = ".875rem";
    public string FsLabel { get; init; } = ".75rem";
    public string FsMono { get; init; } = ".76rem";

    public string LineHeightDisplay { get; init; } = "1.2";
    public string LineHeightHeadline { get; init; } = "1.2";
    public string LineHeightTitle { get; init; } = "1.2";
    public string LineHeightBody { get; init; } = "1.45";
    public string LineHeightLabel { get; init; } = "1.2";
    public string LineHeightMono { get; init; } = "1.4";
    public string LetterSpacingDisplay { get; init; } = "-.025em";

    public static SUITypography Default { get; } = new();
}
