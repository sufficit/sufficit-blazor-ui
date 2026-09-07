using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Demos;

public abstract class DemoBase : ComponentBase
{
    [Inject] protected ISUISnackbar Snackbar { get; set; } = default!;
    [Inject] protected ISUIDialogService Dialogs { get; set; } = default!;
    protected string? Text { get; set; } = "Operação Sudeste";
    protected int Count { get; set; } = 3;
    protected bool Enabled { get; set; } = true;
    protected bool Open { get; set; } = true;
    protected string? Region { get; set; } = "sul";
    protected DateOnly? Date { get; set; } = new DateOnly(2026, 9, 7);
    protected int PageIndex { get; set; }
    protected int PageSize { get; set; } = 10;
    protected int Step { get; set; } = 1;
    protected string[] Steps { get; } = ["Configurar", "Revisar", "Concluir"];
    protected string[] Cities { get; } = ["São Paulo", "Rio de Janeiro", "Curitiba", "Porto Alegre"];
    protected string[] Rows { get; } = ["Telefonia", "Provisionamento", "Atendimento"];
    protected void Notify() => Snackbar.Success("Exemplo executado com sucesso.");
    protected void Reset() { Text = "Operação Sudeste"; Snackbar.Info("Alterações descartadas."); }
    protected async Task Confirm() => Snackbar.Info(await Dialogs.ConfirmAsync("Confirmar alteração", "Aplicar a configuração de demonstração?") ? "Confirmado." : "Cancelado.");
    protected async Task Decide()
    {
        var reference = await Dialogs.ShowAsync<SUIDecisionDialog>("Salvar alterações", new Dictionary<string, object?>
        {
            ["Message"] = "Deseja salvar antes de sair?", ["PrimaryText"] = "Salvar", ["SecondaryText"] = "Descartar",
        });
        var result = await reference.Result;
        Snackbar.Info(result is true ? "Salvo." : result is false ? "Descartado." : "Cancelado.");
    }
    protected async Task<IEnumerable<string>> Search(string text, CancellationToken token)
    {
        await Task.Delay(150, token);
        return Cities.Where(city => city.Contains(text, StringComparison.OrdinalIgnoreCase));
    }
}
