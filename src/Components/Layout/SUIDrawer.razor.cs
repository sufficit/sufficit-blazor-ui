using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;

namespace Sufficit.Blazor.UI.Components;

public partial class SUIDrawer
{
    private ElementReference _drawerElement;
    private DotNetObjectReference<SUIDrawer>? _dotNetReference;
    private IJSObjectReference? _module;
    private IJSObjectReference? _interop;
    private bool _isCompact;
    private bool _responsiveReady;
    private bool _disposed;

    [Parameter] public bool Open { get; set; }
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

    [Parameter] public bool ShowBackdrop { get; set; } = true;
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;
    [Parameter] public bool ShowCloseButton { get; set; } = true;
    [Parameter] public string? Title { get; set; }
    [Parameter] public string CloseLabel { get; set; } = "Fechar navegação";
    [Parameter] public string AriaLabel { get; set; } = "Navegação principal";
    [Parameter] public string? Id { get; set; }
    [Parameter] public EventCallback<bool> CompactChanged { get; set; }
    [Parameter] public int Elevation { get; set; } = 1;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?> AdditionalAttributes { get; set; } = new();

    protected override void OnInitialized()
    {
        _isCompact = IsTemporary;
        _responsiveReady = !IsResponsive;
        Navigation.LocationChanged += OnLocationChanged;
    }

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
