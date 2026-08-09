using System;
using System.Collections.Generic;

namespace Sufficit.Blazor.UI.Utilities;

/// <summary>
/// Tiny fluent CSS class builder. Drop-in replacement for the subset of
/// MudBlazor.Utilities.CssBuilder used by the SUI components.
/// </summary>
public sealed class SuiClassBuilder
{
    private readonly List<string> _classes = new();

    public static SuiClassBuilder Default(string? initial = null)
    {
        var builder = new SuiClassBuilder();
        if (!string.IsNullOrWhiteSpace(initial))
        {
            builder._classes.Add(initial.Trim());
        }
        return builder;
    }

    /// <summary>Adds a class when <paramref name="when"/> is true.</summary>
    public SuiClassBuilder AddClass(string? value, bool when)
    {
        if (when && !string.IsNullOrWhiteSpace(value))
        {
            _classes.Add(value.Trim());
        }
        return this;
    }

    /// <summary>Adds a class built from a callback, when <paramref name="when"/> is true.</summary>
    public SuiClassBuilder AddClass(string? value, Func<bool>? when = null)
        => AddClass(value, when is null || when());

    /// <summary>Merges another builder's result, when <paramref name="when"/> is true.</summary>
    public SuiClassBuilder AddClass(SuiClassBuilder? builder, bool when = true)
        => builder is null ? this : AddClass(builder.Build(), when);

    /// <summary>Adds a raw class only when the value is non-empty (no condition).</summary>
    public SuiClassBuilder Add(string? value)
        => AddClass(value);

    public string Build()
        => string.Join(' ', _classes).Trim();

    public override string ToString() => Build();

    /// <summary>
    /// Normalizes an enum-like value (SUI enum or a legacy MudBlazor enum
    /// passed through the compatibility bridge) into a lowercase CSS slug.
    /// Tolerates null (returns empty string). Used by components whose
    /// parameters are typed <c>object</c> to accept both enum families.
    /// </summary>
    public static string Slug(object? value)
        => value is null ? string.Empty : value.ToString()!.ToLowerInvariant();
}
