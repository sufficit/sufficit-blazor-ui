using System;
using System.Collections.Generic;
using System.Text;

namespace Sufficit.Blazor.UI.Utilities;

/// <summary>
/// Builds the value of the <c>data-sui-style</c> attribute: SUI custom
/// properties that the package's JS initializer
/// (<c>Sufficit.Blazor.UI.lib.module.js</c>) applies through CSSOM
/// (<c>element.style.setProperty</c>).
/// </summary>
/// <remarks>
/// An inline <c>style</c> attribute is ignored under a Content-Security-Policy with
/// <c>style-src-attr 'none'</c> (or a <c>style-src</c> without
/// <c>'unsafe-inline'</c>), both in prerendered HTML and when Blazor sets it
/// later. CSSOM writes are not governed by that directive, so component-computed
/// geometry (grid templates, widths, progress values) travels in this attribute
/// instead. Only <c>--sui-*</c> custom properties are accepted, on both sides,
/// so the attribute cannot be used to restyle arbitrary properties. Values are
/// validated against an allow-list of characters and CSS functions; anything
/// else is dropped, never escaped.
/// </remarks>
public sealed class SUIStyleBuilder
{
    /// <summary>Attribute read by the JS initializer.</summary>
    public const string AttributeName = "data-sui-style";

    private const int MaximumValueLength = 256;

    /// <summary>Functions a geometry value may call; none fetches a resource.</summary>
    private static readonly HashSet<string> AllowedFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "var", "calc", "min", "max", "clamp", "minmax", "repeat", "fit-content",
    };

    private readonly StringBuilder _value = new();

    /// <summary>Creates an empty builder.</summary>
    public static SUIStyleBuilder Default() => new();

    /// <summary>
    /// Adds <c>--sui-*</c> <paramref name="property"/> when <paramref name="value"/>
    /// passes <see cref="IsSafeValue"/>; otherwise the declaration is skipped.
    /// </summary>
    public SUIStyleBuilder Add(string property, string? value, bool when = true)
    {
        if (when && IsSafeProperty(property) && IsSafeValue(value))
            Append(property, value!.Trim());
        return this;
    }

    /// <summary>
    /// Adds an image custom property (name ending in <c>-image</c>) as
    /// <c>url("…")</c> for an http(s), data or relative URL without quotes,
    /// parentheses, backslashes, separators or whitespace.
    /// </summary>
    public SUIStyleBuilder AddUrl(string property, string? url)
    {
        if (!IsSafeProperty(property) || !property.EndsWith("-image", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(url) || url.Length > 2048
            || url.IndexOfAny(['"', '\'', '(', ')', '\\', ';', '{', '}', '<', '>', '@']) >= 0)
            return this;
        foreach (var character in url)
            if (char.IsControl(character) || char.IsWhiteSpace(character)) return this;
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri)
            || (uri.IsAbsoluteUri && uri.Scheme is not ("https" or "http" or "data")))
            return this;
        Append(property, $"url(\"{url}\")");
        return this;
    }

    /// <summary>The attribute value, or <see langword="null"/> when nothing was added.</summary>
    public string? Build() => _value.Length == 0 ? null : _value.ToString();

    /// <summary>Whether <paramref name="property"/> is a <c>--sui-</c> custom property name.</summary>
    public static bool IsSafeProperty(string? property)
    {
        if (property is null || property.Length <= 6 || !property.StartsWith("--sui-", StringComparison.Ordinal))
            return false;
        foreach (var character in property)
            if (character is not ('-' or (>= 'a' and <= 'z') or (>= '0' and <= '9'))) return false;
        return true;
    }

    /// <summary>
    /// Whether <paramref name="value"/> is a geometry/token value safe to apply:
    /// letters, digits, spaces and <c>. % # - + * / , ( ) [ ]</c> only, balanced
    /// parentheses, no comments, and only <c>var</c>, <c>calc</c>, <c>min</c>,
    /// <c>max</c>, <c>clamp</c>, <c>minmax</c>, <c>repeat</c> and
    /// <c>fit-content</c> as functions.
    /// </summary>
    public static bool IsSafeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumValueLength) return false;
        var depth = 0;
        foreach (var character in value)
        {
            if (char.IsAsciiLetterOrDigit(character) || character is ' ' or '.' or '%' or '#' or '-' or '+' or '*' or '/' or ',' or '[' or ']' or '_')
                continue;
            if (character == '(') { if (++depth > 8) return false; continue; }
            if (character == ')') { if (--depth < 0) return false; continue; }
            return false;
        }
        if (depth != 0 || value.Contains("/*", StringComparison.Ordinal) || value.Contains("*/", StringComparison.Ordinal))
            return false;
        return CallsOnlyAllowedFunctions(value);
    }

    private static bool CallsOnlyAllowedFunctions(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '(') continue;
            var start = index;
            while (start > 0 && (char.IsAsciiLetter(value[start - 1]) || value[start - 1] == '-')) start--;
            var name = value[start..index];
            // A bare parenthesis is grouping inside calc(); anything named must be listed.
            if (name.Length > 0 && !AllowedFunctions.Contains(name)) return false;
        }
        return true;
    }

    private void Append(string property, string value)
    {
        if (_value.Length > 0) _value.Append(';');
        _value.Append(property).Append(':').Append(value);
    }
}
