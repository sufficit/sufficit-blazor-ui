namespace Sufficit.Blazor.UI.Themes;

/// <summary>
/// Shape, spacing, elevation and motion tokens. Components use these via the
/// generated CSS variables (<c>--sui-radius</c>, <c>--sui-space-2</c>,
/// <c>--sui-shadow-2</c>, etc.). A consumer can tighten the whole system by
/// overriding radius/density here without touching component markup.
/// </summary>
public sealed record SUILayout
{
    // shape
    /// <summary>Small corner radius (chips, inputs); feeds <c>--sui-radius-sm</c>. Default 4px.</summary>
    public string RadiusSm { get; init; } = "4px";
    /// <summary>Base corner radius (buttons, cards); feeds <c>--sui-radius</c>. Default 8px.</summary>
    public string Radius { get; init; } = "8px";
    /// <summary>Large corner radius (dialogs, panels); feeds <c>--sui-radius-lg</c>. Default 14px.</summary>
    public string RadiusLg { get; init; } = "14px";
    /// <summary>Pill/circular radius (avatars, badges); feeds <c>--sui-radius-full</c>. Default 9999px.</summary>
    public string RadiusFull { get; init; } = "9999px";

    // spacing
    /// <summary>Spacing step 1 of the 4px grid; feeds <c>--sui-space-1</c>. Default 4px.</summary>
    public string Space1 { get; init; } = "4px";
    /// <summary>Spacing step 2 of the 4px grid; feeds <c>--sui-space-2</c>. Default 8px.</summary>
    public string Space2 { get; init; } = "8px";
    /// <summary>Spacing step 3 of the 4px grid; feeds <c>--sui-space-3</c>. Default 12px.</summary>
    public string Space3 { get; init; } = "12px";
    /// <summary>Spacing step 4 of the 4px grid; feeds <c>--sui-space-4</c>. Default 16px.</summary>
    public string Space4 { get; init; } = "16px";
    /// <summary>Spacing step 5 of the 4px grid; feeds <c>--sui-space-5</c>. Default 24px.</summary>
    public string Space5 { get; init; } = "24px";
    /// <summary>Spacing step 6 of the 4px grid; feeds <c>--sui-space-6</c>. Default 32px.</summary>
    public string Space6 { get; init; } = "32px";

    // elevation
    /// <summary>Lowest elevation shadow (resting cards); feeds <c>--sui-shadow-1</c>.</summary>
    public string Shadow1 { get; init; } = "0 1px 2px rgba(15,23,42,.06)";
    /// <summary>Mid elevation shadow (hovered cards, menus); feeds <c>--sui-shadow-2</c>.</summary>
    public string Shadow2 { get; init; } = "0 4px 10px rgba(15,23,42,.08)";
    /// <summary>Highest elevation shadow (dialogs, popovers); feeds <c>--sui-shadow-3</c>.</summary>
    public string Shadow3 { get; init; } = "0 12px 28px rgba(15,23,42,.14)";

    // motion
    /// <summary>Default transition duration and easing for hover/focus changes; feeds <c>--sui-transition</c>. Default 160ms.</summary>
    public string Transition { get; init; } = "160ms cubic-bezier(.4, 0, .2, 1)";
    /// <summary>Slower transition for enter/exit motion (drawers, dialogs); feeds <c>--sui-transition-slow</c>. Default 280ms.</summary>
    public string TransitionSlow { get; init; } = "280ms cubic-bezier(.4, 0, .2, 1)";

    // control sizing
    /// <summary>Height of small controls (small buttons, checkboxes); feeds <c>--sui-control-h-sm</c>. Default 28px.</summary>
    public string ControlHSm { get; init; } = "28px";
    /// <summary>Height of medium (default) controls; feeds <c>--sui-control-h-md</c>. Default 36px.</summary>
    public string ControlHMd { get; init; } = "36px";
    /// <summary>Height of large controls; feeds <c>--sui-control-h-lg</c>. Default 44px.</summary>
    public string ControlHLg { get; init; } = "44px";
    /// <summary>Horizontal padding of small controls; feeds <c>--sui-control-px-sm</c>. Default 10px.</summary>
    public string ControlPxSm { get; init; } = "10px";
    /// <summary>Horizontal padding of medium (default) controls; feeds <c>--sui-control-px-md</c>. Default 14px.</summary>
    public string ControlPxMd { get; init; } = "14px";
    /// <summary>Horizontal padding of large controls; feeds <c>--sui-control-px-lg</c>. Default 18px.</summary>
    public string ControlPxLg { get; init; } = "18px";

    /// <summary>Default shape, spacing, elevation and motion tokens matching the original hardcoded SUI values.</summary>
    public static SUILayout Default { get; } = new();
}
