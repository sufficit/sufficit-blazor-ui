namespace Sufficit.Blazor.UI.Components;

/// <summary>Immutable zero-based query passed to a server-backed table loader.</summary>
public sealed record SUITableQuery(int PageIndex, int PageSize, string? SortKey, SUISortDirection SortDirection);

/// <summary>Authorized page of rows and total matching record count.</summary>
public sealed record SUITablePage<TItem>
{
    /// <summary>Rows in this page, already filtered and sorted by the data owner.</summary>
    public IEnumerable<TItem> Items { get; init; } = Array.Empty<TItem>();
    /// <summary>Total number of matching records, independent of current page size.</summary>
    public int TotalItems { get; init; }
}
