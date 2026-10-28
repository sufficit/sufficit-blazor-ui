using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Shared click/icon plumbing for <see cref="SUIButton"/> and <see cref="SUIIconButton"/>.
/// </summary>
internal static class SUIClickLoading
{
    /// <summary>
    /// Milliseconds a pending click handler must run before the automatic
    /// spinner appears. Below this the spinner is a flash, not feedback: a nav
    /// toggle whose handler completes in a frame showed it on every click.
    /// </summary>
    internal const int DefaultDelay = 300;

    /// <summary>
    /// Awaits <paramref name="work"/>, raising the busy state only when it is
    /// still pending after <paramref name="delay"/> ms (never when negative).
    /// </summary>
    internal static async Task RunAsync(Task work, int delay, Func<bool, Task> setBusy)
    {
        if (work.IsCompleted || delay < 0)
        {
            await work;
            return;
        }

        if (delay > 0 && await Task.WhenAny(work, Task.Delay(delay)) == work)
        {
            await work;
            return;
        }

        await setBusy(true);
        try
        {
            await work;
        }
        finally
        {
            await setBusy(false);
        }
    }

    /// <summary>
    /// Renders an icon parameter: validated SVG markup (the <see cref="SUIIcons"/>
    /// constants) as before, or an <see cref="SUIIcon"/> name such as "trash".
    /// </summary>
    internal static RenderFragment Glyph(string? icon) => builder => BuildGlyph(builder, icon);

    private static void BuildGlyph(RenderTreeBuilder builder, string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
            return;

        if (SUIIconMarkup.IsMarkup(icon))
        {
            builder.OpenElement(0, "svg");
            builder.AddAttribute(1, "viewBox", "0 0 24 24");
            builder.AddAttribute(2, "fill", "currentColor");
            builder.AddAttribute(3, "aria-hidden", "true");
            builder.AddContent(4, SUIIconMarkup.Render(icon));
            builder.CloseElement();
            return;
        }

        builder.OpenComponent<SUIIcon>(5);
        builder.AddComponentParameter(6, nameof(SUIIcon.Name), icon.Trim());
        builder.CloseComponent();
    }
}
