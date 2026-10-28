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
    /// part of the accessible name.
    /// </summary>
    internal static RenderFragment? OptionalMarker(bool optional, string? text)
        => !optional || string.IsNullOrWhiteSpace(text) ? null : builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "sui-field__optional");
            builder.AddContent(2, text);
            builder.CloseElement();
        };
}
