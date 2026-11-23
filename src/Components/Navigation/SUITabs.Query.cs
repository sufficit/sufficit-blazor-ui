using System;
using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Accessible tab list; this part keeps the active tab in the URL query (<see cref="QueryKey"/>).</summary>
public partial class SUITabs
{
    /// <summary>When set, the active tab is kept in this query parameter (e.g. "tab"), so a refresh or a shared link
    /// reopens the same tab. Values are the panels' <see cref="SUITabPanel.Key"/> (or their title slug).</summary>
    [Parameter]
    public string? QueryKey { get; set; }

    private string? QueryTab()
    {
        if (string.IsNullOrWhiteSpace(QueryKey)) return null;
        foreach (var pair in new Uri(Navigation.Uri).Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var cut = pair.IndexOf('=');
            var name = Uri.UnescapeDataString(cut < 0 ? pair : pair[..cut]);
            if (!string.Equals(name, QueryKey, StringComparison.OrdinalIgnoreCase)) continue;
            var value = cut < 0 ? string.Empty : Uri.UnescapeDataString(pair[(cut + 1)..].Replace('+', ' '));
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        return null;
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(QueryKey)) Navigation.LocationChanged += OnLocationChanged;
    }

    // Back/forward between tab URLs selects the matching panel without navigating again.
    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        if (_disposed || QueryTab() is not { } wanted) return;
        var index = _panels.FindIndex(x => x.Slug == wanted);
        if (index < 0 || index == ActiveIndex) return;
        ActiveIndex = index;
        _ = InvokeAsync(async () => { await ActiveIndexChanged.InvokeAsync(index); StateHasChanged(); });
    }
}
