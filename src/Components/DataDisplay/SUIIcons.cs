namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Inline SVG path markup (the inner content of an <c>&lt;svg&gt;</c>)
/// for the icons used as defaults by the SUI components. Each constant is a
/// ready-to-render <c>&lt;path&gt;</c> (or set of paths) drawn on a 24x24
/// viewBox with <c>fill="currentColor"</c>.
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

    /// <summary>Hamburger menu (three lines).</summary>
    public const string Menu =
        "<path d=\"M3 6h18M3 12h18M3 18h18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/>";

    /// <summary>Logout / sign-out arrow.</summary>
    public const string Logout =
        "<path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    /// <summary>Plus / add.</summary>
    public const string Add =
        "<path d=\"M12 5v14M5 12h14\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/>";

    /// <summary>Play triangle.</summary>
    public const string Play =
        "<path d=\"M8 5v14l11-7z\"/>";

    /// <summary>Stop square.</summary>
    public const string Stop =
        "<rect x=\"6\" y=\"6\" width=\"12\" height=\"12\" rx=\"1.5\"/>";

    /// <summary>Restart / refresh.</summary>
    public const string Restart =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M4.75 4.75V9H9\"/><path d=\"M4.75 9a8 8 0 1 1 1.6 7.55\"/>" +
        "</g>";

    /// <summary>Cast / screen-share.</summary>
    public const string Cast =
        "<path d=\"M2 16.1A5 5 0 0 1 5.9 20M2 12.05A9 9 0 0 1 9.95 20M2 8V6a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-6M2 20h.01\"/>";

    /// <summary>Delete / trash.</summary>
    public const string Delete =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M4.75 7.25h14.5M9.25 7.25v-2.5h5.5v2.5M6.75 7.25l.75 12h9l.75-12M10 11v4.75M14 11v4.75\"/>" +
        "</g>";

    /// <summary>Save / floppy disk.</summary>
    public const string Save =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M4.75 3.75h11.5l3 3v13.5H4.75zM8 3.75v6h8v-5M8 20.25v-6h8v6\"/>" +
        "</g>";

    /// <summary>Broken link / unlink.</summary>
    public const string Unlink =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"m9.5 14.5-1.75 1.75a3.18 3.18 0 0 1-4.5-4.5L5 10M14.5 9.5l1.75-1.75a3.18 3.18 0 0 1 4.5 4.5L19 14M9.75 12h4.5M4 4l16 16\"/>" +
        "</g>";

    /// <summary>Phone / device.</summary>
    public const string Phone =
        "<rect x=\"7\" y=\"2\" width=\"10\" height=\"20\" rx=\"2\"/><path d=\"M11 18h2\"/>";

    /// <summary>Dashboard grid.</summary>
    public const string Dashboard =
        "<rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"3\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/>";

    /// <summary>Pending / clock list.</summary>
    public const string Pending =
        "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/>";

    /// <summary>Admin / shield-settings.</summary>
    public const string Admin =
        "<path d=\"M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z\"/><path d=\"m9 12 2 2 4-4\"/>";

    /// <summary>Category / layers.</summary>
    public const string Category =
        "<path d=\"m12 2 9 5-9 5-9-5 9-5ZM3 12l9 5 9-5M3 17l9 5 9-5\"/>";

    /// <summary>Storage / database disk.</summary>
    public const string Storage =
        "<ellipse cx=\"12\" cy=\"5\" rx=\"8\" ry=\"3\"/><path d=\"M4 5v6c0 1.7 3.6 3 8 3s8-1.3 8-3V5M4 11v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6\"/>";

    /// <summary>Data usage / pie chart.</summary>
    public const string DataUsage =
        "<path d=\"M12 2a10 10 0 1 0 10 10h-10V2Z\"/><circle cx=\"12\" cy=\"12\" r=\"10\"/>";
}
