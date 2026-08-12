using System;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Cascading context for nested navigation (SUINavGroup / SUINavLink).
/// Replaces the MudBlazor.NavigationContext record that used to flow through
/// the vendored NavMenu tree.
/// </summary>
public sealed record SUINavigationContext
{
    /// <summary>Unique id for the collapsible region (wired to aria-controls).</summary>
    public string MenuId { get; } = Guid.NewGuid().ToString("N");

    /// <summary>Whether the group (or any ancestor) is disabled.</summary>
    public bool Disabled { get; init; }

    /// <summary>Whether the group is currently expanded.</summary>
    public bool Expanded { get; init; }
}
