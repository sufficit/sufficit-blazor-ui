namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Inline SVG path markup (the inner content of an <c>&lt;svg&gt;</c>)
/// for the icons used as defaults by the SUI components. Each constant is a
/// ready-to-render <c>&lt;path&gt;</c> (or set of paths) drawn on a 24x24
/// viewBox with <c>fill="currentColor"</c>.
/// </summary>
public static class SUIIcons
{
    /// <summary>Home / application entry.</summary>
    public const string Home =
        "<path d=\"m3 11 9-8 9 8M5 10v11h14V10M9 21v-7h6v7\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    /// <summary>Magnifier with a slash — "no results". Replaces SearchOff.</summary>
    public const string SearchOff =
        "<path d=\"M15.5 14h-.79l-.28-.27a6.5 6.5 0 0 0 1.48-5.34c-.47-2.78-2.79-5-5.59-5.34a6.505 6.505 0 0 0-7.27 7.27c.34 2.8 2.56 5.12 5.34 5.59a6.5 6.5 0 0 0 5.34-1.48l.27.28v.79l4.25 4.25c.41.41 1.08.41 1.49 0 .41-.41.41-1.08 0-1.49L15.5 14zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z\"/>" +
        "<path d=\"M2.7 2.7a.996.996 0 0 0-1.41 0c-.39.39-.39 1.02 0 1.41l16.31 16.32c.39.39 1.02.39 1.41 0s.39-1.02 0-1.41L2.7 2.7z\"/>";

    /// <summary>Down-pointing chevron used by expandable navigation groups.</summary>
    public const string ArrowDropDown =
        "<path d=\"m6 9 6 6 6-6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

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

    /// <summary>Edit / pencil.</summary>
    public const string Edit =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<path d=\"M13.5 5.5 18.5 10.5M4 20l4.25-1 10.5-10.5a2.12 2.12 0 0 0-3-3L5.25 16 4 20Z\"/>" +
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

    /// <summary>Clock used for history, pending state and time-based activity.</summary>
    public const string Clock =
        "<circle cx=\"12\" cy=\"12\" r=\"9\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>" +
        "<path d=\"M12 7v5l3 2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    /// <summary>Pending state uses the shared clock glyph.</summary>
    public const string Pending = Clock;

    /// <summary>Admin / shield-settings.</summary>
    public const string Admin =
        "<path d=\"M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z\"/><path d=\"m9 12 2 2 4-4\"/>";

    /// <summary>Category / layers.</summary>
    public const string Category =
        "<path d=\"m12 2 9 5-9 5-9-5 9-5ZM3 12l9 5 9-5M3 17l9 5 9-5\"/>";

    /// <summary>Connected workflow used by services and recurring operations.</summary>
    public const string Workflow =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<rect x=\"3\" y=\"3\" width=\"6\" height=\"6\" rx=\"1.5\"/><rect x=\"15\" y=\"15\" width=\"6\" height=\"6\" rx=\"1.5\"/>" +
        "<path d=\"M9 6h4a4 4 0 0 1 4 4v5M15 18h-4a4 4 0 0 1-4-4V9\"/>" +
        "</g>";

    /// <summary>Storage / database disk.</summary>
    public const string Storage =
        "<g fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\">" +
        "<ellipse cx=\"12\" cy=\"5\" rx=\"8\" ry=\"3\"/><path d=\"M4 5v6c0 1.7 3.6 3 8 3s8-1.3 8-3V5M4 11v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6\"/>" +
        "</g>";

    /// <summary>Data usage / pie chart.</summary>
    public const string DataUsage =
        "<path d=\"M12 2a10 10 0 1 0 10 10h-10V2Z\"/><circle cx=\"12\" cy=\"12\" r=\"10\"/>";

    public const string Search =
        "<circle cx=\"11\" cy=\"11\" r=\"7\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"/><path d=\"m20 20-4-4\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"/>";

    public const string Refresh = Restart;

    public const string ChevronLeft =
        "<path d=\"m15 18-6-6 6-6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string ChevronRight =
        "<path d=\"m9 18 6-6-6-6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string Send =
        "<path d=\"m3 3 18 9-18 9 4-9-4-9Zm4 9h14\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string Filter =
        "<path d=\"M4 5h16l-6 7v5l-4 2v-7L4 5Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linejoin=\"round\"/>";

    public const string Copy =
        "<rect x=\"8\" y=\"8\" width=\"11\" height=\"11\" rx=\"2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M16 8V5a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h3\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>";

    public const string Link =
        "<path d=\"M10 13a5 5 0 0 0 7.5.5l2-2a5 5 0 0 0-7-7l-1.1 1.1M14 11a5 5 0 0 0-7.5-.5l-2 2a5 5 0 0 0 7 7l1.1-1.1\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>";

    public const string External =
        "<path d=\"M15 3h6v6M10 14 21 3M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string Close =
        "<path d=\"m18 6-12 12M6 6l12 12\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/>";

    public const string Warning =
        "<path d=\"M10.3 2.9 1.8 17a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 2.9a2 2 0 0 0-3.4 0Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M12 9v4M12 17h.01\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"/>";

    public const string Receipt =
        "<path d=\"M5 3h14a2 2 0 0 1 2 2v16l-3-1.5-3 1.5-3-1.5L9 21l-3-1.5L3 21V5a2 2 0 0 1 2-2Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M7 8h10M7 12h10M7 16h5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>";

    /// <summary>Bank building used for banking and settlement areas.</summary>
    public const string Bank =
        "<path d=\"m3 9 9-6 9 6M5 10h14M6 10v8M10 10v8M14 10v8M18 10v8M3 21h18\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>";

    /// <summary>Barcode used for boleto identification and reconciliation.</summary>
    public const string Barcode =
        "<path d=\"M5 6v12M8 6v12M11 6v12M14 6v12M17 6v12M19 6v12\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>";

    /// <summary>Wallet used for a customer's own financial documents.</summary>
    public const string Wallet =
        "<path d=\"M4 6h14a2 2 0 0 1 2 2v10H4a2 2 0 0 1-2-2V6a3 3 0 0 1 3-3h12v3M15 11h7v4h-7a2 2 0 0 1 0-4Z\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>";

    public const string Tune =
        "<path d=\"M4 6h10M18 6h2M4 12h2M10 12h10M4 18h7M15 18h5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><circle cx=\"16\" cy=\"6\" r=\"2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><circle cx=\"8\" cy=\"12\" r=\"2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><circle cx=\"13\" cy=\"18\" r=\"2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>";

    public const string Pdf =
        "<path d=\"M6 2h8l4 4v16H6zM14 2v5h5\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M8 13h3a2 2 0 0 1 0 4H8v-4Zm7 0v4M15 13h3\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.5\"/>";

    public const string Code =
        "<path d=\"m8 9-3 3 3 3M16 9l3 3-3 3M14 5l-4 14\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string Login =
        "<path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>";

    public const string Payments =
        "<rect x=\"3\" y=\"6\" width=\"18\" height=\"12\" rx=\"2\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/><path d=\"M3 10h18M7 14h4\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\"/>";
}
