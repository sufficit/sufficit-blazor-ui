// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.State;
using MudBlazor.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Blazor.UI.Components
{
#nullable enable

    /// <summary>
    /// A deeper level of navigation links as part of a <see cref="MudNavMenu"/>.
    /// </summary>
    /// <seealso cref="MudNavLink"/>
    /// <seealso cref="MudNavMenu"/>
    public partial class SuffNavGroup : MudComponentBase, IDisposable
    {
        private readonly ParameterState<bool> _expandedState;
        private readonly ParameterState<bool> _disabledState;
        private readonly ParameterState<NavigationContext?> _parentNavigationContextState;
        private NavigationContext _navigationContext = new(false, true);

        public SuffNavGroup()
        {
            using var registerScope = CreateRegisterScope();
            _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
                .WithParameter(() => Disabled)
                .WithChangeHandler(UpdateNavigationContext);
            _parentNavigationContextState = registerScope.RegisterParameter<NavigationContext?>(nameof(ParentNavigationContext))
                .WithParameter(() => ParentNavigationContext)
                .WithChangeHandler(UpdateNavigationContext);
            _expandedState = registerScope.RegisterParameter<bool>(nameof(Expanded))
                .WithParameter(() => Expanded)
                .WithEventCallback(() => ExpandedChanged)
                .WithChangeHandler(UpdateNavigationContext);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            UpdateNavigationContext();

            RailFlyoutOpened += OnAnotherRailFlyoutOpened;
            _railCoordinatorSubscribed = true;

            ParentAccordionScope?.Register(this);
        }

        public void Dispose()
        {
            if (_railCoordinatorSubscribed)
            {
                RailFlyoutOpened -= OnAnotherRailFlyoutOpened;
                _railCoordinatorSubscribed = false;
            }

            ParentAccordionScope?.Unregister(this);

            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected string Classname =>
            new CssBuilder("mud-nav-group")
                .AddClass("mud-nav-group-nested", ParentNavigationContext is not null)
                .AddClass("mud-nav-group-root", ParentNavigationContext is null)
                .AddClass(Class)
                .AddClass("mud-nav-group-disabled", _disabledState.Value)
                .Build();

        protected string ButtonClassname =>
            new CssBuilder("mud-nav-link")
                .AddClass("mud-nav-group-toggle")
                .AddClass("mud-nav-group-toggle-nested", ParentNavigationContext is not null)
                .AddClass($"mud-ripple", Ripple)
                .AddClass("mud-expanded", _expandedState.Value)
                .AddClass(HeaderClass)
                .Build();

        protected string IconClassname =>
            new CssBuilder("mud-nav-link-icon")
                .AddClass("mud-nav-link-icon-default", IconColor == Color.Default)
                .Build();

        protected string ExpandIconClassname =>
            new CssBuilder("mud-nav-link-expand-icon")
                .AddClass("mud-transform", _expandedState.Value && _disabledState.Value is false)
                .AddClass("mud-transform-disabled", _expandedState.Value && _disabledState.Value)
                .Build();

        protected int ButtonTabIndex => _disabledState.Value || _parentNavigationContextState.Value is { Disabled: true } or { Expanded: false } ? -1 : 0;

        [CascadingParameter]
        private NavigationContext? ParentNavigationContext { get; set; }

        /// <summary>
        /// The CSS classes applied to this nav group title.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  You can use spaces to separate multiple classes.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public string? HeaderClass { get; set; }

        /// <summary>
        /// The content within the title area.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, overrides the <see cref="Title"/> property.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public RenderFragment? TitleContent { get; set; }

        /// <summary>
        /// The content within the icon area.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, overrides the <see cref="Icon"/> property.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public RenderFragment? IconContent { get; set; }

        /// <summary>
        /// The text shown for this group.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public string? Title { get; set; }

        /// <summary>
        /// The sub text shown for this group.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public string? SubTitle { get; set; }

        /// <summary>
        /// The icon displayed next to the <see cref="Title"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public string? Icon { get; set; }

        /// <summary>
        /// The color of the icon when <see cref="Icon"/> is set.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public Color IconColor { get; set; } = Color.Default;

        /// <summary>
        /// Prevents the user from interacting with this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Shows a ripple effect when the user clicks this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public bool Ripple { get; set; } = true;

        /// <summary>
        /// Displays the items within this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When this value changes, <see cref="ExpandedChanged"/> occurs.  Can be bound via <c>@bind-Expanded</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public bool Expanded { get; set; }

        /// <summary>
        /// Hides the expand/collapse icon.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public bool HideExpandIcon { get; set; }

        /// <summary>
        /// The maximum height, in pixels, of this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, it will override the CSS default.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public int? MaxHeight { get; set; }

        /// <summary>
        /// The icon for expanding and collapsing this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ArrowDropDown"/>.  Only shows when <see cref="HideExpandIcon"/> is <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public string ExpandIcon { get; set; } = Icons.Material.Filled.ArrowDropDown;

        /// <summary>
        /// The content within this group.
        /// </summary>
        /// <remarks>
        /// Typically contains <see cref="MudNavGroup"/> and <see cref="MudNavLink"/> components.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Occurs when <see cref="Expanded"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<bool> ExpandedChanged { get; set; }

        private async Task ExpandedToggleAsync()
        {
            await _expandedState.SetValueAsync(!_expandedState.Value);
            UpdateNavigationContext();

            // Exclusive accordion (rail flyout only): expanding one group collapses
            // its siblings so a tall feature tree (e.g. Telephony) can't fill the
            // whole flyout and hide everything below.
            if (_expandedState.Value)
                ParentAccordionScope?.NotifyExpanded(this);
        }

        // ---------------------------------------------------------------------
        // Accordion scope (Sufficit) — one expanded group per level inside the
        // rail flyout. A group consumes its parent's scope (sibling exclusivity)
        // and provides a fresh scope to its own children (so each nesting level is
        // independent). Only active within the flyout chain: in the regular
        // expanded drawer ParentAccordionScope is null, so nothing changes there.
        // ---------------------------------------------------------------------

        [CascadingParameter]
        private NavAccordionScope? ParentAccordionScope { get; set; }

        private readonly NavAccordionScope _childAccordionScope = new();

        /// <summary>Scope cascaded to this group's children — only inside the flyout chain.</summary>
        protected NavAccordionScope? ChildAccordionScope
            => ParentAccordionScope is not null ? _childAccordionScope : null;

        /// <summary>Collapse this group because a sibling was expanded.</summary>
        internal void CollapseFromScope()
        {
            if (!_expandedState.Value)
                return;

            _ = _expandedState.SetValueAsync(false);
            UpdateNavigationContext();
            InvokeAsync(StateHasChanged);
        }

        // ---------------------------------------------------------------------
        // Rail mode (Sufficit) — top-level groups become a rail icon whose
        // children open in a floating glass MudPopover flyout. Off by default,
        // so mobile and every other usage keep the standard accordion column.
        // The flyout escapes the drawer's overflow clipping via the popover portal.
        // ---------------------------------------------------------------------

        /// <summary>When true (cascaded by the desktop rail shell) and this is a
        /// root group, render as a rail icon + popover flyout instead of inline.</summary>
        [CascadingParameter(Name = "SufficitRailMode")]
        public bool RailMode { get; set; }

        /// <summary>Root-level group rendered in rail mode.</summary>
        protected bool IsRootRail => RailMode && ParentNavigationContext is null;

        private bool _flyoutOpen;
        private CancellationTokenSource? _flyoutCloseCts;

        // Coordinator: only ONE rail flyout may be open at a time. Without this, moving
        // the pointer from one rail icon to another left the first flyout in its close
        // grace period while the second opened → two panels overlapping ("ghost").
        private static event Action<SuffNavGroup>? RailFlyoutOpened;
        private bool _railCoordinatorSubscribed;

        /// <summary>Context for flyout children: forced expanded so links stay focusable/tabbable.</summary>
        protected NavigationContext RailFlyoutContext => _navigationContext with { Expanded = true };

        protected void OpenFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutOpen = true;
            // Tell every other rail group to close immediately (no grace period).
            RailFlyoutOpened?.Invoke(this);
        }

        private void OnAnotherRailFlyoutOpened(SuffNavGroup opener)
        {
            if (ReferenceEquals(opener, this) || !_flyoutOpen)
                return;

            _flyoutCloseCts?.Cancel();
            _flyoutOpen = false;
            InvokeAsync(StateHasChanged);
        }

        protected void ToggleFlyout()
        {
            if (_flyoutOpen)
            {
                _flyoutOpen = false;
            }
            else
            {
                OpenFlyout();
            }
        }

        /// <summary>Close after a short grace period so the pointer can travel the
        /// gap from the rail trigger into the flyout without it disappearing.</summary>
        protected void ScheduleCloseFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts = new CancellationTokenSource();
            var token = _flyoutCloseCts.Token;
            _ = InvokeAsync(async () =>
            {
                try { await Task.Delay(170, token); }
                catch (TaskCanceledException) { return; }
                if (token.IsCancellationRequested)
                    return;
                _flyoutOpen = false;
                StateHasChanged();
            });
        }

        private void UpdateNavigationContext()
            => _navigationContext = _navigationContext with
            {
                Disabled = _disabledState.Value || _parentNavigationContextState.Value is { Disabled: true },
                Expanded = _expandedState.Value
                           && _parentNavigationContextState.Value is null or { Expanded: true }
            };

        /// <summary>
        /// Gets first 2 letters from title, for icon generate
        /// </summary>
        protected string GetInitials()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return string.Empty;

            string result = string.Empty;
            foreach (var s in Title.Split(" "))
                if (s.Length > 3)
                    result += s[0];
            return result;
        }
    }

    /// <summary>
    /// Coordinates exclusive accordion behaviour among sibling <see cref="SuffNavGroup"/>
    /// at one nesting level: expanding a group collapses the others. Cascaded by a parent
    /// group to its direct children (used inside the rail flyout).
    /// </summary>
    public sealed class NavAccordionScope
    {
        private readonly System.Collections.Generic.List<SuffNavGroup> _members = new();

        public void Register(SuffNavGroup group)
        {
            if (!_members.Contains(group))
                _members.Add(group);
        }

        public void Unregister(SuffNavGroup group)
            => _members.Remove(group);

        public void NotifyExpanded(SuffNavGroup opener)
        {
            foreach (var member in _members)
                if (!ReferenceEquals(member, opener))
                    member.CollapseFromScope();
        }
    }
}
