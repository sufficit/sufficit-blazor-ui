# Adoção SUI no Sufficit.Identity.UI.Management + contrato de temas

**Data:** 2026-08-09
**Status:** concluída, build verde (solução completa do identity)
**Plano de origem:** [`PLAN-CONSUMER-MIGRATION.md`](../PLAN-CONSUMER-MIGRATION.md)

## Contexto

Primeira adoção da biblioteca SUI num consumidor que **não** vinha do MudBlazor.
O `Sufficit.Identity.UI.Management` tem UI própria em CSS feito à mão (5.013
linhas em `app.css`), identidade visual vermelha (`--brand: #cc0000`), fonte
Inter e só tema claro. A biblioteca SUI, por sua vez, codificava valores fixos
azuis sem contrato de tema — não havia forma de o consumidor impor a sua
identidade. Esta entrega resolve as duas coisas.

## Entrega

### 1. Contrato de temas no `sufficit-blazor-ui` (novo)

- `Themes/ISuiTheme.cs` — interface com `Palette`, `Typography`, `Layout`,
  `IsDark`. É o contrato entre a biblioteca e cada app consumidora.
- `Themes/SuiPalette.cs`, `SuiTypography.cs`, `SuiLayout.cs` — records com os
  tokens (cor, fonte, raio, sombra, espaçamento, motion), cada um com `Default`.
- `Themes/DefaultSuiTheme.cs` — implementação que espelha os tokens originais
  (azul, claro) para quem não customiza.
- `Themes/SuiThemeProvider.razor` — componente raiz que injeta as variáveis CSS
  (`--sui-*`) num `<style>` a partir do tema ativo, emite `data-sui-theme` e
  faz cascade do `ISuiTheme` para os filhos.
- `ServiceCollectionExtensions.cs` — `AddSufficitUI(opts => opts.Theme = ...)`
  regista o tema no DI.
- `wwwroot/sufficit-ui.css` — o `:root` mantém-se como fallback; o provider
  sobrescreve em runtime.

Isto substitui a ponte frágil anterior (`--sui-color-primary:
var(--sufficit-amber)`, uma linha amarrada ao blazor) por um contrato explícito
que qualquer consumidor implementa.

### 2. Quatro componentes promovidos do Identity para o `sufficit-blazor-ui`

| Origem (Identity Components/Common) | Destino (SUI) | Notas |
| --- | --- | --- |
| `AppIcon.razor` | `Components/SUIIcon.razor` | Dicionário de ~34 paths SVG preservado |
| `EmptyState.razor` | `Components/SUIEmptyState.razor` | Unificado: suporta `Icon` (nome SUI) e `IconPath` (SVG direto); `Actions` + alias `ActionContent` |
| `PageHeader.razor` | `Components/SUIPageHeader.razor` | API idêntica (Title, Eyebrow, Description, Actions) |
| `StatusBadge.razor` | `Components/SUIStatusBadge.razor` | `Tone` mantido como `string` ("success", etc.) — ver decisão abaixo |

- `Components/SUIEnums.cs` — adicionado `SUITone` (Neutral, Success, Warning,
  Danger, Info) para uso futuro; o `SUIStatusBadge` usa string por now.
- `wwwroot/sufficit-ui.css` — adicionados `.sui-page-header` e
  `.sui-status-badge` (com variantes `--success/--warning/--danger/--info`),
  que consomem tokens do tema.
- `Utilities/SuiClassBuilder.cs` — adicionado `Slug(object?)` para normalizar
  enums (SUI ou legados) em classes CSS, fixando nullable warnings introduzidos
  pela ponte de compatibilidade `object` nos botões.

### 3. Adoção no `Sufficit.Identity.UI.Management`

- **ProjectReference** ao `sufficit-blazor-ui` adicionada (via projeto
  `Sufficit.Identity.UI.Components`).
- **`IdentitySuiTheme`** (`Configuration/IdentitySuiTheme.cs`) — mapeia os
  tokens do Identity (`--brand` #cc0000, `--ink`, `--surface-*`, `--success`/
  `warning`/`danger`/`info`, Inter) para o contrato `ISuiTheme`.
- **Registo no DI** — `AddSufficitUI(opts => opts.Theme = new IdentitySuiTheme())`
  dentro de `AddSufficitIdentityManagementUI`.
- **`App.razor`** — `<link>` para `sufficit-ui.css` e `<SuiThemeProvider>` a
  envolver `<Routes>`.
- **`_Imports.razor`** — importados `Sufficit.Blazor.UI.Components` e
  `Sufficit.Blazor.UI.Themes`.
- **30 páginas + 2 layouts migrados** — renomeação mecânica de 316 ocorrências:
  `AppIcon`→`SUIIcon` (196), `StatusBadge`→`SUIStatusBadge` (58),
  `EmptyState`→`SUIEmptyState` (42), `PageHeader`→`SUIPageHeader` (26).

## Decisões de design

- **`SUIStatusBadge.Tone` é `string`, não `SUITone` enum.** O Identity calcula
  o tone em runtime via dezenas de helpers (`OutcomeTone`, `PoolTone`,
  `MfaTone`, ...) que retornam string. Forçar o enum exigiria refatorar cada
  helper. O `SUITone` enum fica no `SUIEnums.cs` para tipagem futura, mas o
  componente aceita string para a migração ser puramente mecânica.
- **NavMenu não migrado para `SUINavLink`.** O NavMenu do Identity é flat com
  classes CSS próprias (`.nav-item`, `.sidebar-nav`) definidas em 5000 linhas
  de `app.css`. O `SUINavLink` tem outra estrutura de classes (`.sui-nav-link`)
  — migrar exigiria reconciliar todo o CSS do shell. A infra de tema + stylesheet
  fica pronta para essa migração futura. Por agora, só os ícones do NavMenu
  foram trocados (`AppIcon`→`SUIIcon`).
- **Componentes antigos do Identity mantidos.** O projeto
  `Sufficit.Identity.UI.Vault` ainda usa `AppIcon`/`EmptyState`/`PageHeader`/
  `StatusBadge` (nomes antigos). Não posso removê-los do projeto Components sem
  migrar o Vault também — fora do escopo. Coexistem sem colisão (namespaces
  diferentes: `Sufficit.Identity.UI.Components.Common` vs
  `Sufficit.Blazor.UI.Components`).

## Não reivindicado

- **Navegação SUI completa** (NavMenu/MainLayout com `SUINavLink`/`SUINavGroup`).
- **Remoção dos componentes antigos** do projeto Components — pendente até o
  Vault migrar.
- **`Sufficit.Identity.UI` público** — ainda não adota SUI (segundo consumidor
  pendente no plano).
- **`sufficit-blazor` no novo contrato `ISuiTheme`** — continua com a ponte CSS
  de uma linha; migrar para o contrato é a Fase 4 (opcional).
- **Reconciliação visual** entre `.sui-*` e as classes do `app.css` do Identity
  (`.button`, `.panel`, `.data-table`, etc.). Os componentes SUI ficam com a
  identidade vermelha via tokens; o restante CSS do Management não foi tocado.

## Validação

- `dotnet build` do `sufficit-blazor-ui` — 0 warnings, 0 erros.
- `dotnet build Sufficit.Identity.sln -c Release` — **14 projetos**, incluindo
  `Sufficit.Identity.UI` público e `Sufficit.Identity.UI.Vault`,
  **0 warnings, 0 erros**.
- 316 renomeações verificadas: zero referências órfãs aos nomes antigos no
  Management; o Vault mantém as suas (resolvidas pelo projeto Components).

## Nota

A verificação é de compilação. Não houve inspeção visual em navegador — os
componentes SUI vão renderizar com a paleta vermelha via tokens, mas a
reconciliação com o CSS existente do Management (5000 linhas) só se confirma
em runtime.
