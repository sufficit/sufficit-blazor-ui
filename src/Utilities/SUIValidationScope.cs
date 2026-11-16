namespace Sufficit.Blazor.UI.Utilities;

/// <summary>Per-form validator registry; never shared across forms or scopes.</summary>
internal sealed class SUIValidationScope : IDisposable
{
    private readonly Dictionary<Guid, Func<bool, CancellationToken, Task<bool>>> _validators = new();
    private readonly Dictionary<Guid, bool> _validity = new();
    private readonly CancellationTokenSource _lifetime = new();
    internal event Action? Changed;
    internal bool Submitted { get; private set; }
    internal bool IsValid => _validity.Values.All(value => value);
    internal bool IsTouched { get; private set; }

    internal void Register(Guid id, Func<bool, CancellationToken, Task<bool>> validate)
        => _validators[id] = validate;

    internal void SetValidity(Guid id, bool valid, bool touched)
    {
        var changed = !_validity.TryGetValue(id, out var prior) || prior != valid;
        _validity[id] = valid;
        if (touched && !IsTouched) { IsTouched = true; changed = true; }
        if (changed) Changed?.Invoke();
    }

    internal void Unregister(Guid id)
    {
        _validators.Remove(id);
        if (_validity.Remove(id)) Changed?.Invoke();
    }

    internal async Task<bool> ValidateAsync()
    {
        Submitted = true;
        var token = _lifetime.Token;
        var valid = true;
        foreach (var validate in _validators.Values.ToArray())
        {
            if (token.IsCancellationRequested) return false;
            valid &= await validate(true, token);
        }
        Changed?.Invoke();
        return valid && IsValid;
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        _validators.Clear();
        _validity.Clear();
        Changed = null;
    }
}
