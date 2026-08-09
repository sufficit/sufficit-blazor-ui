namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Inline SVG path markup (the inner content of an <c>&lt;svg&gt;</c>)
/// for the icons used as defaults by the SUI components. Each constant is a
/// ready-to-render <c>&lt;path&gt;</c> (or set of paths) drawn on a 24x24
/// viewBox with <c>fill="currentColor"</c>. Replaces the MudBlazor
/// <c>Icons.Material.Filled.*</c> constants.
/// </summary>
public static class SUIIcons
{
    /// <summary>Magnifier with a slash — "no results". Replaces SearchOff.</summary>
    public const string SearchOff =
        "<path d=\"M15.5 14h-.79l-.28-.27a6.5 6.5 0 0 0 1.48-5.34c-.47-2.78-2.79-5-5.59-5.34a6.505 6.505 0 0 0-7.27 7.27c.34 2.8 2.56 5.12 5.34 5.59a6.5 6.5 0 0 0 5.34-1.48l.27.28v.79l4.25 4.25c.41.41 1.08.41 1.49 0 .41-.41.41-1.08 0-1.49L15.5 14zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z\"/>" +
        "<path d=\"M2.7 2.7a.996.996 0 0 0-1.41 0c-.39.39-.39 1.02 0 1.41l16.31 16.32c.39.39 1.02.39 1.41 0s.39-1.02 0-1.41L2.7 2.7z\"/>";

    /// <summary>Down-pointing chevron. Replaces ArrowDropDown.</summary>
    public const string ArrowDropDown =
        "<path d=\"M8.71 11.71a.996.996 0 0 1 0-1.41l3.29-3.29c.63-.63 1.71-.18 1.71.71v6.59c0 .89-1.08 1.34-1.71.71L8.7 11.72z\"/>";

    /// <summary>Up-pointing chevron, used for collapsed nav groups.</summary>
    public const string ArrowDropUp =
        "<path d=\"M8.71 12.29 12 8.99c.63-.63 1.71-.18 1.71.71v6.59c0 .89-1.08 1.34-1.71.71l-3.29-3.29a.996.996 0 0 1 0-1.42z\"/>";

    /// <summary>Material-style circular indeterminate spinner arc.</summary>
    public const string ProgressCircular =
        "<path d=\"M12 4V1L8 5l4 4V6c3.31 0 6 2.69 6 6 0 1.01-.25 1.97-.7 2.8l1.46 1.46A7.93 7.93 0 0 0 20 12c0-4.42-3.58-8-8-8z\"/>";

    /// <summary>Inbox tray icon.</summary>
    public const string Inbox =
        "<path d=\"M19 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2zm0 16h-3.5l-1.2-2.4a3 3 0 0 0-2.6-1.6H11.3a3 3 0 0 0-2.6 1.6L7.5 19H5V5h14v14z\"/>";

    /// <summary>Folder icon.</summary>
    public const string Folder =
        "<path d=\"M10 4H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-8l-2-2z\"/>";
}
