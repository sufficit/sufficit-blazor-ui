using Microsoft.AspNetCore.Components;
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
    public partial class SUINavGroup : ComponentBase, IDisposable
    {
        private SUINavigationContext _navigationContext = new() { Disabled = false, Expanded = true };

        protected override void OnInitialized()
        {
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

        // ---------------------------------------------------------------------
        // Styling helpers (SUIClassBuilder replaces MudBlazor CssBuilder).
        // ---------------------------------------------------------------------

        protected string Classname =>
            SUIClassBuilder.Default("sui-nav-group")
                .AddClass("sui-nav-group--nested", ParentNavigationContext is not null)
                .AddClass("sui-nav-group--root", ParentNavigationContext is null)
                .AddClass(Class)
                .AddClass("sui-nav-group--disabled", _isDisabled)
                .Build();

        protected string ButtonClassname =>
            SUIClassBuilder.Default("sui-nav-link")
                .AddClass("sui-nav-group__toggle")
                .AddClass("sui-nav-group__toggle--nested", ParentNavigationContext is not null)
                .AddClass("is-expanded", _navigationContext.Expanded)
                .AddClass(HeaderClass)
                .Build();

        protected string IconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__icon")
                .AddClass($"sui-color-{IconColor.ToString().ToLowerInvariant()}", IconColor != SUIColor.Default)
                .Build();

        protected string ExpandIconClassname =>
            SUIClassBuilder.Default("sui-icon sui-nav-link__expand")
                .AddClass("is-expanded", _navigationContext.Expanded && !_isDisabled)
                .AddClass("is-disabled", _navigationContext.Expanded && _isDisabled)
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

        private async Task ExpandedToggleAsync()
        {
            await SetExpandedAsync(!Expanded);
            UpdateNavigationContext();

            // Exclusive accordion (rail flyout only): expanding one group collapses
            // its siblings so a tall feature tree can't fill the whole flyout.
            if (Expanded)
                ParentAccordionScope?.NotifyExpanded(this);
        }

        private async Task SetExpandedAsync(bool value)
        {
            if (Expanded == value)
                return;
            Expanded = value;
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
            if (!Expanded)
                return;

            _ = SetExpandedAsync(false);
            UpdateNavigationContext();
            InvokeAsync(StateHasChanged);
        }

        // ---------------------------------------------------------------------
        // Rail mode (Sufficit) — top-level groups become a rail icon whose
        // children open in a floating flyout. Pure CSS (no JS portal).
        // ---------------------------------------------------------------------

        [CascadingParameter(Name = "SufficitRailMode")]
        public bool RailMode { get; set; }

        protected bool IsRootRail => RailMode && ParentNavigationContext is null;

        private bool _flyoutOpen;
        private CancellationTokenSource? _flyoutCloseCts;

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

        private void OnAnotherRailFlyoutOpened(SUINavGroup opener)
        {
            if (ReferenceEquals(opener, this) || !_flyoutOpen)
                return;

            _flyoutCloseCts?.Cancel();
            _flyoutOpen = false;
            InvokeAsync(StateHasChanged);
        }

        protected void ToggleFlyout()
        {
            if (_flyoutOpen) _flyoutOpen = false;
            else OpenFlyout();
        }

        protected void ScheduleCloseFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutCloseCts = new CancellationTokenSource();
            var token = _flyoutCloseCts.Token;
            _ = InvokeAsync(async () =>
            {
                try { await Task.Delay(170, token); }
                catch (TaskCanceledException) { return; }
                if (token.IsCancellationRequested) return;
                _flyoutOpen = false;
                StateHasChanged();
            });
        }

        private void UpdateNavigationContext()
        {
            _isDisabled = Disabled || ParentNavigationContext is { Disabled: true };
            _isExpanded = Expanded && ParentNavigationContext is null or { Expanded: true };
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
