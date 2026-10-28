using Microsoft.AspNetCore.Components;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Label, helper, warning and unit slots of the numeric field.</summary>
public partial class SUINumericField<TValue>
{
    /// <summary>Appends a secondary <see cref="OptionalText"/> marker to the label instead of the caller editing the label text.</summary>
    [Parameter] public bool Optional { get; set; }

    /// <summary>Marker shown when <see cref="Optional"/> is set. Default "(opcional)".</summary>
    [Parameter] public string OptionalText { get; set; } = SUIFieldParts.DefaultOptionalText;

    /// <summary>
    /// Unit shown inside the field at the inline end (e.g. "min", "dias", "h")
    /// and announced through <c>aria-describedby</c>. Short units only.
    /// </summary>
    [Parameter] public string? Suffix { get; set; }

    /// <summary>Markup label; takes precedence over <see cref="Label"/>.</summary>
    [Parameter] public RenderFragment? LabelContent { get; set; }

    /// <summary>Markup helper content; takes precedence over <see cref="HelperText"/>.</summary>
    [Parameter] public RenderFragment? HelperContent { get; set; }

    /// <summary>Non-blocking warning in the warning tone, linked by <c>aria-describedby</c>; never sets <c>aria-invalid</c>.</summary>
    [Parameter] public string? WarningText { get; set; }

    private string SuffixId => $"{EffectiveId}-suffix";
    // Kept as the original literal when no suffix, so default markup is unchanged.
    private string ControlClass
        => (SpinnerStep is not null ? "sui-numeric__control sui-numeric__control--steppers" : "sui-numeric__control ")
            + (string.IsNullOrWhiteSpace(Suffix) ? null : " sui-numeric__control--suffix");
}
