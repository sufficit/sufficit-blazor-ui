using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Blazor.UI.Components
{
#nullable enable

    public partial class SUINavGroup
    {
        // ---------------------------------------------------------------------
        // Rail mode (Sufficit) — top-level groups become a rail icon whose
        // children open in a floating flyout. CSS is the fallback; the shared
        // browser helper clamps the panel to the viewport when it reaches an edge.
        // ---------------------------------------------------------------------

        /// <summary>Cascaded as <c>SufficitRailMode</c> by the host layout; true turns top-level groups into rail icons whose children open in a floating flyout.</summary>
        [CascadingParameter(Name = "SufficitRailMode")]
        public bool RailMode { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        /// <summary>True when <see cref="RailMode"/> is on and this group has no parent, i.e. it renders as a rail trigger.</summary>
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

        /// <summary>Navigation context cascaded into the flyout: this group's state with Expanded forced to true so the children render open.</summary>
        protected SUINavigationContext RailFlyoutContext
            => _navigationContext with { Expanded = true };

        /// <summary>Opens the flyout, cancels a pending close and tells every other rail group to close its own.</summary>
        protected void OpenFlyout()
        {
            _flyoutCloseCts?.Cancel();
            _flyoutOpen = true;
            RailFlyoutOpened?.Invoke(this);
        }

        /// <summary>Pointer entered the rail trigger: opens the flyout.</summary>
        protected void EnterRail()
        {
            _pointerWithinRail = true;
            OpenFlyout();
        }

        /// <summary>Pointer left the rail trigger: schedules the delayed close.</summary>
        protected void LeaveRail()
        {
            _pointerWithinRail = false;
            ScheduleCloseFlyout();
        }

        /// <summary>Pointer entered the floating panel: keeps it open.</summary>
        protected void EnterFlyout()
        {
            _pointerWithinFlyout = true;
            OpenFlyout();
        }

        /// <summary>Pointer left the floating panel: schedules the delayed close.</summary>
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

        private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs args)
        {
            if (!IsRootRail || !_flyoutOpen)
                return;

            _flyoutCloseCts?.Cancel();
            _pointerWithinRail = false;
            _pointerWithinFlyout = false;
            _flyoutOpen = false;
            _ = InvokeAsync(StateHasChanged);
        }

        /// <summary>Click on the rail trigger: closes an open flyout at once, otherwise opens it.</summary>
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

        /// <summary>Closes the flyout after a 900 ms grace period unless the pointer returned to the rail or panel, or hover/focus is still inside according to the browser helper.</summary>
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
                var module = await GetJsModuleAsync();
                return await module.InvokeAsync<bool>(
                    "isRailInteractionActive",
                    _flyoutElement);
            }
            catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
            {
                return false;
            }
        }

        private Task<IJSObjectReference> GetJsModuleAsync()
            => _jsModuleTask ??= JS.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/Sufficit.Blazor.UI/Components/Navigation/SUINavGroup.razor.js").AsTask();
    }
}
