using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

public partial class SUISelect<T>
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
    public bool Disabled { get; set; }

    /// <summary>
    /// Optional CSS width for the open menu. When omitted, the menu sizes itself
    /// to its content while preserving the trigger width as its minimum.
    /// </summary>
    [Parameter]
    public string? MenuWidth { get; set; }

    /// <summary>
    /// Optional CSS max-width for the open menu. The viewport guard always wins,
    /// so a menu cannot become wider than the available browser window.
    /// </summary>
    [Parameter]
    public string? MenuMaxWidth { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> UserAttributes { get; set; } = new();

    private readonly List<SUISelectItem> _items = new();
    private readonly Dictionary<SUISelectItem, int> _itemVersions = new();
    private readonly string _generatedId = $"sui-select-{Guid.NewGuid():N}";
    private ElementReference _triggerElement;
    private ElementReference _menuElement;
    private IJSObjectReference? _module;
    private bool _open;
    private int _focusVersion;
    private bool _interopOpen;
    private bool _keyboardInteropConnected;
    private int _activeIndex = -1;

    private string EffectiveId => string.IsNullOrWhiteSpace(Id) ? _generatedId : Id.Trim();
    private string LabelId => $"{EffectiveId}-label";
    private string HelperId => $"{EffectiveId}-helper";
    private string ErrorId => $"{EffectiveId}-error";
    private string MenuId => $"{EffectiveId}-menu";
    private string? LabelledBy => string.IsNullOrWhiteSpace(Label) ? null : LabelId;
    private string? ErrorMessageId => !string.IsNullOrWhiteSpace(EffectiveErrorText) ? ErrorId : null;
    private string? ActiveDescendantId
        => _open && _activeIndex >= 0 && _activeIndex < _items.Count
            ? OptionId(_activeIndex)
            : null;
    private string? DescribedBy
    {
        get
        {
            var ids = new[]
            {
                AriaDescribedBy,
                string.IsNullOrWhiteSpace(HelperText) ? null : HelperId,
                !string.IsNullOrWhiteSpace(EffectiveErrorText) ? ErrorId : null
            };
            var value = string.Join(" ", ids.Where(id => !string.IsNullOrWhiteSpace(id)));
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

    private bool ShowPlaceholder => !string.IsNullOrWhiteSpace(Placeholder);

    private SUISelectItem? SelectedItem
        => _items.FirstOrDefault(item => Equals(item.Value, Value))
            ?? (Value is null ? _items.FirstOrDefault(item => item.Selected) : null);

    private string Classname
        => SUIClassBuilder.Default("sui-field sui-select")
            .AddClass(Class)
            .AddClass(_open ? "sui-select--open" : null)
            .Build();

    private string MenuClass => _open
        ? "sui-select__menu sui-select__menu--open"
        : "sui-select__menu";

    private string? MenuStyle
    {
        get
        {
            var declarations = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(MenuWidth))
            {
                declarations.Add($"--sui-select-menu-width: {MenuWidth.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(MenuMaxWidth))
            {
                declarations.Add($"--sui-select-menu-max-width: {MenuMaxWidth.Trim()}");
            }

            return declarations.Count == 0 ? null : string.Join("; ", declarations);
        }
    }

    void ISUISelectRegistry.Register(SUISelectItem item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
            _itemVersions[item] = item.Version;
            _ = InvokeAsync(StateHasChanged);
            return;
        }

        if (_itemVersions.TryGetValue(item, out var version) && version == item.Version)
        {
            return;
        }

        _itemVersions[item] = item.Version;
        _ = InvokeAsync(StateHasChanged);
    }

    void ISUISelectRegistry.Unregister(SUISelectItem item)
    {
        if (_items.Remove(item))
        {
            _itemVersions.Remove(item);
            _ = InvokeAsync(StateHasChanged);
        }
    }

    protected override void OnParametersSet()
    {
        _field.Configure(FormContext, ValueExpression, () => _ = InvokeAsync(StateHasChanged));
        if (Disabled)
        {
            _open = false;
        }
    }

    private void Toggle()
    {
        if (Disabled)
        {
            return;
        }

        _open = !_open;
        if (_open)
        {
            _activeIndex = SelectedIndex();
        }
    }

    private async Task OnKeyDown(KeyboardEventArgs args)
    {
        if (Disabled || _items.Count == 0)
        {
            return;
        }

        switch (args.Key)
        {
            case "Escape":
                _open = false;
                break;
            case "ArrowDown":
                OpenAndMove(1);
                break;
            case "ArrowUp":
                OpenAndMove(-1);
                break;
            case "Home":
                _open = true;
                _activeIndex = FindEnabledIndex(0, 1);
                break;
            case "End":
                _open = true;
                _activeIndex = FindEnabledIndex(_items.Count - 1, -1);
                break;
            case "Enter":
            case " ":
                if (!_open)
                {
                    _open = true;
                    _activeIndex = SelectedIndex();
                }
                else if (_activeIndex >= 0)
                {
                    await SelectAsync(_items[_activeIndex]);
                }
                break;
        }
    }

    private void OpenAndMove(int direction)
    {
        if (!_open)
        {
            _open = true;
            var selected = SelectedItem;
            _activeIndex = selected is not null && !selected.Disabled
                ? _items.IndexOf(selected)
                : FindEnabledIndex(direction > 0 ? 0 : _items.Count - 1, direction);
            return;
        }

        _open = true;
        var current = _activeIndex >= 0 ? _activeIndex : SelectedIndex();
        var next = current + direction;
        if (current < 0)
        {
            next = direction > 0 ? 0 : _items.Count - 1;
        }

        _activeIndex = FindEnabledIndex(next, direction);
    }

    private async Task OnFocusOutAsync(FocusEventArgs _)
    {
        // Keep multiple close paths: toggle, option selection, Escape, and focus loss
        // cover pointer, keyboard, and browser-specific interaction scenarios.
        var focusVersion = ++_focusVersion;
        await Task.Delay(120);
        // A previous blur must not close a menu after focus has returned.
        if (!_open || focusVersion != _focusVersion)
        {
            return;
        }

        _open = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectAsync(SUISelectItem item)
    {
        if (item.Disabled)
        {
            return;
        }

        _open = false;
        _activeIndex = _items.IndexOf(item);

        if (TryConvertValue(item.Value, out var value))
        {
            await ValueChanged.InvokeAsync(value);
        _field.Notify();
        }
    }

    private int SelectedIndex()
    {
        var selected = SelectedItem;
        if (selected is not null && !selected.Disabled)
        {
            return _items.IndexOf(selected);
        }

        return FindEnabledIndex(0, 1);
    }

    private bool IsSelected(SUISelectItem item)
        => ReferenceEquals(item, SelectedItem);

    private string OptionClass(int index)
    {
        var item = _items[index];
        var classes = new List<string> { "sui-select__option" };
        if (index == _activeIndex)
        {
            classes.Add("sui-select__option--active");
        }

        if (item.Disabled)
        {
            classes.Add("sui-select__option--disabled");
        }

        return string.Join(" ", classes);
    }

    private string OptionId(int index) => $"{EffectiveId}-option-{index}";

    private int FindEnabledIndex(int start, int direction)
    {
        if (_items.Count == 0)
        {
            return -1;
        }

        var index = Math.Clamp(start, 0, _items.Count - 1);
        while (index >= 0 && index < _items.Count)
        {
            if (!_items[index].Disabled)
            {
                return index;
            }

            index += direction;
        }

        return -1;
    }

    private static bool TryConvertValue(object? raw, out T? value)
    {
        if (raw is null)
        {
            value = default;
            return true;
        }

        if (raw is T typed)
        {
            value = typed;
            return true;
        }

        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object converted = targetType == typeof(Guid)
                ? Guid.Parse(raw.ToString()!)
                : Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture)!;
            value = (T)converted;
            return true;
        }
        catch (FormatException)
        {
            value = default;
            return false;
        }
        catch (InvalidCastException)
        {
            value = default;
            return false;
        }
    }

}
