# Consolidação de design — F5/P3: motor único de feedback + generics TValue/TItem

**Data:** 2026-10-05  
**Branch:** `work/design-consolidation`  
**Escopo:** avaliação `docs/evaluations/EVALUATION-2026-10-04-sufficit-ai-genius.md` (F5, P3)

## F5 — Snackbar e Toast num motor só

**Antes:** `SUISnackbarHost` e `SUIToastHost` duplicavam a mesma máquina —
lista com teto, `Task.Delay` até `ExpiresAt`, dismiss, teardown com
`CancellationTokenSource` — e nenhum dos dois pausava o cronômetro sob
hover/foco (gap de acessibilidade WCAG 2.2.1 apontado na avaliação).

**Depois:** `SUINotificationHostBase<TEntry>` (público para satisfazer a
acessibilidade do Razor nos hosts) centraliza fila/expiração/dismiss/teardown
e o requisito de pausa:

- cada entrada vive num `Slot` com estado (`Paused`, `Remaining`,
  `Countdown` CTS vinculado ao lifetime, `Wake` TCS);
- `Pause(id)` congela o cronômetro guardando o tempo restante; `Resume(id)`
  retoma **do tempo restante**, recalculando `ExpiresAt` (não do zero);
- o loop `RunAsync` arma exatamente uma espera por iteração — delay até o
  prazo (cancelável pela pausa) ou espera indefinida liberada apenas por
  resume/teardown — sem polling;
- `SUIToast` ganhou `OnPause`/`OnResume` (hover/foco no próprio toast);
  snackbar aplica os handlers direto na entrada.

Interfaces públicas `ISUISnackbar`/`ISUIToast` e assinaturas de `Add` não
mudaram. Testes bUnit (`RenderContractFeedbackTests`) cobrem pausa por hover
e por foco nos dois hosts, provando que a mensagem sobrevive além do prazo
enquanto pausada e some pouco depois do resume.

## P3 — Generics consistentes

`SUISelect`/`SUITextField`/`SUINumericField`/`SUIAutocomplete`: `T` →
`TValue` (componentes de valor); `SUITable`/`SUITableSortLabel`: `T` →
`TItem` (coleções). Marcado na avaliação como quebra de fonte deliberada,
janela aberta nesta onda: `eng/PublicApiBaseline.txt` regenerado
(`SUISelect`0:TValue`` etc.), todos os `T="…"` de markup em samples/docs/
README migrados e um bug de exemplo corrigido de passagem (`SUISelectItem`
não é genérico — o `T="string"` no README era atributo fantasma).

`NamingConventionTests.GenericComponents_UseValueOrItemTypeParameterNames`
impede que o próximo componente genético reintroduza um `T` solto.

## Validação

- Suíte unitária: **933/933** verdes (929 + 3 de pausa/resume + 1 de naming).
- `dotnet build` da solução completa (`Sufficit.Blazor.UI.slnx`): 0 erros,
  0 warnings.
- Catálogo de componentes regenerado (73 componentes; `generate-catalog.py`).
- Baseline de API pública revisado via `SUI_UPDATE_PUBLIC_API=1`.
