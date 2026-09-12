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
    /// A deeper level of navigation links. Standalone: plain Blazor + CSS. Preserves rail-mode flyout, exclusive accordion
    /// between siblings, and animated collapse.
    /// </summary>
    public partial class SUINavGroup : ComponentBase, IAsyncDisposable
    {
        private SUINavigationContext _navigationContext = new() { Disabled = false, Expanded = true };
        private bool _expandedState;
        private bool _expandedParameterInitialized;
        private bool _lastExpandedParameter;
        private bool _railInteropConnected;
        private bool _navigationSubscribed;

        /// <summary>Subscribes to the rail flyout coordinator and to navigation changes, and joins the parent accordion scope.</summary>
        protected override void OnInitialized()
        {
            UpdateNavigationContext();

            RailFlyoutOpened += OnAnotherRailFlyoutOpened;
            _railCoordinatorSubscribed = true;

            Navigation.LocationChanged += OnLocationChanged;
            _navigationSubscribed = true;

            ParentAccordionScope?.Register(this);
        }

        /// <summary>Adopts <see cref="Expanded"/> only when the parent supplies a new value, so a rerender does not undo the user's last click.</summary>
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

        /// <summary>Root rail groups only: connects the browser flyout helper on first use and re-clamps the panel to the viewport after every render.</summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!IsRootRail)
            {
                return;
            }

            var module = await GetJsModuleAsync();
            if (!_railInteropConnected)
            {
                await module.InvokeVoidAsync("connectRailFlyout", _flyoutElement);
                _railInteropConnected = true;
            }
            await module.InvokeVoidAsync("updateRailFlyout", _flyoutElement);
        }

        /// <summary>Unsubscribes from the coordinator, navigation and accordion scope, cancels a pending flyout close and releases the browser module.</summary>
        public async ValueTask DisposeAsync()
        {
            if (_railCoordinatorSubscribed)
            {
                RailFlyoutOpened -= OnAnotherRailFlyoutOpened;
                _railCoordinatorSubscribed = false;
            }

            if (_navigationSubscribed)
            {
                Navigation.LocationChanged -= OnLocationChanged;
                _navigationSubscribed = false;
            }

            ParentAccordionScope?.Unregister(this);

            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts?.Dispose();

            if (_jsModuleTask is { } moduleTask)
            {
                try
                {
                    var module = await moduleTask;
                    if (_railInteropConnected)
                    {
                        await module.InvokeVoidAsync("disconnectRailFlyout", _flyoutElement);
                    }
                    await module.DisposeAsync();
                }
                catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
                {
                    // The browser circuit may already be gone during disposal.
                }
            }

            GC.SuppressFinalize(this);
        }

        // ---------------------------------------------------------------------
        // Styling helpers.
        // ---------------------------------------------------------------------

        /// <summary>Root CSS classes: root/nested, expanded, disabled and the user <see cref="Class"/>.</summary>
        protected string Classname =>
            SUIClassBuilder.Default("sui-nav-group")
                .AddClass("sui-nav-group--nested", ParentNavigationContext is not null)
                .AddClass("sui-nav-group--root", ParentNavigationContext is null)
                .AddClass("is-expanded", IsExpanded)
                .AddClass(Class)
                .AddClass("sui-nav-group--disabled", _isDisabled)
                .Build();

        /// <summary>CSS classes of the toggle button, including <see cref="HeaderClass"/>.</summary>
        protected string ButtonClassname =>
            SUIClassBuilder.Default("sui-nav-link")
                .AddClass("sui-nav-group__toggle")
                .AddClass("sui-nav-group__toggle--nested", ParentNavigationContext is not null)
                .AddClass("is-expanded", IsExpanded)
                .AddClass(HeaderClass)
                .Build();

        /// <summary>CSS classes of the leading icon; adds a colour class unless <see cref="IconColor"/> is Default.</summary>
        protected string IconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__icon")
                .AddClass($"sui-color-{IconColor.ToString().ToLowerInvariant()}", IconColor != SUIColor.Default)
                .Build();

        /// <summary>CSS classes of the expand chevron; marked expanded only while open and enabled.</summary>
        protected string ExpandIconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__expand")
                .AddClass("is-expanded", IsExpanded && !_isDisabled)
                .AddClass("is-disabled", IsExpanded && _isDisabled)
                .Build();

        /// <summary>-1 while this group or an ancestor is disabled or collapsed, so hidden toggles leave the Tab order; otherwise 0.</summary>
        protected int ButtonTabIndex
            => _isDisabled || ParentNavigationContext is { Disabled: true } or { Expanded: false } ? -1 : 0;

        // ---------------------------------------------------------------------
        // Parameters
        // ---------------------------------------------------------------------

        [CascadingParameter]
        private SUINavigationContext? ParentNavigationContext { get; set; }

        /// <summary>Additional CSS class applied to the toggle button.</summary>
        [Parameter] public string? HeaderClass { get; set; }
        /// <summary>Custom markup for the title area; replaces <see cref="Title"/> and <see cref="SubTitle"/>.</summary>
        [Parameter] public RenderFragment? TitleContent { get; set; }
        /// <summary>Custom markup for the leading icon; replaces <see cref="Icon"/> and the initials avatar.</summary>
        [Parameter] public RenderFragment? IconContent { get; set; }
        /// <summary>Group title; also the accessible label of the nav and, in rail mode, of the trigger and flyout.</summary>
        [Parameter] public string? Title { get; set; }
        /// <summary>Optional secondary line rendered under the title.</summary>
        [Parameter] public string? SubTitle { get; set; }
        /// <summary>Inline SVG markup for the leading icon. Without it (and without <see cref="IconContent"/>) an avatar with the title initials is shown.</summary>
        [Parameter] public string? Icon { get; set; }
        /// <summary>Semantic colour of the leading icon or initials avatar. Default <see cref="SUIColor.Default"/>.</summary>
        [Parameter] public SUIColor IconColor { get; set; } = SUIColor.Default;
        /// <summary>Disables the toggle; nested links and groups inherit the disabled state through the cascade.</summary>
        [Parameter] public bool Disabled { get; set; }
        /// <summary>Kept for API compatibility; the standalone markup renders no ripple effect. Default true.</summary>
        [Parameter] public bool Ripple { get; set; } = true;
        /// <summary>Initial or bound expanded state. One-way values and <c>@bind-Expanded</c> both work; user clicks keep a local state until the parent supplies a different value.</summary>
        [Parameter] public bool Expanded { get; set; }
        /// <summary>Hides the expand chevron on the toggle.</summary>
        [Parameter] public bool HideExpandIcon { get; set; }
        /// <summary>Optional max-height in pixels for the collapsible children area.</summary>
        [Parameter] public int? MaxHeight { get; set; }
        /// <summary>Inline SVG markup for the expand chevron. Default <see cref="SUIIcons.ArrowDropDown"/>.</summary>
        [Parameter] public string ExpandIcon { get; set; } = SUIIcons.ArrowDropDown;
        /// <summary>Nested navigation links and groups.</summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }
        /// <summary>Raised when the user toggles the group; enables <c>@bind-Expanded</c>.</summary>
        [Parameter] public EventCallback<bool> ExpandedChanged { get; set; }
        /// <summary>Additional CSS class for the root element.</summary>
        [Parameter] public string? Class { get; set; }
        /// <summary>Inline style for the root <c>nav</c> element (non-rail mode).</summary>
        [Parameter] public string? Style { get; set; }
        /// <summary>Unmatched attributes forwarded to the root <c>nav</c> element (non-rail mode).</summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object?> UserAttributes { get; set; } = new();

        // ---------------------------------------------------------------------
        // Effective state — derived from this group + ancestor cascade.
        // ---------------------------------------------------------------------

        private bool _isDisabled;
        private bool _isExpanded;

        /// <summary>Current expanded state, including local toggles not yet reflected by the parent.</summary>
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
        private SUINavAccordionScope? ParentAccordionScope { get; set; }

        private readonly SUINavAccordionScope _childAccordionScope = new();

        /// <summary>Accordion scope cascaded to nested groups; non-null only when this group is itself inside a scope, which confines exclusivity to rail flyouts.</summary>
        protected SUINavAccordionScope? ChildAccordionScope
            => ParentAccordionScope is not null ? _childAccordionScope : null;

        internal void CollapseFromScope()
        {
            if (!IsExpanded)
                return;

            _ = SetExpandedAsync(false);
            UpdateNavigationContext();
            InvokeAsync(StateHasChanged);
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
    public sealed class SUINavAccordionScope
    {
        private readonly List<SUINavGroup> _members = new();

        /// <summary>Adds a group to the scope; registering twice is a no-op.</summary>
        public void Register(SUINavGroup group)
        {
            if (!_members.Contains(group))
                _members.Add(group);
        }

        /// <summary>Removes a group from the scope.</summary>
        public void Unregister(SUINavGroup group)
            => _members.Remove(group);

        /// <summary>Collapses every registered group other than <paramref name="opener"/>.</summary>
        public void NotifyExpanded(SUINavGroup opener)
        {
            foreach (var member in _members)
                if (!ReferenceEquals(member, opener))
                    member.CollapseFromScope();
        }
    }
}
