using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Small markup pieces shared by the field components.</summary>
internal static class SUIFieldParts
{
    /// <summary>Default suffix of <c>Optional</c> fields.</summary>
    internal const string DefaultOptionalText = "(opcional)";

    /// <summary>
    /// The secondary "(optional)" marker appended inside a field label, or
    /// nothing when the field is not optional. Being inside the label, it is
    /// part of the accessible name. A real space precedes the marker, so the
    /// label reads "Name (optional)" both on screen and to assistive technology
    /// without callers padding their text; HTML whitespace collapsing keeps a
    /// caller's own leading space from doubling the gap.
    /// </summary>
    internal static RenderFragment? OptionalMarker(bool optional, string? text)
        => !optional || string.IsNullOrWhiteSpace(text) ? null : builder =>
        {
            builder.AddContent(0, " ");
            builder.OpenElement(1, "span");
            builder.AddAttribute(2, "class", "sui-field__optional");
            builder.AddContent(3, text);
            builder.CloseElement();
        };
}
