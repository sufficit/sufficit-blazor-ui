using Microsoft.AspNetCore.Components.Web;

namespace Sufficit.Blazor.UI.Components;

/// <summary>A request to open at a viewport point, or below an anchor element for keyboard activation.</summary>
public sealed record SUIContextMenuRequest(string? AnchorId = null, double? X = null, double? Y = null)
{
    /// <summary>Preserves pointer coordinates and anchors an activation without a pointer position.</summary>
    public static SUIContextMenuRequest FromMouse(string? anchorId, MouseEventArgs args) =>
        args.ClientX != 0 || args.ClientY != 0 || args.Button == 2 || args.Detail > 0
            ? new(anchorId, args.ClientX, args.ClientY)
            : new(anchorId);

    /// <summary>Recognizes the standard menu key and its keyboard alternative.</summary>
    public static bool IsKeyboardShortcut(KeyboardEventArgs args) =>
        args.Key == "ContextMenu" || args.Key == "F10" && args.ShiftKey;
}
