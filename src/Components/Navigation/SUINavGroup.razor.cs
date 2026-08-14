using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Sufficit.Blazor.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Blazor.UI.Components
{
#nullable enable

    /// <summary>
    /// A deeper level of navigation links. Standalone (no MudBlazor dependency):
    /// plain Blazor + CSS. Preserves rail-mode flyout, exclusive accordion
    /// between siblings, and animated collapse.
    /// </summary>
    public partial class SUINavGroup : ComponentBase, IAsyncDisposable
    {
        private SUINavigationContext _navigationContext = new() { Disabled = false, Expanded = true };
        private bool _expandedState;
        private bool _expandedParameterInitialized;
        private bool _lastExpandedParameter;

        protected override void OnInitialized()
        {
            UpdateNavigationContext();

            RailFlyoutOpened += OnAnotherRailFlyoutOpened;
            _railCoordinatorSubscribed = true;

            ParentAccordionScope?.Register(this);
        }

        protected override void OnParametersSet()
        {
            // Expanded can be supplied as a one-way route expression (the common
            // rail/flyout case) or through @bind-Expanded. Keep a local interaction
            // state when the parent keeps supplying the same value; otherwise a
            // parent rerender would immediately undo a user's click.
            if (!_expandedParameterInitialized || _lastExpandedParameter != Expanded)
            {
                _expandedState = Expanded;
                _expandedParameterInitialized = true;
                _lastExpandedParameter = Expanded;
            }

            UpdateNavigationContext();
        }

        public async ValueTask DisposeAsync()
        {
            if (_railCoordinatorSubscribed)
            {
                RailFlyoutOpened -= OnAnotherRailFlyoutOpened;
                _railCoordinatorSubscribed = false;
            }

            ParentAccordionScope?.Unregister(this);

            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts?.Dispose();

            if (_jsModuleTask is { } moduleTask)
            {
                try
                {
                    var module = await moduleTask;
                    await module.DisposeAsync();
                }
                catch (Exception)
                {
                    // The browser circuit may already be gone during disposal.
                }
            }

            GC.SuppressFinalize(this);
        }

        // ---------------------------------------------------------------------
        // Styling helpers (SUIClassBuilder replaces MudBlazor CssBuilder).
        // ---------------------------------------------------------------------

        protected string Classname =>
            SUIClassBuilder.Default("sui-nav-group")
                .AddClass("sui-nav-group--nested", ParentNavigationContext is not null)
                .AddClass("sui-nav-group--root", ParentNavigationContext is null)
                .AddClass("is-expanded", IsExpanded)
                .AddClass(Class)
                .AddClass("sui-nav-group--disabled", _isDisabled)
                .Build();

        protected string ButtonClassname =>
            SUIClassBuilder.Default("sui-nav-link")
                .AddClass("sui-nav-group__toggle")
                .AddClass("sui-nav-group__toggle--nested", ParentNavigationContext is not null)
                .AddClass("is-expanded", IsExpanded)
                .AddClass(HeaderClass)
                .Build();

        protected string IconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__icon")
                .AddClass($"sui-color-{IconColor.ToString().ToLowerInvariant()}", IconColor != SUIColor.Default)
                .Build();

        protected string ExpandIconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__expand")
                .AddClass("is-expanded", IsExpanded && !_isDisabled)
                .AddClass("is-disabled", IsExpanded && _isDisabled)
                .Build();

        protected int ButtonTabIndex
            => _isDisabled || ParentNavigationContext is { Disabled: true } or { Expanded: false } ? -1 : 0;

        // ---------------------------------------------------------------------
        // Parameters
        // ---------------------------------------------------------------------

        [CascadingParameter]
        private SUINavigationContext? ParentNavigationContext { get; set; }

        [Parameter] public string? HeaderClass { get; set; }
        [Parameter] public RenderFragment? TitleContent { get; set; }
        [Parameter] public RenderFragment? IconContent { get; set; }
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? SubTitle { get; set; }
        [Parameter] public string? Icon { get; set; }
        [Parameter] public SUIColor IconColor { get; set; } = SUIColor.Default;
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool Ripple { get; set; } = true;
        [Parameter] public bool Expanded { get; set; }
        [Parameter] public bool HideExpandIcon { get; set; }
        [Parameter] public int? MaxHeight { get; set; }
        [Parameter] public string ExpandIcon { get; set; } = SUIIcons.ArrowDropDown;
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback<bool> ExpandedChanged { get; set; }
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object?> UserAttributes { get; set; } = new();

        // ---------------------------------------------------------------------
        // Effective state — derived from this group + ancestor cascade.
        // ---------------------------------------------------------------------

        private bool _isDisabled;
        private bool _isExpanded;

        protected bool IsExpanded => _expandedState;

        private async Task ExpandedToggleAsync()
        {
            await SetExpandedAsync(!IsExpanded);
            UpdateNavigationContext();

            // Exclusive accordion (rail flyout only): expanding one group collapses
            // its siblings so a tall feature tree can't fill the whole flyout.
            if (IsExpanded)
                ParentAccordionScope?.NotifyExpanded(this);
        }

        private async Task SetExpandedAsync(bool value)
        {
            if (IsExpanded == value)
                return;
            _expandedState = value;
            if (ExpandedChanged.HasDelegate)
                await ExpandedChanged.InvokeAsync(value);
        }

        // ---------------------------------------------------------------------
        // Accordion scope (Sufficit).
        // ---------------------------------------------------------------------

        [CascadingParameter]
        private NavAccordionScope? ParentAccordionScope { get; set; }

        private readonly NavAccordionScope _childAccordionScope = new();

        protected NavAccordionScope? ChildAccordionScope
            => ParentAccordionScope is not null ? _childAccordionScope : null;

        internal void CollapseFromScope()
        {
            if (!IsExpanded)
                return;

            _ = SetExpandedAsync(false);
            UpdateNavigationContext();
            InvokeAsync(StateHasChanged);
        }

        // ---------------------------------------------------------------------
        // Rail mode (Sufficit) — top-level groups become a rail icon whose
        // children open in a floating flyout. CSS is the fallback; the shared
        // browser helper clamps the panel to the viewport when it reaches an edge.
        // ---------------------------------------------------------------------

        [CascadingParameter(Name = "SufficitRailMode")]
        public bool RailMode { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        protected bool IsRootRail => RailMode && ParentNavigationContext is null;

        private bool _flyoutOpen;
        private bool _pointerWithinRail;
        private bool _pointerWithinFlyout;
        private ElementReference _flyoutElement;
        private Task<IJSObjectReference>? _jsModuleTask;
        private CancellationTokenSource? _flyoutCloseCts;
        // The pointer needs time to cross the intentional gap between the
        // fixed rail and the floating panel, including diagonal movement.
        private const int FlyoutCloseDelayMilliseconds = 900;

        private static event Action<SUINavGroup>? RailFlyoutOpened;
        private bool _railCoordinatorSubscribed;

        protected SUINavigationContext RailFlyoutContext
            => _navigationContext with { Expanded = true };

        protected void OpenFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutOpen = true;
            RailFlyoutOpened?.Invoke(this);
        }

        protected void EnterRail()
        {
            _pointerWithinRail = true;
            OpenFlyout();
        }

        protected void LeaveRail()
        {
            _pointerWithinRail = false;
            ScheduleCloseFlyout();
        }

        protected void EnterFlyout()
        {
            _pointerWithinFlyout = true;
            OpenFlyout();
        }

        protected void LeaveFlyout()
        {
            _pointerWithinFlyout = false;
            ScheduleCloseFlyout();
        }

        private void OnAnotherRailFlyoutOpened(SUINavGroup opener)
        {
            if (ReferenceEquals(opener, this) || !_flyoutOpen)
                return;

            _flyoutCloseCts?.Cancel();
            _pointerWithinRail = false;
            _pointerWithinFlyout = false;
            _flyoutOpen = false;
            InvokeAsync(StateHasChanged);
        }

        protected void ToggleFlyout()
        {
            if (_flyoutOpen)
            {
                _flyoutCloseCts?.Cancel();
                _pointerWithinRail = false;
                _pointerWithinFlyout = false;
                _flyoutOpen = false;
            }
            else OpenFlyout();
        }

        protected void ScheduleCloseFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts = new CancellationTokenSource();
            var token = _flyoutCloseCts.Token;
            _ = InvokeAsync(async () =>
            {
                try { await Task.Delay(FlyoutCloseDelayMilliseconds, token); }
                catch (TaskCanceledException) { return; }
                if (token.IsCancellationRequested) return;
                if (_pointerWithinRail || _pointerWithinFlyout) return;
                if (await IsRailInteractionActiveAsync()) return;
                _flyoutOpen = false;
                StateHasChanged();
            });
        }

        private async Task<bool> IsRailInteractionActiveAsync()
        {
            try
            {
                _jsModuleTask ??= JS.InvokeAsync<IJSObjectReference>(
                    "import",
                    "/_content/Sufficit.Blazor.UI/sufficit-ui.js").AsTask();
                var module = await _jsModuleTask;
                return await module.InvokeAsync<bool>(
                    "isRailInteractionActive",
                    _flyoutElement);
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
            {
                return false;
            }
        }

        private void UpdateNavigationContext()
        {
            _isDisabled = Disabled || ParentNavigationContext is { Disabled: true };
            _isExpanded = IsExpanded && ParentNavigationContext is null or { Expanded: true };
            _navigationContext = _navigationContext with
            {
                Disabled = _isDisabled,
                Expanded = _isExpanded
            };
        }

        /// <summary>Gets first letters from title, for icon generation.</summary>
        protected string GetInitials()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return string.Empty;

            string result = string.Empty;
            foreach (var s in Title.Split(' '))
                if (s.Length > 3)
                    result += s[0];
            return result;
        }
    }

    /// <summary>
    /// Coordinates exclusive accordion behaviour among sibling <see cref="SUINavGroup"/>
    /// at one nesting level: expanding a group collapses the others.
    /// </summary>
    public sealed class NavAccordionScope
    {
        private readonly List<SUINavGroup> _members = new();

        public void Register(SUINavGroup group)
        {
            if (!_members.Contains(group))
                _members.Add(group);
        }

        public void Unregister(SUINavGroup group)
            => _members.Remove(group);

        public void NotifyExpanded(SUINavGroup opener)
        {
            foreach (var member in _members)
                if (!ReferenceEquals(member, opener))
                    member.CollapseFromScope();
        }
    }
}
