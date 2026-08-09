# Plano — migração de `sufficit-blazor` e `sufficit-ai`

**Status:** proposto
**Criado:** 2026-08-09
**Escopo:** `sufficit-blazor`, `sufficit-ai`, `sufficit-blazor-ui`

## Objetivo

Levar os dois consumidores para `net10.0` e substituir os componentes de UI
locais pelos `Suff*` desta biblioteca, sem quebrar o que funciona hoje.

## Situação atual

| Projeto | Framework | Projetos | UI |
| --- | --- | --- | --- |
| `sufficit-blazor-ui` | `net10.0` | 1 | MudBlazor vendorizado, sem pacote externo |
| `sufficit-blazor` | `net9.0` | 6 | MudBlazor via pacote (`Version="*"`) |
| `sufficit-ai` | `net9.0` | 6 | MudBlazor via pacote (`9.*`) |

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
| `TableNoRecords` | `SuffTableEmpty` | 40 |
| `MudButtonEnchanted` | `SuffButton` | 16 |
| `MudNavGroupEnhanted` | `SuffNavGroup` | 16 |
| `LoadingButton` | `SuffLoadingButton` | 5 |
| `MudNavLinkEnchanted` | `SuffNavLink` | 3 |
| `MudIconButtonEnchanted` | `SuffIconButton` | 2 |
| `SkeletonLoader` | `SuffSkeletonLoader` | 2 |
| `EmptyState` | `SuffEmptyState` | 1 |
| `MudSwitchButton` | `SuffSwitchButton` | 1 |

`sufficit-ai` **não usa nenhum** deles hoje. Ali é adoção, não migração — e
vale confirmar se compensa antes de investir.

## Fases

Cada fase termina com build verde e pode parar sem deixar trabalho pela metade.

### Fase 1 — `sufficit-blazor` para `net10.0`

- [ ] Recuperar por que existem os tetos `[*,10.0.0)` antes de mexer neles.
- [ ] Fixar `MudBlazor` numa versão exata (hoje `*`).
- [ ] Subir `<TargetFramework>` dos 6 projetos.
- [ ] Remover os `Microsoft.AspNetCore.*` em `2.*` que forem redundantes com o
      framework compartilhado.
- [ ] Build e testes verdes (`Sufficit.Blazor.Tests`,
      `Sufficit.Blazor.Provisioning.Tests`, `Sufficit.Blazor.Zabbix.Tests`).
- [ ] **Verificar em execução**, não só na build: os projetos WASM podem
      compilar e falhar em runtime.

### Fase 2 — trocar os componentes no `sufficit-blazor`

- [ ] Referenciar `Sufficit.Blazor.UI` e remover o `PackageReference` do
      MudBlazor (a biblioteca já o traz vendorizado).
- [ ] Substituir os nomes antigos pelos `Suff*` nos 84 arquivos. Começar por
      `SuffTableEmpty` (40 arquivos, componente simples): é o maior ganho com o
      menor risco.
- [ ] Deixar `SuffNavGroup` por último — é o mais complexo (popover, overlay,
      collapse, interop JS).
- [ ] Remover de `src/Components` os componentes que passaram a vir da
      biblioteca, mantendo os de domínio (`DIDTable`, `UserRolesTable`,
      `ClientView`, `Features/*`).
- [ ] Conferir visualmente as telas afetadas — a build não detecta regressão de
      estilo.

### Fase 3 — decidir sobre o `sufficit-ai`

- [ ] Levantar quais componentes do `ai` duplicam os da biblioteca
      (`AdminAppBar`, `AdminBottomNav`, layouts são candidatos).
- [ ] Se houver sobreposição real, migrar para `net10.0` e adotar.
- [ ] Se não houver, registrar a decisão de manter separado — adotar sem
      duplicação a eliminar é custo sem retorno.

### Fase 4 — contrato de temas

Só depois de dois consumidores reais. O `ThemeService` e o `MudThemeContainer`
do `sufficit-blazor` viram um contrato explícito (paleta, tipografia, densidade)
que cada aplicação fornece. Desenhar temas com um consumidor só é adivinhação.

## Riscos

- **Os tetos de versão são o risco principal.** Foram postos deliberadamente e
  o motivo não está documentado.
- **WASM em runtime.** Build verde não é garantia; testar as telas.
- **CSS.** A biblioteca traz o `MudBlazor.min.css` compilado da versão 9.8.0. Se
  o `sufficit-blazor` estiver hoje numa versão diferente, pode haver diferença
  visual sutil. Comparar antes e depois.
- **Sincronização com o upstream.** O MudBlazor vendorizado não recebe mais
  correções automáticas. Definir quem acompanha os releases e com que
  periodicidade sincroniza — inclusive os assets, via workflow
  `vendor-assets.yml`.

## Nota de método

Os números vieram de leitura dos repositórios (csproj, contagem de referências
em `.razor`), não de build nem de execução. Não foi possível compilar nenhum dos
dois projetos no ambiente onde este plano foi preparado, então o esforço real da
Fase 1 pode divergir — especialmente na parte de WASM.
