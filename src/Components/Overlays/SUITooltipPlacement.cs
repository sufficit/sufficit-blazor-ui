namespace Sufficit.Blazor.UI.Components;

/// <summary>Preferred side used to place an <see cref="SUITooltip"/>.</summary>
public enum SUITooltipPlacement
{
    /// <summary>No preference: tries right, left, top then bottom and keeps the first side that fits the viewport.</summary>
    Auto,
    /// <summary>Prefer the right side; falls back to left, top, bottom.</summary>
    Right,
    /// <summary>Prefer the left side; falls back to right, top, bottom.</summary>
    Left,
    /// <summary>Prefer above the target (the SUITooltip default); falls back to bottom, right, left.</summary>
    Top,
    /// <summary>Prefer below the target; falls back to top, right, left.</summary>
    Bottom,
}
