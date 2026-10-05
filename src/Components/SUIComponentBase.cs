using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Shared, markup-free base for SUI components. Provides the Blazor-standard
/// Class/Style/AdditionalAttributes surface without duplicating parameter
/// declarations in every component. Splat the dictionary onto the intended
/// element: it is not automatically attached to the root, because form
/// components intentionally forward attributes to their input or trigger.
/// </summary>
public abstract class SUIComponentBase : ComponentBase
{
    /// <summary>Optional user CSS classes appended to component-owned classes.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Optional per-instance styles (consumer supplied).</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Unmatched HTML attributes. Components decide which element receives
    /// them via <c>@attributes="AdditionalAttributes"</c>.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> AdditionalAttributes { get; set; } = new();

    /// <summary>
    /// Build the component's CSS class list without hand-concatenating the
    /// consumer's <see cref="Class"/>. The component supplies its owned classes.
    /// </summary>
    protected string EffectiveClass(string owned)
        => SUIClassBuilder.Default(owned).AddClass(Class).Build();

    /// <summary>
    /// Merge the obsolete <c>UserAttributes</c> bag into the canonical bag.
    /// Blazor requires [Parameter] auto-properties (BL0007), so an obsolete
    /// alias cannot forward from its setter. Call this from OnParametersSet
    /// after Blazor has captured unmatched attributes into AdditionalAttributes.
    /// Canonical values win on collisions; caller dictionaries stay untouched.
    /// </summary>
    protected void MergeLegacyAttributes(Dictionary<string, object?> legacy)
    {
        if (legacy.Count == 0)
            return;

        var merged = new Dictionary<string, object?>(legacy);
        foreach (var (name, value) in AdditionalAttributes)
            merged[name] = value;
        AdditionalAttributes = merged;
    }
}
