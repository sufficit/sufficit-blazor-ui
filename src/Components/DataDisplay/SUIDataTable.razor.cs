using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Controlled local/server table with cancellation, stable query keys and SUI pagination.</summary>
public partial class SUIDataTable<TItem>
{
    private IReadOnlyList<TItem> _rows = Array.Empty<TItem>();
    private CancellationTokenSource? _pending;
    private long _generation;
    private bool _rendered, _disposed, _loading, _hasResult, _wasServer;
    private Exception? _error;
    private (int Index, int Size, string? Sort, SUISortDirection Direction, string? Key)? _lastQuery;

    /// <summary>Local records when no asynchronous loader is configured.</summary>
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    /// <summary>Optional server loader; receives an immutable query and a canceled-on-replacement token.</summary>
    [Parameter] public Func<SUITableQuery, CancellationToken, Task<SUITablePage<TItem>>>? LoadPage { get; set; }
    /// <summary>Stable scope/filter identity. A changed key clears prior rows before requesting another scope.</summary>
    [Parameter] public string? QueryKey { get; set; }
    /// <summary>Zero-based selected page.</summary>
    [Parameter] public int PageIndex { get; set; }
    /// <summary>Reports page changes so the application can persist them in its URL.</summary>
    [Parameter] public EventCallback<int> PageIndexChanged { get; set; }
    /// <summary>Number of records per page when pagination is visible.</summary>
    [Parameter] public int PageSize { get; set; } = 25;
    /// <summary>Reports page size changes.</summary>
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }
    /// <summary>Owner-defined server sort key.</summary>
    [Parameter] public string? SortKey { get; set; }
    /// <summary>Sort direction sent to the server; local sorting remains an explicit owner projection.</summary>
    [Parameter] public SUISortDirection SortDirection { get; set; }
    /// <summary>Whether to show the shared pagination controls.</summary>
    [Parameter] public bool ShowPagination { get; set; }
    /// <summary>Allowed page sizes.</summary>
    [Parameter] public IReadOnlyList<int>? PageSizeOptions { get; set; }
    /// <summary>Stable row identity.</summary>
    [Parameter] public Func<TItem, object>? RowKey { get; set; }
    /// <summary>Compact row presentation.</summary>
    [Parameter] public bool Dense { get; set; }
    /// <summary>Highlights a hovered row.</summary>
    [Parameter] public bool Hover { get; set; }
    /// <summary>Number of visible columns for empty/loading content.</summary>
    [Parameter] public int ColumnCount { get; set; } = 1;
    /// <summary>Owner-defined row classes.</summary>
    [Parameter] public Func<TItem, int, string?>? RowClassFunc { get; set; }
    /// <summary>Optional owner-defined row styles.</summary>
    [Parameter] public Func<TItem, int, string?>? RowStyleFunc { get; set; }
    /// <summary>Keyboard/pointer row activation, rendered by SUITable's grid interaction.</summary>
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
    /// <summary>Accessible label for an actionable row.</summary>
    [Parameter] public Func<TItem, string?>? RowAriaLabelFunc { get; set; }
    /// <summary>Stacks labeled cells on narrow screens.</summary>
    [Parameter] public bool StackOnMobile { get; set; }
    /// <summary>Column headings.</summary>
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    /// <summary>Content for each record.</summary>
    [Parameter] public RenderFragment<TItem>? RowTemplate { get; set; }
    /// <summary>Shown only after a completed empty query.</summary>
    [Parameter] public RenderFragment? NoRecordsContent { get; set; }
    /// <summary>Optional initial loading placeholder.</summary>
    [Parameter] public RenderFragment? LoadingContent { get; set; }
    /// <summary>Optional table footer cells.</summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }
    /// <summary>Localized initial loading label.</summary>
    [Parameter] public string LoadingText { get; set; } = "Carregando registros";
    /// <summary>Localized query failure message; exception details are never rendered.</summary>
    [Parameter] public string ErrorText { get; set; } = "Não foi possível carregar os registros.";
    /// <summary>Explains that preserved rows may be stale after an unsuccessful explicit refresh.</summary>
    [Parameter] public string StaleText { get; set; } = "A lista anterior foi mantida; consulte novamente antes de agir.";
    /// <summary>Localized retry action.</summary>
    [Parameter] public string RetryText { get; set; } = "Tentar novamente";
    /// <summary>Optional error reporter for authentication/logging owned by the application.</summary>
    [Parameter] public EventCallback<Exception> OnLoadError { get; set; }

    /// <summary>Total matching records from the last successful query or current local collection.</summary>
    public int TotalItems { get; private set; }
    private bool Pending => LoadPage != null && (_loading || !_hasResult && _error == null);
    private IEnumerable<TItem> VisibleItems => LoadPage != null ? _rows
        : ShowPagination ? (Items ?? []).Skip(PageIndex * PageSize).Take(PageSize) : Items ?? [];

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (PageSize < 1 || PageIndex < 0) throw new ArgumentOutOfRangeException(nameof(PageSize));
        var query = (PageIndex, PageSize, SortKey, SortDirection, QueryKey);
        if (LoadPage == null)
        {
            if (_wasServer)
            {
                _generation++; _pending?.Cancel(); _lastQuery = null;
                _rows = Array.Empty<TItem>(); _hasResult = false; _loading = false; _error = null;
            }
            _wasServer = false;
            TotalItems = Items?.Count() ?? 0;
            return;
        }
        _wasServer = true;
        if (_lastQuery == query) return;
        var changedScope = _lastQuery?.Key != QueryKey;
        _lastQuery = query;
        _rows = Array.Empty<TItem>(); _hasResult = false;
        if (changedScope) TotalItems = 0;
        if (_rendered) await ReloadAsync();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _rendered = true;
        if (LoadPage != null) await ReloadAsync();
    }

    /// <summary>Refreshes the current immutable query, canceling an older request.</summary>
    public async Task ReloadAsync()
    {
        if (_disposed || LoadPage == null) return;
        var generation = ++_generation;
        _pending?.Cancel();
        var request = new CancellationTokenSource();
        _pending = request;
        _loading = true; _error = null;
        await InvokeAsync(StateHasChanged);
        try
        {
            var page = await LoadPage(new(PageIndex, PageSize, SortKey, SortDirection), request.Token);
            if (_disposed || generation != _generation || request.IsCancellationRequested) return;
            _rows = page.Items.ToArray();
            TotalItems = Math.Max(0, page.TotalItems);
            _hasResult = true;
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested) { }
        catch (Exception error)
        {
            if (!_disposed && generation == _generation)
            {
                _error = error;
                await OnLoadError.InvokeAsync(error);
            }
        }
        finally
        {
            if (ReferenceEquals(_pending, request)) _pending = null;
            request.Dispose();
            if (!_disposed && generation == _generation)
            { _loading = false; await InvokeAsync(StateHasChanged); }
        }
    }

    private async Task ChangePage(int value)
    {
        PageIndex = value;
        await PageIndexChanged.InvokeAsync(value);
        await RefreshChangedQuery();
    }

    private async Task ChangePageSize(int value)
    {
        PageSize = value; PageIndex = 0;
        await PageSizeChanged.InvokeAsync(value);
        await PageIndexChanged.InvokeAsync(0);
        await RefreshChangedQuery();
    }

    private async Task RefreshChangedQuery()
    {
        var query = (PageIndex, PageSize, SortKey, SortDirection, QueryKey);
        if (_lastQuery == query) return;
        _lastQuery = query;
        _rows = Array.Empty<TItem>(); _hasResult = false;
        if (LoadPage != null) await ReloadAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _generation++;
        _pending?.Cancel();
    }
}
