namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Color tokens shared by every SUI component.
/// </summary>
public enum SUIColor
{
    /// <summary>Neutral default: SUIText renders <c>--sui-text-secondary</c>; controls keep their base styling (e.g. <c>sui-chip--default</c>).</summary>
    Default,
    /// <summary>Brand accent (<c>--sui-color-primary</c>).</summary>
    Primary,
    /// <summary>Secondary accent (<c>--sui-color-secondary</c>).</summary>
    Secondary,
    /// <summary>Informational emphasis (<c>--sui-color-info</c>).</summary>
    Info,
    /// <summary>Positive / confirmed state (<c>--sui-color-success</c>).</summary>
    Success,
    /// <summary>Caution state (<c>--sui-color-warning</c>).</summary>
    Warning,
    /// <summary>Failure or destructive state (<c>--sui-color-error</c>).</summary>
    Error,
    /// <summary>Dark slate surface (<c>--sui-color-dark</c>, e.g. <c>sui-chip--dark</c>, <c>sui-appbar--dark</c>); not mapped by SUIText.</summary>
    Dark,
    /// <summary>Light surface (<c>--sui-color-light</c>, e.g. <c>sui-overlay--light</c>); not mapped by SUIText.</summary>
    Light,
    /// <summary>Card / surface tone; no <c>--sui-color-*</c> token is defined for it, so components render their base styling.</summary>
    Surface,
    /// <summary>Inherit the surrounding color (text/icon).</summary>
    Inherit,
}

/// <summary>
/// Visual style of a control surface.
/// </summary>
public enum SUIVariant
{
    /// <summary>No border, no fill (transparent text-like surface).</summary>
    Text,
    /// <summary>Outlined (border only, transparent fill).</summary>
    Outlined,
    /// <summary>Solid filled surface (default).</summary>
    Filled,
    /// <summary>Tinted surface for SUIButton, SUILoadingButton and SUIIconButton.</summary>
    Soft,
}

/// <summary>
/// Size buckets for controls.
/// </summary>
public enum SUISize
{
    /// <summary>Compact control: <c>sui-btn--small</c> uses <c>--sui-control-h-sm</c>; <c>sui-icon--small</c> is 1.25em.</summary>
    Small,
    /// <summary>Default control height; buttons add no size modifier, <c>sui-icon--medium</c> is 1.5em.</summary>
    Medium,
    /// <summary>Roomy control: <c>sui-btn--large</c> uses <c>--sui-control-h-lg</c>; <c>sui-icon--large</c>.</summary>
    Large,
}

/// <summary>
/// Rotation direction of spinning indicators (loading buttons, circular
/// progress). Screen convention is clockwise; counter-clockwise exists for
/// glyphs whose artwork reads better unrolled the other way.
/// </summary>
public enum SUISpinDirection
{
    /// <summary>Default screen convention; no extra class is added.</summary>
    Clockwise,
    /// <summary>Reverses the animation (<c>sui-progress-circular--reverse</c> and the reverse spin class of SUILoadingButton).</summary>
    CounterClockwise,
}

/// <summary>
/// HTML button types.
/// </summary>
public enum SUIButtonType
{
    /// <summary><c>type="button"</c>: plain click handler, does not submit a form (default).</summary>
    Button,
    /// <summary><c>type="submit"</c>: submits the enclosing form.</summary>
    Submit,
    /// <summary><c>type="reset"</c>: resets the enclosing form's fields.</summary>
    Reset,
}

/// <summary>
/// Edge shape for icon buttons.
/// </summary>
public enum SUIEdge
{
    /// <summary>Square / rounded-rectangle edge.</summary>
    False,
    /// <summary>Fully circular edge.</summary>
    True,
    /// <summary>Half-circle edge on one side (fab-like).</summary>
    Center,
}

/// <summary>
/// Typography scale.
/// </summary>
public enum SUITypo
{
    /// <summary>Largest heading (<c>--sui-fs-h1</c>, weight 700); SUITextTag.Auto renders <c>&lt;h1&gt;</c>.</summary>
    h1,
    /// <summary>Second-level heading (<c>--sui-fs-h2</c>, weight 700); Auto renders <c>&lt;h2&gt;</c>.</summary>
    h2,
    /// <summary>Third-level heading (<c>--sui-fs-h3</c>, weight 600); Auto renders <c>&lt;h3&gt;</c>.</summary>
    h3,
    /// <summary>Fourth-level heading (<c>--sui-fs-h4</c>, weight 600); Auto renders <c>&lt;h4&gt;</c>.</summary>
    h4,
    /// <summary>Fifth-level heading (<c>--sui-fs-h5</c>, weight 600); Auto renders <c>&lt;h5&gt;</c>.</summary>
    h5,
    /// <summary>Smallest heading (<c>--sui-fs-h6</c>, weight 600); Auto renders <c>&lt;h6&gt;</c>.</summary>
    h6,
    /// <summary>Primary subtitle (<c>--sui-fs-subtitle1</c>, weight 500); used by the compact SUIDrawer title.</summary>
    subtitle1,
    /// <summary>Secondary, smaller subtitle (<c>--sui-fs-subtitle2</c>, weight 500).</summary>
    subtitle2,
    /// <summary>Default body copy (<c>--sui-fs-body1</c>); the SUIText default.</summary>
    body1,
    /// <summary>Smaller body copy (<c>--sui-fs-body2</c>).</summary>
    body2,
    /// <summary>Button label style (<c>--sui-fs-button</c>, weight 600, .02em tracking).</summary>
    button,
    /// <summary>Small helper text (<c>--sui-fs-caption</c>); used by pagination and stat labels.</summary>
    caption,
    /// <summary>Uppercase micro-label (<c>--sui-fs-overline</c>, weight 600, .08em tracking).</summary>
    overline,
    /// <summary>Primary page title in dense operational products.</summary>
    display,
    /// <summary>Dominant status or diagnostic title.</summary>
    headline,
    /// <summary>Surface and section title.</summary>
    title,
    /// <summary>Compact operational body copy.</summary>
    body,
    /// <summary>Control, table and metadata label.</summary>
    label,
    /// <summary>Structured identifiers, schedules and code-like values.</summary>
    mono,
}

/// <summary>
/// Horizontal text alignment.
/// </summary>
public enum SUIAlign
{
    /// <summary>Align to the inline start (left in LTR); adds <c>sui-align-start</c>.</summary>
    Start,
    /// <summary>Center the text; adds <c>sui-align-center</c>.</summary>
    Center,
    /// <summary>Align to the inline end (right in LTR); adds <c>sui-align-end</c>.</summary>
    End,
    /// <summary>Stretch lines to both edges; adds <c>sui-align-justify</c>.</summary>
    Justify,
}

/// <summary>Semantic HTML element rendered by <c>SUIText</c>.</summary>
public enum SUITextTag
{
    /// <summary>Map h1-h6 typography to the matching heading; otherwise use div.</summary>
    Auto,
    /// <summary>Always render a <c>&lt;div&gt;</c>.</summary>
    Div,
    /// <summary>Always render an inline <c>&lt;span&gt;</c>.</summary>
    Span,
    /// <summary>Always render a <c>&lt;p&gt;</c> paragraph.</summary>
    P,
    /// <summary>Force an <c>&lt;h1&gt;</c> regardless of the typography scale.</summary>
    H1,
    /// <summary>Force an <c>&lt;h2&gt;</c> regardless of the typography scale.</summary>
    H2,
    /// <summary>Force an <c>&lt;h3&gt;</c> regardless of the typography scale.</summary>
    H3,
    /// <summary>Force an <c>&lt;h4&gt;</c> regardless of the typography scale.</summary>
    H4,
    /// <summary>Force an <c>&lt;h5&gt;</c> regardless of the typography scale.</summary>
    H5,
    /// <summary>Force an <c>&lt;h6&gt;</c> regardless of the typography scale.</summary>
    H6,
}

/// <summary>
/// Anchor points used by popover/flyout positioning.
/// </summary>
public enum SUIOrigin
{
    /// <summary>Top edge, left corner.</summary>
    TopLeft,
    /// <summary>Top edge, horizontally centered.</summary>
    TopCenter,
    /// <summary>Top edge, right corner.</summary>
    TopRight,
    /// <summary>Vertically centered on the left edge.</summary>
    CenterLeft,
    /// <summary>Exact center of the box.</summary>
    Center,
    /// <summary>Vertically centered on the right edge.</summary>
    CenterRight,
    /// <summary>Bottom edge, left corner.</summary>
    BottomLeft,
    /// <summary>Bottom edge, horizontally centered.</summary>
    BottomCenter,
    /// <summary>Bottom edge, right corner.</summary>
    BottomRight,
}

/// <summary>
/// Semantic tone for status badges, alerts and similar emphasis surfaces.
/// Replaces the string-based <c>Tone</c> of the Identity StatusBadge.
/// </summary>
public enum SUITone
{
    /// <summary>Muted, no semantic meaning (<c>sui-status-badge--neutral</c>); default for badges, banners and choice cards.</summary>
    Neutral,
    /// <summary>Positive / completed state (<c>*--success</c> modifiers).</summary>
    Success,
    /// <summary>Needs attention (<c>*--warning</c> modifiers).</summary>
    Warning,
    /// <summary>Error or destructive state (<c>*--danger</c> modifiers).</summary>
    Danger,
    /// <summary>Informational (<c>*--info</c> modifiers); default for SUIAlert.</summary>
    Info,
}

/// <summary>
/// Sort state of a table column.
/// </summary>
public enum SUISortDirection
{
    /// <summary>Not sorted by this column.</summary>
    None,
    /// <summary>Sorted low-to-high; the sort label becomes active with an upward arrow.</summary>
    Ascending,
    /// <summary>Sorted high-to-low; adds <c>sui-sort-label--descending</c>, flipping the arrow.</summary>
    Descending,
}
