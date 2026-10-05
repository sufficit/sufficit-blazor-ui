using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Shared motor for transient-notification hosts (snackbar, toast). Owns the
/// visible-entry stack — enqueue with cap, per-entry auto-dismiss, manual
/// dismissal and teardown — plus the WCAG 2.2.1 timing requirement: the
/// expiry countdown pauses while the entry is hovered or focused and resumes
/// from the remaining time (not from zero) afterwards. Subclasses render the
/// markup and map tones; queue, expiry and accessibility timing live here
/// exactly once.
/// </summary>
public abstract class SUINotificationHostBase<TEntry> : SUIComponentBase, IDisposable
    where TEntry : class
{
    private sealed class Slot
    {
        public required TEntry Entry { get; set; }
        public readonly CancellationTokenSource Lifetime = new();
        public CancellationTokenSource? Countdown;
        public TimeSpan? Remaining;
        public bool Paused;
        public TaskCompletionSource Wake { get; private set; } = NewWake();
        public TaskCompletionSource SwapWake()
        {
            var opened = Wake;
            opened.TrySetResult();
            Wake = NewWake();
            return Wake;
        }
    }

    private readonly object _gate = new();
    private readonly Dictionary<Guid, Slot> _slots = new();
    private readonly List<TEntry> _entries = new();
    private readonly Action<TEntry> _handler;
    private bool _disposed;

    /// <summary>Builds the host and pre-binds the shared enqueue handler.</summary>
    protected SUINotificationHostBase()
        => _handler = entry => _ = EnqueueAsync(entry);

    /// <summary>Maximum simultaneous entries; the oldest is evicted beyond it.</summary>
    protected abstract int MaxVisible { get; }

    /// <summary>Unique id of an entry, used for dismissal and pause/resume.</summary>
    protected abstract Guid IdOf(TEntry entry);

    /// <summary>UTC deadline after which the entry auto-dismisses.</summary>
    protected abstract DateTime ExpiresAtOf(TEntry entry);

    /// <summary>Returns a copy of <paramref name="entry"/> with a new deadline.</summary>
    protected abstract TEntry WithExpiresAt(TEntry entry, DateTime expiresAt);

    /// <summary>Subscribes the shared enqueue handler to the service event.</summary>
    protected abstract void Subscribe(Action<TEntry> handler);

    /// <summary>Unsubscribes the shared enqueue handler from the service event.</summary>
    protected abstract void Unsubscribe(Action<TEntry> handler);

    /// <summary>Entries currently visible, oldest first.</summary>
    protected IReadOnlyList<TEntry> Entries
    {
        get { lock (_gate) return _entries.ToArray(); }
    }

    /// <summary>Subscribes the shared enqueue handler to the service event.</summary>
    protected override void OnInitialized()
        => Subscribe(_handler);

    private static TaskCompletionSource NewWake()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private async Task EnqueueAsync(TEntry entry)
    {
        if (_disposed) return;
        var id = IdOf(entry);
        await InvokeAsync(() =>
        {
            if (_disposed) return;
            lock (_gate)
            {
                while (_entries.Count >= MaxVisible)
                {
                    var oldestId = IdOf(_entries[0]);
                    _entries.RemoveAt(0);
                    if (_slots.Remove(oldestId, out var oldest)) Release(oldest);
                }
                _entries.Add(entry);
                _slots[id] = new Slot { Entry = entry };
            }
            StateHasChanged();
        });
        if (!_disposed && _slots.TryGetValue(id, out var slot))
            _ = RunAsync(slot);
    }

    /// <summary>
    /// Countdown loop of one entry. Running state: delay until the remaining
    /// deadline, cancellable by Pause. Paused state: indefinite wait released
    /// only by Resume/Release, cancellable by lifetime. Lifetime cancellation
    /// exits without dismissing again; a completed delay dismisses.
    /// </summary>
    private async Task RunAsync(Slot slot)
    {
        try
        {
            while (true)
            {
                CancellationTokenSource? countdown;
                Task wait;
                lock (slot)
                {
                    if (slot.Lifetime.IsCancellationRequested) return;

                    if (slot.Paused)
                    {
                        countdown = null;
                        wait = Task.WhenAny(slot.Wake.Task,
                            Task.Delay(Timeout.Infinite, slot.Lifetime.Token));
                    }
                    else
                    {
                        var remaining = slot.Remaining ?? ExpiresAtOf(slot.Entry) - DateTime.UtcNow;
                        if (remaining <= TimeSpan.Zero) break;
                        countdown = CancellationTokenSource.CreateLinkedTokenSource(slot.Lifetime.Token);
                        slot.Countdown = countdown;
                        wait = Task.Delay(remaining, countdown.Token);
                    }
                }

                try
                {
                    await wait;
                }
                catch (OperationCanceledException)
                {
                    if (slot.Lifetime.IsCancellationRequested) return;
                    // Paused mid-delay: re-evaluate state below.
                }

                lock (slot)
                {
                    if (slot.Lifetime.IsCancellationRequested) return;
                    if (countdown is not null && !countdown.IsCancellationRequested)
                        break; // delay ran to the deadline untouched
                }
            }

            await InvokeAsync(() => Dismiss(IdOf(slot.Entry)));
        }
        catch (OperationCanceledException) when (slot.Lifetime.IsCancellationRequested) { }
    }

    /// <summary>Pauses the expiry countdown, keeping the remaining time.</summary>
    protected void Pause(Guid id)
    {
        lock (_gate)
        {
            if (!_slots.TryGetValue(id, out var slot)) return;
            lock (slot)
            {
                var remaining = slot.Remaining ?? ExpiresAtOf(slot.Entry) - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) return;
                slot.Remaining = remaining;
                slot.Paused = true;
                CancelCountdown(slot);
                slot.SwapWake();
            }
        }
    }

    /// <summary>Resumes the countdown from the remaining time.</summary>
    protected void Resume(Guid id)
    {
        lock (_gate)
        {
            if (!_slots.TryGetValue(id, out var slot)) return;
            lock (slot)
            {
                if (!slot.Paused) return;
                if (slot.Remaining is { } remaining)
                    slot.Entry = WithExpiresAt(slot.Entry, DateTime.UtcNow + remaining);
                slot.Remaining = null;
                slot.Paused = false;
                slot.SwapWake();
            }
        }
    }

    /// <summary>Removes an entry (manual close, action or expiry) and stops its loop.</summary>
    protected void Dismiss(Guid id)
    {
        if (_disposed) return;
        lock (_gate)
        {
            if (_slots.Remove(id, out var slot)) Release(slot);
            _entries.RemoveAll(entry => IdOf(entry) == id);
        }
        StateHasChanged();
    }

    private static void CancelCountdown(Slot slot)
    {
        slot.Countdown?.Cancel();
        slot.Countdown?.Dispose();
        slot.Countdown = null;
    }

    private static void Release(Slot slot)
    {
        lock (slot)
        {
            CancelCountdown(slot);
            slot.Remaining = null;
            slot.Paused = false;
            slot.SwapWake();
        }
        slot.Lifetime.Cancel();
    }

    /// <summary>Unsubscribes and releases every pending countdown.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unsubscribe(_handler);
        lock (_gate)
        {
            foreach (var slot in _slots.Values) Release(slot);
            _slots.Clear();
            _entries.Clear();
        }
    }
}
