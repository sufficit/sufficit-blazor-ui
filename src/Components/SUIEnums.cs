namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Color tokens shared by every SUI component. Replaces MudBlazor.Color.
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
/// Visual style of a control surface. Replaces MudBlazor.Variant.
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
/// Size buckets for controls. Replaces MudBlazor.Size.
/// </summary>
public enum SUISize
{
    Small,
    Medium,
    Large,
}

/// <summary>
/// HTML button types. Replaces MudBlazor.ButtonType.
/// </summary>
public enum SUIButtonType
{
    Button,
    Submit,
    Reset,
}

/// <summary>
/// Edge shape for icon buttons. Replaces MudBlazor.Edge.
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
/// Typography scale. Replaces MudBlazor.Typo.
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
}

/// <summary>
/// Horizontal text alignment. Replaces MudBlazor.Align.
/// </summary>
public enum SUIAlign
{
    Start,
    Center,
    End,
    Justify,
}

/// <summary>
/// Anchor points used by popover/flyout positioning. Replaces MudBlazor.Origin.
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
