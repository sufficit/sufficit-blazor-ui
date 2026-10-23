using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Services;

/// <summary>Application-wide default for <see cref="SUIAnimation"/>; registered by <c>AddSufficitUI</c>.</summary>
public sealed class SUIAnimationPolicy(SUIAnimation animations = SUIAnimation.Default)
{
    /// <summary>Animations enabled by default for every SUI component.</summary>
    public SUIAnimation Animations { get; } = animations;

    /// <summary>Component parameter wins; otherwise the registered policy; otherwise <see cref="SUIAnimation.Default"/>.</summary>
    public static SUIAnimation Resolve(SUIAnimation? local, SUIAnimationPolicy? policy) => local ?? policy?.Animations ?? SUIAnimation.Default;
}
