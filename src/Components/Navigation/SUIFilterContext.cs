namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Filter state flowing from a <see cref="SUIFilterScope"/> to every
/// <see cref="SUIFilterTree"/> beneath it.
///
/// A dedicated type rather than a cascaded string with a magic name: typed
/// cascading cannot collide with an unrelated string cascade a consumer already
/// has, and the compiler catches a missing scope instead of the filter silently
/// never activating.
/// </summary>
/// <param name="FilterText">Current filter text; null or blank means no filter.</param>
public sealed record SUIFilterContext(string? FilterText)
{
    /// <summary>Whether a filter is active. Blank text counts as inactive.</summary>
    public bool IsFiltering => !string.IsNullOrWhiteSpace(FilterText);
}
