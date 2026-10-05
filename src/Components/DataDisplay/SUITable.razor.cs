using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Data table: materialized rows by default, or a windowed body through
/// <see cref="Virtualize"/>. Clickable rows turn the table into an ARIA grid
/// with row focus (roving tabindex plus a keyboard module). Markup lives in
/// <c>SUITable.razor</c>; this partial holds the parameter surface and the
/// interop lifecycle.
/// </summary>
/// <typeparam name="TItem">Row model type.</typeparam>
public partial class SUITable<TItem>
{
    /// <summary>Stable identity retained when rows are sorted or paged.</summary>
    [Parameter] public Func<TItem, object>? RowKey { get; set; }

    /// <summary>
    /// Renders the body through <c>Virtualize</c>: only the visible window of
    /// rows exists in the DOM. For operational listings with thousands of rows
    /// under continuous scroll. The wrapper becomes the scroll container
    /// (a max height applies automatically); paginated navigation
    /// (<c>SUIPagination</c>) remains the recommended shape for discrete
    /// listings. Needs an interactive render mode — a statically rendered page
    /// shows only the initial rows.
    /// </summary>
    [Parameter] public bool Virtualize { get; set; }

    /// <summary>
    /// Estimated row height in pixels, used by <see cref="Virtualize"/> to
    /// translate scroll position into a row window. Measure a representative
    /// row (padding + line height + border): a wrong estimate causes jitter,
    /// not data loss. Ignored when <see cref="Virtualize"/> is false.
    /// </summary>
    [Parameter] public float ItemSize { get; set; } = 44f;

    /// <summary>
    /// Extra rows rendered before and after the visible window when
    /// <see cref="Virtualize"/> is on, so a fast scroll lands on already
    /// rendered rows.
    /// </summary>
    [Parameter] public int OverscanCount { get; set; } = 5;

    /// <summary>
    /// Accessible name of the scrollable region a virtualized read-only table
    /// becomes. A scroller that keyboard users cannot focus is unreachable
    /// (WCAG 2.1.1), so the wrapper joins the tab order and is named so a
    /// screen reader announces what it scrolls. Ignored unless
    /// <see cref="Virtualize"/> is on and <see cref="OnRowClick"/> is unset
    /// (interactive rows are already focusable).
    /// </summary>
    [Parameter] public string ScrollLabel { get; set; } = "Tabela com rolagem";

    /// <summary>
    /// Items to render. When null/empty, <see cref="NoRecordsContent"/> shows.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>Applies the hover background to data rows.</summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>Compacts cell padding for dense operational layouts.</summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// Presents each body row as a labelled card on compact screens. Cells should
    /// provide <see cref="SUITd.DataLabel"/> so their column name remains visible.
    /// Desktop table semantics and layout are unchanged.
    /// </summary>
    [Parameter]
    public bool StackOnMobile { get; set; }

    /// <summary>Shows an indeterminate progress indicator and exposes aria-busy.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>Optional class selector calculated for each rendered row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowClassFunc { get; set; }

    /// <summary>Optional inline style calculated for each rendered row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowStyleFunc { get; set; }

    /// <summary>
    /// Number of data columns, used by the no-records cell so its
    /// <c>colspan</c> covers the full table. Left unset (0) the cell spans a
    /// column count no table reaches, which is what a browser needs to make it
    /// fill the row: a short colspan pins the empty state to the first column.
    /// </summary>
    [Parameter]
    public int ColumnCount { get; set; }

    /// <summary>
    /// Called when a row is clicked or activated with Enter/Space (item passed).
    /// Setting it turns the table into a grid: one row is in the tab order and
    /// arrow keys, Home and End move between rows.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>
    /// Optional accessible name for an interactive row, e.g. "Abrir pedido 42".
    /// Without it the row is announced by its cell contents.
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? RowAriaLabelFunc { get; set; }

    /// <summary>Optional header row: rendered inside <c>thead</c>.</summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>Cell content for each rendered body row.</summary>
    [Parameter]
    public RenderFragment<TItem>? RowTemplate { get; set; }

    /// <summary>Content of the empty-state row; when null, an empty table shows no body row.</summary>
    [Parameter]
    public RenderFragment? NoRecordsContent { get; set; }

    /// <summary>Optional footer row: rendered inside <c>tfoot</c>.</summary>
    [Parameter]
    public RenderFragment? FooterContent { get; set; }


    private string WrapperClass
        => SUIClassBuilder.Default("sui-table-wrapper")
            .AddClass("sui-table-wrapper--stack-mobile", StackOnMobile)
            .AddClass("sui-table-wrapper--virtual", Virtualize)
            .Build();

    private string Classname
        => SUIClassBuilder.Default("sui-table")
            .AddClass("sui-table--hover", Hover)
            .AddClass("sui-table--dense", Dense)
            .AddClass("sui-table--stack-mobile", StackOnMobile)
            .AddClass(Class)
            .Build();

    // A virtualized table scrolls inside its wrapper. With interactive rows the
    // focusable rows already make the region reachable; a read-only one needs
    // the wrapper itself in the tab order.
    private bool ScrollRegion => Virtualize && !OnRowClick.HasDelegate;

    private Task OnRowItemClick(TItem item) => OnRowClick.InvokeAsync(item);

    private ElementReference _wrapperElement;
    private IJSObjectReference? _module;
    private IJSObjectReference? _interop;
    private bool _interopInitialized;
    // The row in the tab order (roving tabindex); follows focus.
    private int _activeRow;

    /// <summary>
    /// Loads the keyboard module on the first render of an interactive table.
    /// A read-only table never loads it.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Row keys live in the module: it sees the event target, so a key
        // pressed on a control inside a row never activates the row itself.
        // A read-only table never loads it. The flag flips before the first
        // await: a render that arrives while the import is still in flight
        // must not start a second concurrent initialize, or the keydown
        // listener is attached twice and one Enter activates the row (and
        // any OnRowClick side effect) more than once.
        if (_interopInitialized || !OnRowClick.HasDelegate)
            return;
        _interopInitialized = true;
        _module = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Sufficit.Blazor.UI/Components/DataDisplay/SUITable.razor.js");
        _interop = await _module.InvokeAsync<IJSObjectReference>("initialize", _wrapperElement);
    }

    /// <summary>Detaches the keyboard module and disposes the JS import.</summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_interop is not null)
                await _interop.InvokeVoidAsync("dispose");
            if (_module is not null)
                await _module.DisposeAsync();
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // The browser circuit may already be gone during disposal.
        }
    }

    // 1000 is the maximum colspan the HTML spec allows, so an unset ColumnCount
    // still spans every column of any real table instead of just the first one.
    private const int FullRowColumnSpan = 1000;

    private int EffectiveColumnCount => ColumnCount > 0 ? ColumnCount : FullRowColumnSpan;

    private string GetRowClass(TItem item, int index, bool interactive = false)
        => SUIClassBuilder.Default("sui-table__row")
            .AddClass("sui-table__row--interactive", interactive)
            .AddClass(RowClassFunc?.Invoke(item, index))
            .Build();

    // Virtualize<TItem> hands ItemContent the item alone: the original index
    // (row classes, tabindex, aria labels depend on it) travels as part of the
    // virtualized item itself, so duplicate values never misindex a row.
    private sealed record IndexedRow(TItem Item, int Index);
}
