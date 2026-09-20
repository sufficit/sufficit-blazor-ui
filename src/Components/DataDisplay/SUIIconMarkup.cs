using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Validates the inline SVG fragment that icon parameters accept, so a value
/// reaching an icon slot cannot introduce script or fetch a remote resource.
/// </summary>
/// <remarks>
/// Every icon parameter in this library is a <see cref="string"/> that may hold
/// either a short name resolved by <c>SUIIcon</c> or a raw SVG fragment, and the
/// fragment is rendered through <see cref="MarkupString"/>. That makes roughly a
/// dozen parameters — <c>Icon</c>, <c>StartIcon</c>, <c>EndIcon</c>,
/// <c>AdornmentIcon</c>, <c>ExpandIcon</c>, <c>LoadingIcon</c>,
/// <c>IconPath</c> — an HTML sink. An application that builds a menu, a tab
/// strip or a category list from a table with an <c>icon</c> column turns that
/// column into stored cross-site scripting: inline SVG executes
/// <c>&lt;script&gt;</c>, and <c>&lt;image onerror&gt;</c> or
/// <c>&lt;foreignObject&gt;</c> reach the same place by other routes.
///
/// The parameters stay <see cref="string"/> on purpose. Consumers pass icons in
/// more than two thousand places, most of them constants from this library or
/// from a third-party icon set, and none of them pass a literal fragment; a type
/// change would rewrite every one of those call sites for no security gain over
/// validating the value here. A typed icon reference remains the better end
/// state, and this validation is what makes introducing it a refactor rather
/// than a security fix.
///
/// The allow lists below are not guesses: they are the union of what this
/// library's own <see cref="SUIIcons"/> constants and the 10,667 icon constants
/// of the icon set consumers use actually contain. Anything outside them is
/// rejected rather than stripped, because a fragment that needs an unexpected
/// element is more likely to be an attack than a drawing.
/// </remarks>
public static class SUIIconMarkup
{
    /// <summary>Longest accepted fragment. The largest real icon is far below this.</summary>
    private const int MaximumLength = 20_000;

    /// <summary>
    /// SVG shape and grouping elements. No <c>script</c>, <c>style</c>,
    /// <c>image</c>, <c>foreignObject</c>, <c>a</c>, <c>animate</c> or
    /// <c>set</c>: each of those either runs code, loads a resource, or escapes
    /// the drawing into interactive content.
    /// </summary>
    private static readonly HashSet<string> AllowedElements = new(StringComparer.Ordinal)
    {
        "path", "circle", "ellipse", "rect", "line", "polyline", "polygon",
        "g", "defs", "clipPath", "use", "title", "desc", "mask", "symbol",
        "linearGradient", "radialGradient", "stop", "pattern",
    };

    /// <summary>
    /// Presentation attributes. Event handlers are absent by construction: any
    /// name starting with <c>on</c> is rejected before this set is consulted.
    /// </summary>
    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.Ordinal)
    {
        "d", "cx", "cy", "r", "rx", "ry", "x", "y", "x1", "x2", "y1", "y2",
        "width", "height", "points", "transform", "id", "class",
        "fill", "fill-opacity", "fill-rule", "clip-path", "clip-rule", "mask",
        "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin",
        "stroke-opacity", "stroke-dasharray", "stroke-dashoffset", "stroke-miterlimit",
        "opacity", "display", "visibility", "overflow", "enable-background",
        "offset", "stop-color", "stop-opacity", "gradientUnits", "gradientTransform",
        "patternUnits", "maskUnits", "clipPathUnits", "viewBox", "preserveAspectRatio",
        "style", "xmlns:xlink", "href", "xlink:href", "vector-effect", "paint-order",
    };

    /// <summary>
    /// Whether a value looks like inline markup rather than a short icon name.
    /// </summary>
    /// <param name="value">Icon parameter value.</param>
    /// <returns><see langword="true"/> when the value starts a tag.</returns>
    public static bool IsMarkup(string? value)
        => value is not null && value.AsSpan().TrimStart().StartsWith("<");

    /// <summary>
    /// Whether an inline SVG fragment is safe to render.
    /// </summary>
    /// <param name="markup">Fragment such as <c>&lt;path d="…"/&gt;</c>.</param>
    /// <returns>
    /// <see langword="true"/> when every element and attribute is allowed and no
    /// value references an external resource or a script URL.
    /// </returns>
    public static bool IsSafe(string? markup)
    {
        if (string.IsNullOrWhiteSpace(markup)) return false;
        if (markup.Length > MaximumLength) return false;

        var index = 0;
        var depth = 0;

        while (index < markup.Length)
        {
            var next = markup.IndexOf('<', index);
            if (next < 0)
            {
                // Trailing text. Only whitespace may sit between elements; real
                // text content would render as a label inside the icon.
                return IsBlank(markup.AsSpan(index)) && depth == 0;
            }

            if (!IsBlank(markup.AsSpan(index, next - index))) return false;
            if (!ReadTag(markup, ref next, ref depth)) return false;
            index = next;
        }

        return depth == 0;
    }

    /// <summary>
    /// Returns the fragment as renderable markup, or an empty fragment when it
    /// fails validation.
    /// </summary>
    /// <param name="markup">Fragment supplied through an icon parameter.</param>
    /// <returns>
    /// The validated markup. A rejected fragment renders as nothing, which
    /// leaves the icon slot empty rather than failing the whole component:
    /// losing a glyph is recoverable, executing a payload is not.
    /// </returns>
    public static MarkupString Render(string? markup)
        => IsSafe(markup) ? new MarkupString(markup!) : default;

    private static bool IsBlank(ReadOnlySpan<char> span)
    {
        foreach (var character in span)
            if (!char.IsWhiteSpace(character)) return false;
        return true;
    }

    /// <summary>
    /// Reads one tag starting at <paramref name="index"/>, advancing it past the
    /// closing angle bracket and tracking element nesting in
    /// <paramref name="depth"/>.
    /// </summary>
    private static bool ReadTag(string markup, ref int index, ref int depth)
    {
        var position = index + 1;
        if (position >= markup.Length) return false;

        // Comments, CDATA, doctypes and processing instructions have no place in
        // an icon and are a convenient way to hide a payload from a reviewer.
        if (markup[position] is '!' or '?') return false;

        var closing = markup[position] == '/';
        if (closing) position++;

        var nameStart = position;
        while (position < markup.Length && (char.IsLetterOrDigit(markup[position]) || markup[position] == '-'))
            position++;

        var name = markup[nameStart..position];
        if (name.Length == 0 || !AllowedElements.Contains(name)) return false;

        if (closing)
        {
            if (--depth < 0) return false;
            while (position < markup.Length && char.IsWhiteSpace(markup[position])) position++;
            if (position >= markup.Length || markup[position] != '>') return false;
            index = position + 1;
            return true;
        }

        var selfClosing = false;
        while (true)
        {
            while (position < markup.Length && char.IsWhiteSpace(markup[position])) position++;
            if (position >= markup.Length) return false;

            if (markup[position] == '/')
            {
                selfClosing = true;
                position++;
                while (position < markup.Length && char.IsWhiteSpace(markup[position])) position++;
                if (position >= markup.Length || markup[position] != '>') return false;
            }

            if (markup[position] == '>')
            {
                position++;
                break;
            }

            if (!ReadAttribute(markup, ref position)) return false;
        }

        if (!selfClosing) depth++;
        index = position;
        return true;
    }

    private static bool ReadAttribute(string markup, ref int position)
    {
        var nameStart = position;
        while (position < markup.Length
               && (char.IsLetterOrDigit(markup[position]) || markup[position] is '-' or ':'))
            position++;

        var name = markup[nameStart..position];
        if (name.Length == 0) return false;
        // Every scripting attribute starts with "on"; rejecting the prefix keeps
        // the check correct as the event list grows.
        if (name.StartsWith("on", StringComparison.OrdinalIgnoreCase)) return false;
        if (!AllowedAttributes.Contains(name)) return false;

        while (position < markup.Length && char.IsWhiteSpace(markup[position])) position++;
        // A valueless attribute is harmless but not something an icon needs.
        if (position >= markup.Length || markup[position] != '=') return false;
        position++;
        while (position < markup.Length && char.IsWhiteSpace(markup[position])) position++;
        if (position >= markup.Length) return false;

        var quote = markup[position];
        if (quote is not ('"' or '\'')) return false;
        position++;

        var valueStart = position;
        while (position < markup.Length && markup[position] != quote) position++;
        if (position >= markup.Length) return false;

        var value = markup[valueStart..position];
        position++;

        return IsSafeAttributeValue(name, value);
    }

    private static bool IsSafeAttributeValue(string name, string value)
    {
        foreach (var character in value)
            if (character is '<' or '>' || char.IsControl(character)) return false;

        // A reference may only point inside the same fragment. An absolute URL
        // here would both leak that the page rendered and, with a data: or
        // javascript: scheme, run code.
        if (name is "href" or "xlink:href")
            return value.StartsWith('#') && value.Length > 1;

        if (name == "style")
        {
            // The same reasoning as the theme tokens: no fetching, no at-rules,
            // no escapes that rebuild a blocked character.
            if (value.Contains("url(", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Contains("expression", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Contains('@') || value.Contains('\\')) return false;
            if (value.Contains("/*", StringComparison.Ordinal)) return false;
        }

        // No attribute on a drawing element legitimately carries a scheme.
        return !value.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("data:", StringComparison.OrdinalIgnoreCase);
    }
}
