using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Side navigation panel. Permanent drawers stay in the layout flow; temporary and
/// compact responsive drawers become a modal (<c>role="dialog"</c>, <c>aria-modal</c>,
/// <c>inert</c> while closed) with a backdrop, Escape dismissal and focus trapping
/// handled by <c>SUIDrawer.razor.js</c>.
/// </summary>
public partial class SUIDrawer
{
    private ElementReference _drawerElement;
    private DotNetObjectReference<SUIDrawer>? _dotNetReference;
    private IJSObjectReference? _module;
    private IJSObjectReference? _interop;
    private bool _isCompact;
    private bool _responsiveReady;
    private bool _disposed;

    /// <summary>Whether the drawer is open (adds <c>sui-drawer--open</c>). Bindable through <see cref="OpenChanged"/>.</summary>
    [Parameter] public bool Open { get; set; }
    /// <summary>Raised when the drawer opens or closes itself: close button, backdrop, Escape, navigation or responsive auto-management.</summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Display behavior: <c>permanent</c>, <c>temporary</c>, or <c>responsive</c>.
    /// Responsive drawers use <see cref="ResponsiveBreakpoint"/> to switch modes.
    /// </summary>
    [Parameter] public string Variant { get; set; } = "permanent";

    /// <summary>Drawer width as a CSS length. Default 256px.</summary>
    [Parameter] public string Width { get; set; } = "256px";

    /// <summary>Viewport width, in pixels, below which a responsive drawer is compact.</summary>
    [Parameter] public int ResponsiveBreakpoint { get; set; } = 900;

    /// <summary>Expands a compact responsive drawer to the complete dynamic viewport.</summary>
    [Parameter] public bool FullScreenOnCompact { get; set; }

    /// <summary>Opens on wide viewports and closes on compact viewports automatically.</summary>
    [Parameter] public bool AutoManageResponsiveOpen { get; set; }

    /// <summary>Closes the compact drawer after Blazor navigation completes.</summary>
    [Parameter] public bool CloseOnNavigate { get; set; }

    /// <summary>Renders a backdrop behind a compact open drawer. Default <c>true</c>.</summary>
    [Parameter] public bool ShowBackdrop { get; set; } = true;
    /// <summary>Closes the drawer when the backdrop is clicked. Default <c>true</c>.</summary>
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;
    /// <summary>Shows the compact header with <see cref="Title"/> and a close button. Default <c>true</c>.</summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;
    /// <summary>Heading shown in the compact header next to the close button.</summary>
    [Parameter] public string? Title { get; set; }
    /// <summary>Accessible title of the compact close button. Default "Fechar navegação".</summary>
    [Parameter] public string CloseLabel { get; set; } = "Fechar navegação";
    /// <summary><c>aria-label</c> of the <c>aside</c> element. Default "Navegação principal".</summary>
    [Parameter] public string AriaLabel { get; set; } = "Navegação principal";
    /// <summary><c>id</c> attribute of the <c>aside</c> element.</summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>Raised when a responsive drawer is first measured or crosses <see cref="ResponsiveBreakpoint"/> (<c>true</c> = compact).</summary>
    [Parameter] public EventCallback<bool> CompactChanged { get; set; }
    /// <summary>Shadow level 1-3 (<c>sui-drawer--e1</c>..<c>e3</c>); out-of-range values fall back to 1.</summary>
    [Parameter] public int Elevation { get; set; } = 1;
    /// <summary>Additional CSS classes appended to the <c>aside</c>.</summary>
    [Parameter] public string? Class { get; set; }
    /// <summary>Inline style appended after the <c>--sui-drawer-width</c> custom property.</summary>
    [Parameter] public string? Style { get; set; }
    /// <summary>Drawer body, rendered inside <c>sui-drawer__content</c>.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Unmatched attributes forwarded to the <c>aside</c> element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> AdditionalAttributes { get; set; } = new();

    /// <summary>Seeds the compact state from the variant and subscribes to navigation changes.</summary>
    protected override void OnInitialized()
    {
        _isCompact = IsTemporary;
        _responsiveReady = !IsResponsive;
        Navigation.LocationChanged += OnLocationChanged;
    }

    /// <summary>For temporary/responsive drawers, loads the JS module on first render and syncs the open state with it.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed || (!IsResponsive && !IsTemporary))
            return;

        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Sufficit.Blazor.UI/Components/Layout/SUIDrawer.razor.js");
            _dotNetReference = DotNetObjectReference.Create(this);
            _interop = await _module.InvokeAsync<IJSObjectReference>(
                "initialize", _drawerElement, _dotNetReference, ResponsiveBreakpoint, NormalizedVariant);
        }

        if (_interop is not null)
            await _interop.InvokeVoidAsync("setOpen", Open, _isCompact, FullScreenOnCompact);
    }

    /// <summary>Called from JS when the viewport crosses the breakpoint; raises <see cref="CompactChanged"/> and, with <see cref="AutoManageResponsiveOpen"/>, opens or closes the drawer.</summary>
    [JSInvokable]
    public async Task SetCompactStateAsync(bool compact)
    {
        if (_disposed)
            return;

        var changed = !_responsiveReady || _isCompact != compact;
        _isCompact = compact;
        _responsiveReady = true;

        if (changed && CompactChanged.HasDelegate)
            await CompactChanged.InvokeAsync(compact);

        if (changed && IsResponsive && AutoManageResponsiveOpen)
            await SetOpenAsync(!compact);
        else
            await InvokeAsync(StateHasChanged);
    }

    /// <summary>Called from JS when Escape is pressed inside a compact open drawer.</summary>
    [JSInvokable]
    public Task CloseFromKeyboardAsync() => SetOpenAsync(false);

    private Task CloseAsync() => SetOpenAsync(false);

    private Task CloseFromBackdropAsync()
        => CloseOnBackdropClick ? SetOpenAsync(false) : Task.CompletedTask;

    private async Task SetOpenAsync(bool value)
    {
        if (Open == value)
            return;

        Open = value;
        await OpenChanged.InvokeAsync(value);
        await InvokeAsync(StateHasChanged);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (!CloseOnNavigate || !_isCompact || !Open || _disposed)
            return;

        _ = InvokeAsync(() => SetOpenAsync(false));
    }

    /// <summary>Unsubscribes from navigation and releases the JS interop; safe when the circuit is already gone.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        Navigation.LocationChanged -= OnLocationChanged;

        try
        {
            if (_interop is not null)
                await _interop.InvokeVoidAsync("dispose");
            if (_module is not null)
                await _module.DisposeAsync();
        }
        catch (Exception exception) when (exception is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // The browser may already be unavailable during teardown or prerendering.
        }

        _dotNetReference?.Dispose();
        GC.SuppressFinalize(this);
    }

    private string NormalizedVariant
        => Variant?.Trim().ToLowerInvariant() switch
        {
            "temporary" => "temporary",
            "responsive" => "responsive",
            _ => "permanent",
        };

    private bool IsResponsive => NormalizedVariant == "responsive";
    private bool IsTemporary => NormalizedVariant == "temporary";
    private string? Role => _isCompact ? "dialog" : null;
    private string? AriaModal => _isCompact && Open ? "true" : null;
    private string? AriaHidden => _isCompact ? (!Open).ToString().ToLowerInvariant() : null;
    private string? TabIndex => _isCompact ? "-1" : null;
    private string StyleValue => $"--sui-drawer-width:{Width};{Style}";

    private string Classname
        => SUIClassBuilder.Default("sui-drawer")
            .AddClass($"sui-drawer--{NormalizedVariant}")
            .AddClass("sui-drawer--open", Open)
            .AddClass("sui-drawer--compact", _isCompact)
            .AddClass("sui-drawer--ready", _responsiveReady)
            .AddClass("sui-drawer--fullscreen-compact", FullScreenOnCompact)
            .AddClass($"sui-drawer--e{(Elevation is > 0 and < 4 ? Elevation : 1)}")
            .AddClass(Class)
            .Build();
}
