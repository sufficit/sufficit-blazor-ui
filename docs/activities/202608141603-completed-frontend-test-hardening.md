# Plano concluído — independência total de terceiros e hardening de testes

**Status:** Concluído
**Criado:** 2026-08-14
**Concluído:** 2026-08-14
**Escopo:** stylesheet, comentários/metadados do pacote, suíte de testes,
catálogo de exemplo e CI

## Problema confirmado

A biblioteca se descrevia como autônoma, mas ainda tinha acoplamento real e
verificável com a biblioteca visual legada:

- 19 declarações CSS liam tokens de terceiros **antes** do token próprio, no
  formato `var(--mud-palette-primary, var(--sui-color-primary))`. Num consumidor
  que ainda carregasse a lib legada, era o tema dela que vencia — a SUI só
  aparecia quando o outro tema estava ausente;
- um seletor `.mud-icon-root` estilizava um elemento que a SUI nunca renderiza;
- 20 comentários, a `PackageDescription` e o `dependabot.yml` citavam o nome do
  framework antigo como referência de identidade ("Replaces X.Color").

Nada disso era pego por teste: o build passava, os componentes renderizavam e a
regressão só apareceria como tema errado na tela de um consumidor.

## Decisão implementada

### Independência

- Todos os `var(--mud-*)` removidos; os componentes resolvem apenas `--sui-*`;
- seletor `.mud-icon-root` removido;
- comentários, `csproj` e `dependabot.yml` reescritos sem citar o produto de
  terceiros;
- bundle `sufficit-ui.css` regenerado (raw 45.872 → gzip 8.762 → brotli 7.655).

**Efeito colateral consciente:** um consumidor que dependia do fallback para
temar a SUI passa a precisar de `SUIThemeProvider`/tokens `--sui-*`. Registrado
em `CHANGELOG.md` e em `skills/sui-design/references/components.md`.

A nota de atribuição MIT no `LICENSE` referente a `SUINavGroup.razor.cs`
permanece: o arquivo já não contém cabeçalho de copyright de terceiros, mas
retirar a nota é decisão jurídica, não técnica, e exigiria confirmar reescrita
limpa. Item aberto.

### Testes adicionados

Camada de contrato de código (`tests/Sufficit.Blazor.UI.Tests`):

| Arquivo | Garante |
| --- | --- |
| `RepositoryLayout.cs` | localiza o repositório em disco (CallerFilePath + walk-up) para os testes lerem fonte real |
| `NoThirdPartyUiFrameworkTests.cs` | nenhum identificador, pacote, assembly referenciado, classe CSS ou custom property de terceiros |
| `NamingConventionTests.cs` | prefixo `SUI`, namespaces aprovados, parâmetros públicos PascalCase, callbacks `OnXxx`/`XxxChanged`, arquivos colocalizados com componente existente, JS só como `.razor.js` |
| `FileSizeBudgetTests.cs` | teto de linhas por extensão, com débito congelado que só encolhe |
| `StyleContractTests.cs` | classes namespacadas, tokens com prefixo próprio, z-index tokenizado, `!important` contido, foco visível, ausência de OKLCH, reduced-motion e forced-colors |
| `AssetBudgetTests.cs` | bytes raw/gzip/brotli do bundle, por módulo JS e do CSS isolado |

Camada de navegador (`tests/Sufficit.Blazor.UI.BrowserTests`):

- `AccessibilityBrowserTests.cs` — axe WCAG 2.2 AA em mobile light/dark e
  desktop dark, sob `prefers-reduced-motion` + `prefers-color-scheme: dark`;
  foco visível em todo elemento focável; ausência de `tabindex` positivo;
  espaçamento de texto (SC 1.4.12) sem overflow; um único `main`, um `h1`,
  headings sem salto, `lang` e `title`; live region presente; `img`/`svg`
  rotulados ou ocultos.
- `PerformanceBudgetBrowserTests.cs` — requests, nós do DOM, folhas de estilo,
  scripts bloqueantes, bytes transferidos por tipo, LCP/CLS/FCP via
  `PerformanceObserver`, estabilidade de layout ao abrir/fechar overlay e
  cacheabilidade do asset estático. Métricas exclusivas do Chromium são
  ignoradas explicitamente nos outros engines em vez de afrouxadas.

Camada de página completa (CI):

- job `lighthouse` no `build.yml`, com `eng/lighthouse-budget.json` e
  `scripts/check-lighthouse.mjs`. `publish` passa a depender dele.

### Correções que os testes encontraram

- `SUIPageHeader.razor.css` suprimia o anel de foco. Confirmado como
  intencional (alvo de foco programático com `tabindex="-1"`, fora da ordem de
  tabulação); a regra do teste passou a aceitar exatamente esse caso e o
  comentário no CSS explica o porquê;
- catálogo sem `meta description` e sem favicon (404 no console). Corrigidos —
  Lighthouse saiu de `best-practices 0.96 / seo 0.90` para `1.0 / 1.0`.

## Evidências

```
dotnet test tests/Sufficit.Blazor.UI.Tests            → 218 passed, 0 failed
dotnet test tests/Sufficit.Blazor.UI.BrowserTests     →  38 passed, 1 skipped (chromium)
node scripts/check-lighthouse.mjs                     → performance=1 accessibility=1 best-practices=1 seo=1
                                                         FCP 446ms · LCP 446ms · TBT 0 · CLS 0 · SI 446
grep -ri mud src/                                     → 0 ocorrências
```

## Débito registrado

Renomes que só cabem na v2 (documentados em `docs/PLAN-SUI-V2.md`):
`NavAccordionScope`, `SUIItem.xs/sm/md/lg/xl`, `SUIAlert.CloseIconClicked`.

Arquivos acima do teto de linhas, congelados no tamanho atual:
`src/styles/sui-components.css` (1.429), `src/styles/sui-foundations.css` (415)
e `tests/.../CatalogBrowserTests.cs` (935).
