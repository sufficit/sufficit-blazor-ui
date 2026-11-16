using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Accessible single-file selection; never uploads or opens a stream automatically.</summary>
public partial class SUIFileUpload
{
    private readonly string _generatedId = $"sui-file-{Guid.NewGuid():N}";
    private IBrowserFile? _previousValue;
    private int _inputVersion;
    private string? _selectionError;

    /// <summary>Optional native input ID, also used by the associated label.</summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>Accessible, visible field label.</summary>
    [Parameter] public string Label { get; set; } = "Arquivo";
    /// <summary>Text on the file chooser; the native input retains keyboard interaction.</summary>
    [Parameter] public string ChooseText { get; set; } = "Escolher arquivo";
    /// <summary>Text shown when no file is selected.</summary>
    [Parameter] public string EmptyText { get; set; } = "Nenhum arquivo selecionado.";
    /// <summary>Native accept hint; the server must still validate the selected file.</summary>
    [Parameter] public string? Accept { get; set; }
    /// <summary>Selected browser file. Set to null to clear the native input.</summary>
    [Parameter] public IBrowserFile? Value { get; set; }
    /// <summary>Reports selection or null when an oversized file is rejected.</summary>
    [Parameter] public EventCallback<IBrowserFile?> ValueChanged { get; set; }
    /// <summary>Prevents selecting a file.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Optional client size limit in bytes, independent of server validation.</summary>
    [Parameter] public long? MaxFileSize { get; set; }
    /// <summary>Localized size rejection message.</summary>
    [Parameter] public string FileTooLargeText { get; set; } = "O arquivo excede o limite permitido.";
    /// <summary>Supplementary instructions linked to the native input.</summary>
    [Parameter] public string? HelperText { get; set; }
    /// <summary>External validation message; takes precedence over size rejection.</summary>
    [Parameter] public string? ErrorText { get; set; }

    private string EffectiveId => Id ?? _generatedId;
    private string? EffectiveError => ErrorText ?? _selectionError;
    private bool HasError => !string.IsNullOrWhiteSpace(EffectiveError);
    private string DescribedBy => $"{EffectiveId}-selection"
        + (string.IsNullOrWhiteSpace(HelperText) ? "" : $" {EffectiveId}-helper")
        + (HasError ? $" {EffectiveId}-error" : "");

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Value == null && _previousValue != null)
        {
            _inputVersion++;
            _selectionError = null;
        }
        _previousValue = Value;
    }

    private async Task ChooseFile(InputFileChangeEventArgs args)
    {
        if (Disabled) return;
        var file = args.File;
        _selectionError = MaxFileSize.HasValue && file.Size > MaxFileSize.Value ? FileTooLargeText : null;
        Value = _selectionError == null ? file : null;
        _previousValue = Value;
        if (_selectionError != null) _inputVersion++;
        await ValueChanged.InvokeAsync(Value);
    }
}
