# Adoção SUI no `sufficit-cloud-mobile` — concluída

**Data:** 2026-08-09
**Status:** **CONCLUÍDA**. MudBlazor removido por completo; biblioteca SUI
expandida para cobertura total; 9 páginas + 3 shared + shell migrados. Build
verde limpo (sem MudBlazor).
**Plano de origem:** [`PLAN-CONSUMER-MIGRATION.md`](../PLAN-CONSUMER-MIGRATION.md)

## Contexto

O `sufficit-cloud-mobile` era o consumidor mais exigente: tinha ~202 tags `<Mud*>`
diretas em páginas (tabelas, tabs, timeline, formulários) além de 40 wrappers
`Suff*`. Diferentemente do Identity (componentes isolados), aqui as páginas
misturam tudo — não havia migração componente-a-componente possível. Decidiu-se
**construir a biblioteca completa primeiro, migrar no fim**. Tema: azul default
(`DefaultSUITheme`, `#2563eb`).

## Entrega

### Biblioteca SUI expandida no `sufficit-blazor-ui` (+33 componentes)

A biblioteca passou de ~11 para **44 componentes** + 4 serviços, cobrindo toda
a superfície MudBlazor do cloud-mobile:

| Categoria | Componentes |
| --- | --- |
| Primitivos | `SUIText`, `SUIAlert`, `SUICard`, `SUIStack`, `SUISpacer`, `SUIDivider` |
| Layout | `SUIGrid`+`SUIItem`, `SUILayout`, `SUIAppBar`, `SUIDrawer`, `SUIContainer` |
| Dados | `SUITable`+`SUITh`+`SUITd`, `SUITabs`+`SUITabPanel`, `SUITimeline`+`SUITimelineItem`, `SUIList`+`SUIListItem`, `SUIProgressLinear` |
| Feedback | `SUIAlert`, `SUIChip`, `SUIStatusBadge`, `SUILink` |
| Forms | `SUITextField`, `SUINumericField`, `SUISelect`+`SUISelectItem`, `SUISwitch`, `SUIAutocomplete` (debounce + templates) |
| Serviços | `ISUISnackbar`/`SUISnackbarService` + `SUISnackbarHost`, `ISUIDialogService`/`SUIDialogService` + `SUIDialogHost` + `SUIConfirmDialog` |
| Ícones | `SUIIcons` expandido: Menu, Logout, Add, Play, Stop, Restart, Cast, Delete, Phone, Dashboard, Pending, Admin, Category, Storage, DataUsage |

### Cloud-mobile migrado por completo

- **`MainLayout.razor`** — providers MudBlazor → `SUIThemeProvider` +
  `SUIDialogHost` + `SUISnackbarHost`; `MudLayout/AppBar/Drawer/Container` → SUI.
- **`NavMenu.razor`** — wrappers `SuffNavLink/SuffNavGroup` → `SUINavLink/SUINavGroup`.
- **9 páginas** (`Home`, `Instances`, `InstanceDetail`, `InstanceNew`,
  `Operations`, `OperationDetail`, `AdminCapacity`, `AdminInstanceTypes`,
  `AdminQuotas`) — todas as ~202 tags `<Mud*>` reescritas para SUI.
- **3 shared** (`ConfirmDeleteDialog`, `OperationStatusChip`, `QuotaMeter`) —
  incluindo reescrita do `ConfirmDeleteDialog` de `MudDialog`/`IMudDialogInstance`
  para o padrão SUI (`SUIDialogReference` via cascade).
- **Serviços** — `IDialogService`/`ISnackbar` (MudBlazor) → `ISUIDialogService`/
  `ISUISnackbar` em todas as páginas.
- **Métodos públicos partilhados** (`Home.StateColor`, `Home.OperationLabel`)
  migrados de `Color` (enum MudBlazor) para `string` (slug aceito pelos SUI).

### MudBlazor removido

- 8 wrappers `Suff*.razor` (+ `.razor.cs`, `.cs`) apagados; diretório `Ui/` removido.
- `<PackageReference MudBlazor>` removido do csproj.
- `AddMudServices()` removido do `Program.cs`.
- `<link MudBlazor.min.css>` e `<script MudBlazor.min.js>` removidos do `App.razor`.
- `@using MudBlazor` removido do `_Imports.razor`.
- `app.css` apagado (classes `mud-*`/`table-no-records`/`empty-state` substituídas
  pelo `sufficit-ui.css`).

## Decisões de design

- **Enums via strings.** O cloud-mobile calcula cores em runtime (`StateColor()`,
  `StepColor()`, `ChipColor()`). Em vez de forçar enums SUI tipados, os
  componentes (`SUIChip`, `SUIAlert`, `SUIStatusBadge`, etc.) aceitam `string`
  lowercase ("success", "error", ...) — a migração preserva os helpers existentes
  mudando só o tipo de retorno de `Color` para `string`.
- **`SUIAutocomplete.SearchFunc` sem CancellationToken.** A API SUI usa
  `Func<string, Task<IEnumerable<T>>>` (sem token); o `AdminQuotas` envolve o
  método local com `CancellationToken.None` num lambda.
- **`ConfirmDeleteDialog` reescrito.** O padrão `IMudDialogInstance` +
  `DialogResult.Ok` foi substituído por `SUIDialogReference` via `[CascadingParameter]`
  + `Dialog.Complete(true/false)`. O `ConfirmAsync` do serviço SUI retorna `bool`.

## Não reivindicado

- **Verificação visual.** Tudo é validado por compilação. Os componentes novos
  (Tabs, Timeline, Table, Autocomplete, Dialog, Snackbar) e o tema azul default
  (antes laranja) precisam de inspeção em navegador.
- **`SUIConfirmDialog` dentro de `SUIDialogHost`** — o `DynamicComponent` com
  `CascadingValue` não foi testado em runtime; pode haver detalhe de registo do
  dialog que só se revela em execução.

## Validação

- `dotnet build` do `sufficit-blazor-ui` — 0 warnings, 0 erros.
- `dotnet build` do `Sufficit.Cloud.Mobile.Web` (limpo, sem MudBlazor) —
  0 warnings, 0 erros.
- `grep` por `MudBlazor`/`<Mud`/`Icons.Material`/`ISnackbar`/`IDialogService`/
  `AddMudServices` em source: **zero** ocorrências funcionais (matches residuais
  são substrings de `ISUISnackbar`).
- `PackageReference` final: só `OpenIdConnect` (auth) + `ProjectReference` ao SUI.
