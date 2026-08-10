# Adoção SUI no Sufficit.Identity.UI.Vault + remoção dos componentes antigos

**Data:** 2026-08-09
**Status:** concluída, build verde (solução identity completa)
**Plano de origem:** [`PLAN-CONSUMER-MIGRATION.md`](../PLAN-CONSUMER-MIGRATION.md)

## Contexto

O `Sufficit.Identity.UI.Vault` era o último consumidor dos componentes antigos
do projeto `Sufficit.Identity.UI.Components` (`AppIcon`, `EmptyState`,
`PageHeader`, `StatusBadge`). Esses componentes tinham equivalentes SUI diretos
já promovidos à biblioteca central, mas não podiam ser removidos enquanto o
Vault os usasse — pendência documentada na migração do Management. Esta entrega
fecha esse ciclo: migra o Vault e apaga os componentes antigos.

## Entrega

### Vault migrado

Projeto pequeno (4 componentes reais), migração direta:

- **`App.razor`** — `<link sufficit-ui.css>` adicionado; `<SUIThemeProvider>` a
  envolver `<Routes>`.
- **`_Imports.razor`** — `@using Sufficit.Identity.UI.Components.Common`
  substituído por `@using Sufficit.Blazor.UI.Components` +
  `@using Sufficit.Blazor.UI.Themes`.
- **`ServiceCollectionExtensions.cs`** — `AddSufficitUI()` registado em
  `AddSufficitIdentityVaultUI` (tema default azul, `DefaultSUITheme`).
- **`csproj`** — `ProjectReference` directa ao `sufficit-blazor-ui`.
- **14 ocorrências renomeadas** em `VaultLayout.razor`, `AdminVault.razor`,
  `UserVault.razor`, `Routes.razor`:
  - `<AppIcon>` → `<SUIIcon>` (5)
  - `<PageHeader>` → `<SUIPageHeader>` (2)
  - `<StatusBadge>` → `<SUIStatusBadge>` (5)
  - `<EmptyState>` → `<SUIEmptyState>` (3, incluindo o `NotFound` do `Routes`)

Os formulários `EditForm`/`InputText` (framework) e as classes `vault-*`
(CSS próprio) não foram tocados — não há MudBlazor neste projeto.

### Componentes antigos removidos

Apagados de `Sufficit.Identity.UI.Components/Components/Common/`:
`AppIcon.razor`, `EmptyState.razor`, `PageHeader.razor`, `StatusBadge.razor`.
O diretório `Common/` foi removido e o `_Imports.razor` do Management perdeu o
`@using Sufficit.Identity.UI.Components.Common` correspondente.

O projeto `Sufficit.Identity.UI.Components` fica agora com um único ficheiro
(`_Imports.razor`) e a referência ao `sufficit-blazor-ui` — é um mero adapter
de namespace se for preciso no futuro.

## Não reivindicado

- **`Sufficit.Identity.UI` (pública)** ainda não adota SUI — é o único
  consumidor pendente no plano de migração.
- **Verificação visual.** Tudo por compilação.

## Validação

- `dotnet build Sufficit.Identity.sln -c Release` — **14 projetos** (incluindo
  o Vault, o Management, o Components e os testes), **0 warnings, 0 erros**.
- `grep` por `<AppIcon`/`<EmptyState`/`<PageHeader`/`<StatusBadge` em todo o
  identity: **zero** ocorrências (nomes antigos extintos).
