# Plano — adoção dos componentes `SUI*`

**Status:** Implementado em `sufficit-blazor` e `sufficit-ai-genius`; pendente
nas UIs do `sufficit-identity` (`Sufficit.Identity.UI` e
`Sufficit.Identity.UI.Management`)
**Criado:** 2026-08-09
**Escopo:** `sufficit-blazor`, `sufficit-ai-genius`, `sufficit-identity`

## Objetivo

Adotar os componentes `SUI*` desta biblioteca nas aplicações Sufficit que têm
UI Blazor, substituindo os componentes locais (e, onde couber, reduzindo a
dependência do MudBlazor), sem quebrar o que funciona hoje.

## Situação atual

| Projeto | Framework | UI | Status |
| --- | --- | --- | --- |
| `sufficit-blazor-ui` | `net10.0` | Componentes SUI autônomos (sem MudBlazor), CSS próprio (`sufficit-ui.css`) | Biblioteca |
| `sufficit-blazor` | `net10.0` | MudBlazor para componentes restantes + SUI via `ProjectReference` | **Implementado** |
| `sufficit-ai-genius` | `net10.0` | `Sufficit.AI.Genius.UI` já referencia SUI | **Implementado** |
| `sufficit-identity` (`Sufficit.Identity.UI`) | — | UI pública, sem MudBlazor nem SUI ainda | **Pendente** |
| `sufficit-identity` (`Sufficit.Identity.UI.Management`) | — | UI de gestão, sem MudBlazor nem SUI ainda | **Pendente** |

> Observação: as duas UIs pendentes vivem **dentro** do repositório
> `sufficit-identity` (em `src/ui/`), não são repositórios separados. Ambas
> estão hoje sem `PackageReference` MudBlazor e sem referência a SUI — a
> adoção é construção, não migração de MudBlazor.

Um RCL Razor não atravessa a diferença de framework, então a migração para
`net10.0` é pré-requisito para qualquer consumo.

## Bloqueadores levantados

**1. Tetos de versão explícitos.** Cinco `PackageReference` no `sufficit-blazor`
usam `[*,10.0.0)`, ou seja, proíbem explicitamente 10.x:
`Microsoft.AspNetCore.Authentication.OpenIdConnect`,
`Microsoft.AspNetCore.Components.Web`,
`Microsoft.AspNetCore.Components.WebAssembly.Server`,
`Microsoft.AspNetCore.SpaProxy`,
`Microsoft.AspNetCore.SpaServices.Extensions`.

Esses tetos foram postos por alguma razão — provavelmente uma incompatibilidade
conhecida. **Antes de removê-los, vale recuperar o motivo** (histórico do git ou
memória do time). Subir o teto às cegas é o risco mais concreto deste plano.

**2. Pacotes ancorados em 2.x.** Vários `Microsoft.AspNetCore.*` estão em
`2.*` (`Authentication`, `Authentication.Cookies`, `Http.Abstractions`,
`Mvc.RazorPages`). Em projetos que referenciam o framework compartilhado, esses
pacotes standalone são redundantes desde o .NET Core 3 e costumam ser resquício.
Provavelmente devem sair, não subir de versão.

**3. WebAssembly.** Cinco projetos usam WASM (`Sufficit.Blazor.Server`,
`Sufficit.Blazor`, `Sufficit.Blazor.Client`, `Sufficit.AI.Web`,
`Sufficit.AI.Api`). É a parte mais sensível de uma troca de framework: o runtime
WASM é recompilado e problemas aparecem em execução, não na build.

**4. `MudBlazor Version="*"` no `sufficit-blazor`.** Curinga irrestrito. Durante
a migração convém fixá-lo, para não misturar duas variáveis (framework novo +
versão de UI diferente) no mesmo diagnóstico.

## Esforço de troca de componentes

`sufficit-blazor` — 84 arquivos `.razor` referenciam algum componente que a
biblioteca agora fornece:

| Antigo | Novo | Arquivos |
| --- | --- | --- |
| `TableNoRecords` | `SUITableEmpty` | 40 |
| `MudButtonEnchanted` | `SUIButton` | 16 |
| `MudNavGroupEnhanted` | `SUINavGroup` | 16 |
| `LoadingButton` | `SUILoadingButton` | 5 |
| `MudNavLinkEnchanted` | `SUINavLink` | 3 |
| `MudIconButtonEnchanted` | `SUIIconButton` | 2 |
| `SkeletonLoader` | `SUISkeletonLoader` | 2 |
| `EmptyState` | `SUIEmptyState` | 1 |
| `MudSwitchButton` | `SUISwitchButton` | 1 |

`sufficit-ai` **não usa nenhum** deles hoje. Ali é adoção, não migração — e
vale confirmar se compensa antes de investir.

## Fases

Cada fase termina com build verde e pode parar sem deixar trabalho pela metade.

### Fase 1 — `sufficit-blazor` para `net10.0`

- [x] Subir `<TargetFramework>` dos 6 projetos.
- [x] Fixar `MudBlazor` em `9.8.0` durante a transição.
- [x] Substituir os tetos incompatíveis `[*,10.0.0)` por referências `10.*`.
- [ ] Remover os `Microsoft.AspNetCore.*` em `2.*` que forem redundantes com o
      framework compartilhado.
- [x] Build e testes verdes (`Sufficit.Blazor.Tests`,
      `Sufficit.Blazor.Provisioning.Tests`, `Sufficit.Blazor.Zabbix.Tests`).
- [ ] **Verificar em execução**, não só na build: os projetos WASM podem
      compilar e falhar em runtime.

### Fase 2 — trocar os componentes no `sufficit-blazor`

- [x] Referenciar `Sufficit.Blazor.UI` e **incluir o stylesheet
      `sufficit-ui.css`** (e, se quiser dark mode, o atributo
      `data-sui-theme="dark"`). A biblioteca não traz mais o MudBlazor, então o
      `MudBlazor.min.css` deixa de ser necessário **apenas para os componentes
      SUI** — mas qualquer outro uso de MudBlazor na aplicação continua
      precisando do pacote e do próprio CSS.
- [x] **Atenção à mudança de API:** os parâmetros que antes usavam enums do
      MudBlazor (`Color`, `Variant`, `Size`, `ButtonType`, `Typo`, `Align`)
      agora usam os enums SUI equivalentes (`SUIColor`, `SUIVariant`, `SUISize`,
      `SUIButtonType`, `SUITypo`, `SUIAlign`) nos usos novos. Os controles de
      ação mantêm uma ponte temporária para os valores legados, evitando uma
      troca big-bang durante a migração.
- [x] Substituir os nomes antigos pelos `SUI*` nos 84 arquivos. Começar por
      `SUITableEmpty` (40 arquivos, componente simples): é o maior ganho com o
      menor risco.
- [x] Migrar `SUINavGroup` — é o mais complexo (flyout rail, collapse
      animado via CSS, accordion entre irmãos). Agora é CSS puro, sem JS nem
      popover portal.
- [x] Remover de `src/Components` os componentes que passaram a vir da
      biblioteca, mantendo os de domínio (`DIDTable`, `UserRolesTable`,
      `ClientView`, `Features/*`).
- [ ] Conferir visualmente as telas afetadas — a build não detecta regressão de
      estilo. O visual dos componentes SUI mudou (tokens próprios, não
      Material Design).

### Fase 3 — adotar nas UIs do `sufficit-identity`

As duas UIs vivem no repositório `sufficit-identity`, em `src/ui/`, e hoje não
usam MudBlazor nem SUI — adoção é construção.

- [ ] `Sufficit.Identity.UI` (UI pública) — referenciar `Sufficit.Blazor.UI`,
      incluir `sufficit-ui.css`, adotar os `SUI*` onde houver sobreposição com
      componentes locais.
- [ ] `Sufficit.Identity.UI.Management` (UI de gestão) — idem.
- [ ] Confirmar o framework (`net10.0`) e o `ProjectReference` em cada projeto;
      um RCL Razor não atravessa diferença de framework.
- [ ] Conferir visualmente as telas afetadas.

### Fase 3 (concluída) — `sufficit-ai-genius`

- [x] `Sufficit.AI.Genius.UI` já referencia `Sufficit.Blazor.UI` e consome os
      `SUI*` em `_Imports.razor`. (O projeto `Mobile`, em `heads/`, ainda usa
      MudBlazor — fora do escopo desta adoção web.)

### Fase 4 — contrato de temas

Só depois de dois consumidores reais. O `ThemeService` e o `MudThemeContainer`
do `sufficit-blazor` viram um contrato explícito (paleta, tipografia, densidade)
que cada aplicação fornece. Desenhar temas com um consumidor só é adivinhação.

## Riscos

- **Os tetos de versão são o risco principal.** Foram postos deliberadamente e
  o motivo não está documentado.
- **WASM em runtime.** Build verde não é garantia; testar as telas.
- **CSS.** A biblioteca agora traz o próprio `sufficit-ui.css` (tokens `--sui-*`,
  visual limpo, não Material). Os componentes SUI não dependem mais do
  `MudBlazor.min.css`; qualquer diferença visual é esperada e precisa de revisão
  tela a tela.
- **Mudança de tipos.** Os enums SUI são o contrato recomendado para código
  novo; os controles de ação preservam temporariamente valores legados para
  permitir migração incremental.

## Nota de método

Os números vieram da leitura dos repositórios e foram conferidos após a
migração. A solução do `sufficit-blazor` compilou com 32 projetos; os testes
executados passaram em `235 + 45 + 16` casos, além dos 3 testes específicos do
`SUINavGroup`.
