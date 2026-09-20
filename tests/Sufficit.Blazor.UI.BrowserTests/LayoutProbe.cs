using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

/// <summary>
/// Reads an element's layout box, retrying until it is real.
/// </summary>
/// <remarks>
/// Visibility and layout are not the same instant. These pages are rendered by
/// an interactive Blazor circuit, so an element can be visible, then be replaced
/// when the circuit connects and the component renders again; a handle taken
/// before that swap is detached, and <c>BoundingBoxAsync</c> answers null for
/// it. A single read after a visibility assertion therefore passes on most
/// engines and fails on whichever one happens to be slowest that day — in this
/// repository, WebKit, where the field-actions alignment test failed a run that
/// passed on Chromium and Firefox and then passed on re-run with no code change.
///
/// Because every browser job gates publishing, that intermittent read was not a
/// test annoyance: it was a release blocker that appeared at random. Polling
/// until the box is non-null and has area removes the race without weakening the
/// assertion that follows, which still compares real coordinates.
/// </remarks>
internal static class LayoutProbe
{
    /// <summary>
    /// Returns the element's bounding box once it has area, or fails the test.
    /// </summary>
    /// <param name="locator">Element to measure.</param>
    /// <param name="description">Name used in the failure message.</param>
    /// <param name="timeoutMilliseconds">How long to keep re-reading.</param>
    public static async Task<LocatorBoundingBoxResult> RequireBoundingBoxAsync(
        this ILocator locator,
        string description,
        int timeoutMilliseconds = 5_000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        LocatorBoundingBoxResult? box = null;

        while (DateTime.UtcNow < deadline)
        {
            box = await locator.BoundingBoxAsync();
            if (box is { Width: > 0, Height: > 0 }) return box;
            await Task.Delay(100);
        }

        Assert.Fail($"{description} reported no layout box within {timeoutMilliseconds} ms "
            + $"(last read: {(box is null ? "null" : $"{box.Width}x{box.Height}")}).");
        return box!;
    }
}
