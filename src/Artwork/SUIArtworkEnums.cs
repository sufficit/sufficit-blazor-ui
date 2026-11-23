namespace Sufficit.Blazor.UI.Artwork;

/// <summary>
/// Generated pattern drawn by <see cref="SUIArtwork"/>. Each value is a
/// deterministic drawing computed from math, not an image file.
/// </summary>
public enum SUIArtworkPattern
{
    /// <summary>
    /// Field of signal waves (sums of sines): calm on one side, rising into a
    /// peak, with one accent lead line.
    /// </summary>
    SignalWaves,
}

/// <summary>
/// Where the artwork stays calm, so the host's text sits on quiet space.
/// </summary>
public enum SUIArtworkCalm
{
    /// <summary>Calm at the start (left); the drawing lives on the end side. For side-by-side heroes.</summary>
    Start,
    /// <summary>Calm at the top; the drawing lives at the bottom. For stacked layouts.</summary>
    Top,
    /// <summary>Start on wide screens (1200px and up), Top below that.</summary>
    Adaptive,
}
