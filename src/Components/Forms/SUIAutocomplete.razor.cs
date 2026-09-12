using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Combobox with a debounced asynchronous search: typing calls <see cref="SearchFunc"/> or <see cref="SearchFuncAsync"/> and the results open in a top-layer listbox navigable with the arrow keys, Home/End, Enter and Escape.</summary>
/// <typeparam name="T">Item type returned by the search.</typeparam>
public partial class SUIAutocomplete<T>
{
    [CascadingParameter] private Microsoft.AspNetCore.Components.Forms.EditContext? FormContext { get; set; }
    private readonly SUIFieldBinding<T?> _field = new();
    /// <summary>Field expression used to bind validation messages from the surrounding <c>EditContext</c>.</summary>
    [Parameter] public System.Linq.Expressions.Expression<Func<T?>>? ValueExpression { get; set; }
    private string? EffectiveErrorText => ErrorText ?? _field.Error;

    /// <summary>Selected item, or default when nothing is chosen.</summary>
    [Parameter]
    public T? Value { get; set; }

    /// <summary>Raised on selection, on clear and when the text is emptied; enables <c>@bind-Value</c>.</summary>
    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }

    /// <summary>Search callback without cancellation. Ignored when <see cref="SearchFuncAsync"/> is set.</summary>
    [Parameter]
    public Func<string, Task<IEnumerable<T>>>? SearchFunc { get; set; }

    /// <summary>Search callback whose token is cancelled when the user keeps typing; preferred over <see cref="SearchFunc"/>.</summary>
    [Parameter]
    public Func<string, CancellationToken, Task<IEnumerable<T>>>? SearchFuncAsync { get; set; }

    /// <summary>Formats an item for the input text and the default option text. Falls back to <c>ToString()</c>.</summary>
    [Parameter]
    public Func<T, string>? ToStringFunc { get; set; }

    /// <summary>Visible label; also names the input and the listbox.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Placeholder of the text input.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Helper text rendered under the field and linked through <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>Explicit error message; takes precedence over validation messages from the form.</summary>
    [Parameter]
    public string? ErrorText { get; set; }

    /// <summary>Forces the invalid state (<c>aria-invalid</c>) without an error message.</summary>
    [Parameter]
    public bool Invalid { get; set; }

    /// <summary>Id of the input; generated when omitted.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>Extra element ids prepended to the input's <c>aria-describedby</c>.</summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>Optional SUI icon rendered inside the input at the inline end.</summary>
    [Parameter]
    public string? AdornmentIcon { get; set; }

    /// <summary>Minimum query length before a search runs. Default 0.</summary>
    [Parameter]
    public int MinCharacters { get; set; }

    /// <summary>Maximum number of results shown. Default 20.</summary>
    [Parameter]
    public int MaxItems { get; set; } = 20;

    /// <summary>Milliseconds to wait after the last keystroke before searching. Default 300.</summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    /// <summary>Disables the input, cancels a pending search and closes the list.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Shows a clear button while the input has text or a value.</summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>Accessible label and tooltip of the clear button.</summary>
    [Parameter]
    public string ClearText { get; set; } = "Limpar seleção";

    /// <summary>Text listed and announced while a search is running.</summary>
    [Parameter]
    public string LoadingText { get; set; } = "Carregando resultados…";

    /// <summary>Text shown when the search returns nothing; overridden by <see cref="NoItemsTemplate"/>.</summary>
    [Parameter]
    public string NoItemsText { get; set; } = "Nenhum resultado encontrado.";

    /// <summary>Text shown when the search callback throws.</summary>
    [Parameter]
    public string SearchErrorText { get; set; } = "Não foi possível carregar os resultados.";

    /// <summary>Accepted for markup compatibility; the component does not render nested content.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Custom markup for each option; defaults to the <see cref="ToStringFunc"/> text.</summary>
    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    /// <summary>Custom markup for the empty-result row.</summary>
    [Parameter]
    public RenderFragment? NoItemsTemplate { get; set; }

    /// <summary>Additional CSS class for the root element.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Unmatched attributes forwarded to the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    private readonly string _generatedId = $"sui-autocomplete-{Guid.NewGuid():N}";
    private readonly List<T> _items = new();
    private string _query = string.Empty;
    private string? _searchError;
    private T? _observedValue;
    private CancellationTokenSource? _cts;
    private bool _valueInitialized;
    private bool _open;
    private bool _loading;
    private bool _disposed;
    private int _focusVersion;
    private int _activeIndex = -1;

    private string EffectiveId => string.IsNullOrWhiteSpace(Id) ? _generatedId : Id.Trim();
    private string LabelId => $"{EffectiveId}-label";
    private string HelperId => $"{EffectiveId}-helper";
    private string ErrorId => $"{EffectiveId}-error";
    private string StatusId => $"{EffectiveId}-status";
    private string ListId => $"{EffectiveId}-listbox";
    private string? LabelledBy => string.IsNullOrWhiteSpace(Label) ? null : LabelId;
    private string? ErrorMessageId => !string.IsNullOrWhiteSpace(EffectiveErrorText) ? ErrorId : null;
    private string? ActiveDescendantId
        => _open && _activeIndex >= 0 && _activeIndex < _items.Count
            ? OptionId(_activeIndex)
            : null;
    private bool CanClear
        => Clearable && !Disabled && (!string.IsNullOrEmpty(_query) || Value is not null);
    private string? DescribedBy
    {
        get
        {
            var ids = new[]
            {
                AriaDescribedBy,
                string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
                !string.IsNullOrWhiteSpace(EffectiveErrorText) ? ErrorId : null,
                StatusId
            };
            return string.Join(" ", ids.Where(id => !string.IsNullOrWhiteSpace(id)));
        }
    }

    private string StatusMessage
        => _loading
            ? LoadingText
            : !string.IsNullOrWhiteSpace(_searchError)
                ? _searchError
                : _open
                    ? _items.Count == 0
                        ? NoItemsText
                        : $"{_items.Count} resultado{(_items.Count == 1 ? string.Empty : "s")} disponível{(_items.Count == 1 ? string.Empty : "is")}."
                    : string.Empty;

    private string Classname
        => SUIClassBuilder.Default("sui-field sui-autocomplete")
            .AddClass(Class)
            .AddClass(Invalid ? "sui-field--invalid" : null)
            .AddClass(_open ? "sui-autocomplete--open" : null)
            .Build();

    /// <summary>Wires form validation, mirrors an externally changed <see cref="Value"/> into the input text and closes the list when disabled.</summary>
    protected override void OnParametersSet()
    {
        _field.Configure(FormContext, ValueExpression, () => _ = InvokeAsync(StateHasChanged));
        if (!_valueInitialized || !EqualityComparer<T?>.Default.Equals(Value, _observedValue))
        {
            _observedValue = Value;
            _query = Value is null ? string.Empty : ToDisplayString(Value);
            _valueInitialized = true;
        }

        if (Disabled)
        {
            CancelPendingSearch();
            _loading = false;
            _open = false;
        }
    }

    private async Task OnInputAsync(ChangeEventArgs args)
    {
        _query = args.Value?.ToString() ?? string.Empty;
        CancelPendingSearch();
        _items.Clear();
        _activeIndex = -1;
        _searchError = null;

        if (string.IsNullOrEmpty(_query) && !EqualityComparer<T?>.Default.Equals(_observedValue, default))
        {
            _observedValue = default;
            await ValueChanged.InvokeAsync(default);
        _field.Notify();
        }

        if (Disabled || (SearchFunc is null && SearchFuncAsync is null) || _query.Length < Math.Max(0, MinCharacters))
        {
            _loading = false;
            _open = false;
            return;
        }

        var searchCts = new CancellationTokenSource();
        _cts = searchCts;
        var token = searchCts.Token;
        var query = _query;
        _loading = true;
        _open = true;
        StateHasChanged();

        try
        {
            await Task.Delay(Math.Max(0, DebounceInterval), token);
            var results = SearchFuncAsync is not null
                ? await SearchFuncAsync(query, token)
                : await SearchFunc!(query);
            token.ThrowIfCancellationRequested();

            _items.AddRange(results.Take(Math.Max(0, MaxItems)));
            _activeIndex = _items.Count > 0 ? 0 : -1;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return;
        }
        catch
        {
            if (!token.IsCancellationRequested)
            {
                _searchError = SearchErrorText;
            }
        }
        finally
        {
            if (ReferenceEquals(_cts, searchCts))
            {
                _loading = false;
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    private void OnFocus()
    {
        _focusVersion++;
        if (!Disabled && (_items.Count > 0 || _loading || !string.IsNullOrWhiteSpace(_searchError)))
        {
            _open = true;
        }
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled || args.CtrlKey || args.MetaKey || args.AltKey)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                if (_items.Count > 0)
                {
                    _open = true;
                    _activeIndex = (_activeIndex + 1 + _items.Count) % _items.Count;
                }
                break;
            case "ArrowUp":
                if (_items.Count > 0)
                {
                    _open = true;
                    _activeIndex = (_activeIndex - 1 + _items.Count) % _items.Count;
                }
                break;
            case "Home":
                if (_items.Count > 0)
                {
                    _open = true;
                    _activeIndex = 0;
                }
                break;
            case "End":
                if (_items.Count > 0)
                {
                    _open = true;
                    _activeIndex = _items.Count - 1;
                }
                break;
            case "Enter":
                if (_open && _activeIndex >= 0 && _activeIndex < _items.Count)
                {
                    await SelectAsync(_items[_activeIndex]);
                }
                break;
            case "Escape":
                _open = false;
                _activeIndex = -1;
                break;
        }
    }

    private async Task OnFocusOutAsync(FocusEventArgs _)
    {
        var version = ++_focusVersion;
        await Task.Delay(120);
        if (_disposed || version != _focusVersion)
        {
            return;
        }

        _open = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectAsync(T item)
    {
        if (Disabled) return;
        CancelPendingSearch();
        _query = ToDisplayString(item);
        _observedValue = item;
        _open = false;
        _activeIndex = -1;
        await ValueChanged.InvokeAsync(item);
        _field.Notify();
    }

    private async Task ClearAsync()
    {
        CancelPendingSearch();
        _query = string.Empty;
        _items.Clear();
        _observedValue = default;
        _loading = false;
        _open = false;
        _activeIndex = -1;
        _searchError = null;
        await ValueChanged.InvokeAsync(default);
        _field.Notify();
        await _inputElement.FocusAsync(preventScroll: true);
    }

    private string ToDisplayString(T item)
        => ToStringFunc?.Invoke(item) ?? item?.ToString() ?? string.Empty;

    private string OptionClass(int index)
        => index == _activeIndex
            ? "sui-autocomplete__option sui-autocomplete__option--active"
            : "sui-autocomplete__option";

    private string OptionId(int index) => $"{EffectiveId}-option-{index}";

    private void CancelPendingSearch()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>Releases the form binding, cancels a pending search and disconnects the browser module.</summary>
    public async ValueTask DisposeAsync()
    {
        _field.Dispose();
        _disposed = true;
        CancelPendingSearch();
        await DisconnectAsync();
    }
}
