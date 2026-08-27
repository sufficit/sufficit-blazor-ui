namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Color tokens shared by every SUI component.
/// </summary>
public enum SUIColor
{
    Default,
    Primary,
    Secondary,
    Info,
    Success,
    Warning,
    Error,
    Dark,
    Light,
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
}

/// <summary>
/// Size buckets for controls.
/// </summary>
public enum SUISize
{
    Small,
    Medium,
    Large,
}

/// <summary>
/// Rotation direction of spinning indicators (loading buttons, circular
/// progress). Screen convention is clockwise; counter-clockwise exists for
/// glyphs whose artwork reads better unrolled the other way.
/// </summary>
public enum SUISpinDirection
{
    Clockwise,
    CounterClockwise,
}

/// <summary>
/// HTML button types.
/// </summary>
public enum SUIButtonType
{
    Button,
    Submit,
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
    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    subtitle1,
    subtitle2,
    body1,
    body2,
    button,
    caption,
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
    Start,
    Center,
    End,
    Justify,
}

/// <summary>Semantic HTML element rendered by <c>SUIText</c>.</summary>
public enum SUITextTag
{
    /// <summary>Map h1-h6 typography to the matching heading; otherwise use div.</summary>
    Auto,
    Div,
    Span,
    P,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6,
}

/// <summary>
/// Anchor points used by popover/flyout positioning.
/// </summary>
public enum SUIOrigin
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

/// <summary>
/// Semantic tone for status badges, alerts and similar emphasis surfaces.
/// Replaces the string-based <c>Tone</c> of the Identity StatusBadge.
/// </summary>
public enum SUITone
{
    Neutral,
    Success,
    Warning,
    Danger,
    Info,
}

/// <summary>
/// Sort state of a table column.
/// </summary>
public enum SUISortDirection
{
    /// <summary>Not sorted by this column.</summary>
    None,
    Ascending,
    Descending,
}
