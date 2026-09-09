using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

public partial class SUIAutocomplete<T>
{
    [CascadingParameter] private Microsoft.AspNetCore.Components.Forms.EditContext? FormContext { get; set; }
    private readonly SUIFieldBinding<T?> _field = new();
    [Parameter] public System.Linq.Expressions.Expression<Func<T?>>? ValueExpression { get; set; }
    private string? EffectiveErrorText => ErrorText ?? _field.Error;

    [Parameter]
    public T? Value { get; set; }

    [Parameter]
    public EventCallback<T?> ValueChanged { get; set; }

    [Parameter]
    public Func<string, Task<IEnumerable<T>>>? SearchFunc { get; set; }

    [Parameter]
    public Func<string, CancellationToken, Task<IEnumerable<T>>>? SearchFuncAsync { get; set; }

    [Parameter]
    public Func<T, string>? ToStringFunc { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [Parameter]
    public bool Invalid { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter]
    public string? AdornmentIcon { get; set; }

    [Parameter]
    public int MinCharacters { get; set; }

    [Parameter]
    public int MaxItems { get; set; } = 20;

    [Parameter]
    public int DebounceInterval { get; set; } = 300;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public string ClearText { get; set; } = "Limpar seleção";

    [Parameter]
    public string LoadingText { get; set; } = "Carregando resultados…";

    [Parameter]
    public string NoItemsText { get; set; } = "Nenhum resultado encontrado.";

    [Parameter]
    public string SearchErrorText { get; set; } = "Não foi possível carregar os resultados.";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment? NoItemsTemplate { get; set; }

    [Parameter]
    public string? Class { get; set; }

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

    public async ValueTask DisposeAsync()
    {
        _field.Dispose();
        _disposed = true;
        CancelPendingSearch();
        await DisconnectAsync();
    }
}
