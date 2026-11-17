using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Accessible single or multiple-file selection; never uploads or opens a stream automatically.</summary>
public partial class SUIFileUpload
{
    private readonly string _generatedId = $"sui-file-{Guid.NewGuid():N}";
    private IBrowserFile? _previousValue;
    private int _inputVersion;
    private int _previousFilesCount;
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
    /// <summary>Allows a native multiple-file selection. Use Files/FilesChanged in this mode.</summary>
    [Parameter] public bool Multiple { get; set; }
    /// <summary>Files from the latest native selection, never implicitly appended. Clear with an empty list.</summary>
    [Parameter] public IReadOnlyList<IBrowserFile> Files { get; set; } = [];
    /// <summary>Reports the accepted files of a multiple selection without opening their streams.</summary>
    /// <remarks>Browser file handles belong to their native input selection. Consume or copy them before selecting again; the caller owns buffering and uploads.</remarks>
    [Parameter] public EventCallback<IReadOnlyList<IBrowserFile>> FilesChanged { get; set; }
    /// <summary>Maximum files per multiple selection, at least one. Count overflow rejects the entire selection.</summary>
    [Parameter] public int MaxFiles { get; set; } = 10;
    /// <summary>Localized count rejection message.</summary>
    [Parameter] public string TooManyFilesText { get; set; } = "O número de arquivos excede o limite permitido.";
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

    private string SelectionSummary => !Multiple ? Value?.Name ?? EmptyText
        : Files is { Count: 1 } ? Files[0].Name
        : Files is { Count: > 1 } ? $"{Files.Count} arquivos selecionados." : EmptyText;

    private string EffectiveId => Id ?? _generatedId;
    private string? EffectiveError => ErrorText ?? _selectionError;
    private bool HasError => !string.IsNullOrWhiteSpace(EffectiveError);
    private string DescribedBy => $"{EffectiveId}-selection"
        + (string.IsNullOrWhiteSpace(HelperText) ? "" : $" {EffectiveId}-helper")
        + (HasError ? $" {EffectiveId}-error" : "");

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Multiple && MaxFiles < 1) throw new ArgumentOutOfRangeException(nameof(MaxFiles), "At least one file must be allowed.");
        if ((!Multiple && Value == null && _previousValue != null) || (Multiple && (Files?.Count ?? 0) == 0 && _previousFilesCount > 0))
        {
            _inputVersion++;
            _selectionError = null;
        }
        _previousValue = Value;
        _previousFilesCount = Files?.Count ?? 0;
    }

    private async Task ChooseFile(InputFileChangeEventArgs args)
    {
        if (Disabled) return;
        if (Multiple)
        {
            if (args.FileCount > MaxFiles)
            {
                _selectionError = TooManyFilesText;
                Files = []; _previousFilesCount = 0; _inputVersion++;
                await FilesChanged.InvokeAsync(Files);
                return;
            }
            var chosen = args.GetMultipleFiles(MaxFiles);
            Files = chosen.Where(file => !MaxFileSize.HasValue || file.Size <= MaxFileSize.Value).ToArray();
            _selectionError = Files.Count == chosen.Count ? null : FileTooLargeText;
            _previousFilesCount = Files.Count;
            if (Files.Count == 0 && chosen.Count > 0) _inputVersion++;
            await FilesChanged.InvokeAsync(Files);
            return;
        }
        var file = args.File;
        _selectionError = MaxFileSize.HasValue && file.Size > MaxFileSize.Value ? FileTooLargeText : null;
        Value = _selectionError == null ? file : null;
        _previousValue = Value;
        if (_selectionError != null) _inputVersion++;
        await ValueChanged.InvokeAsync(Value);
    }
}
