using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;

namespace Sufficit.Blazor.UI.Utilities;

/// <summary>Optional EditContext integration without changing component inheritance.</summary>
internal sealed class SUIFieldBinding<T> : IDisposable
{
    private EditContext? _context;
    private FieldIdentifier? _field;
    private ValidationMessageStore? _messages;
    private Action? _refresh;
    private string? _parseError;

    public string? Error => _parseError ?? (_field is { } identifier
        ? _context?.GetValidationMessages(identifier).FirstOrDefault() : null);

    public void Configure(EditContext? context, Expression<Func<T>>? expression, Action refresh)
    {
        var field = expression is null ? (FieldIdentifier?)null : FieldIdentifier.Create(expression);
        if (ReferenceEquals(context, _context) && Nullable.Equals(field, _field)) return;
        Dispose();
        _context = context;
        _field = field;
        _refresh = refresh;
        if (context is not null)
        {
            _messages = new(context);
            context.OnValidationStateChanged += ValidationChanged;
        }
    }

    public void Notify(string? parsingError = null)
    {
        _parseError = parsingError;
        _messages?.Clear();
        if (_context is not null && _field is { } field)
        {
            if (parsingError is not null) _messages!.Add(field, parsingError);
            _context.NotifyFieldChanged(field);
            _context.NotifyValidationStateChanged();
        }
    }

    private void ValidationChanged(object? sender, ValidationStateChangedEventArgs args) => _refresh?.Invoke();

    public void Dispose()
    {
        if (_context is not null) _context.OnValidationStateChanged -= ValidationChanged;
        _messages?.Clear();
        _context = null;
        _field = null;
        _messages = null;
        _parseError = null;
    }
}
