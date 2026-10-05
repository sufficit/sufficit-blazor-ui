# Consolidação de design — F1/F4 + L1/L4: densidade em cascata, z-index/FsField tematizáveis, .editorconfig e scripts documentados

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `sui-foundations.css`, `SUILayout`, `SUITypography`, `SUIThemeCssWriter`, vitrine (densidade), `DesignTokenContractTests`, orçamentos de asset, `.editorconfig`, `scripts/README.md`.

## Contexto

Última etapa da varredura priorizada pela avaliação de 2026-10-04. Três achados:

- **F1** — densidade não era sistêmica: `Dense` existia como parâmetro em
  `SUIList`/`SUITable`/`SUIAppBar`, e a vitrine fingia densidade global
  sobrescrevendo `ControlHMd` (32px/40px) — um único controle, não o sistema.
- **F4** — `--sui-z-dropdown…tooltip` (7 tokens) e `--sui-fs-field` existiam
  apenas no CSS autoral: fora do modelo `SUILayout`/`SUITypography`, fora do
  `SUIThemeCssWriter`, intemáveis pelo consumidor.
- **L1/L4** — sem `.editorconfig` na raiz; `scripts/` mistura `.mjs` e `.py`
  sem explicar qual runtime cobre o quê.

## Mudanças

1. **F1 — densidade como token em cascata** (`sui-foundations.css`,
   `SUILayout`):
   - `--sui-density-scale: 1` publicado em `:root`; consumidor (ou qualquer
     subárvore) sobrepõe `0.8` e todo o sistema aperta junto.
   - `--sui-control-h-sm/md/lg` e `--sui-control-px-sm/md/lg` agora derivam
     via `calc(Npx * var(--sui-density-scale))` — no CSS autoral **e** nos
     defaults do `SUILayout` (uma única fonte da regra).
   - `SUILayout.DensityScale` (string, default `"1"`) + writer publica
     `--sui-density-scale` junto dos demais tokens.
   - **Áreas de toque de navegação ficam literais** (`48px`/`40px`):
     piso de acessibilidade não escala com densidade — documentado no CSS,
     no modelo e guardado por teste (`DoesNotContain … nav-item-h: calc(`).
   - Vitrine: `ShowcaseTheme` troca o override `ControlHMd = 32px/40px` por
     `DensityScale = "0.85"/"1"`, e o snippet gerado pelo `ThemeEditor`
     passa a mostrar o token certo.

2. **F4 — z-index e FsField no modelo de tema** (`SUILayout`, `SUITypography`,
   `SUIThemeCssWriter`):
   - `SUILayout.ZDropdown/ZSticky/ZDrawer/ZBackdrop/ZModal/ZToast/ZTooltip`
     (defaults 1000–1600, espelhando o CSS autoral).
   - `SUITypography.FsField` (default `.8125rem`) — o token que um consumidor
     com tipografia própria mais precisa alcançar, hoje inalcançável.
   - Writer publica os 8 tokens novos.

3. **L1 — `.editorconfig`** na raiz: charset/EOF/indentação por família de
   arquivo (4 espaços cs/razor/py; 2 css/json/mjs/yml), file-scoped
   namespaces, `using` com `System` primeiro, `var` preferido — nada além
   do que o código **já** faz; agora editores e agentes concordam de graça.

4. **L4 — `scripts/README.md`**: papel de cada runtime (Node = pipeline de
   CSS/Lighthouse; Python = catálogo/release), regra de bolso no fim.

## Contratos novos

- `DesignTokenContractTests`:
  - `DensityToken_ScalesControlSizing_InFoundationsAndTheme` — token existe,
    deriva alturas/paddings, nav fica literal, modelo publica e defaults C#
    derivam igualmente.
  - `StackingOrder_AndFieldFontSize_AreThemeableTokens` — writer emite os 7
    z-index + `--sui-fs-field`; CSS autoral segue como fonte.

## Orçamentos (regra de headroom ×1,03)

- Bundle medido: **59,213 / 10,877 / 9,490 B** (raw/gzip/brotli) — o custo
  dos seis `calc(...var(--sui-density-scale))`.
- `build-css.mjs` raw: 59,136 → **59,392** (gzip/brotli seguem nos tetos).
- `FileSizeBudgetTests` raw: 59,136 → **61,184** com o comentário medido.
- `AssetBudgetTests` já tinha headroom (60,928/11,264/9,984) — intocado.

## Incidente de edição (transparente)

Um `fs_edit` com âncora errada corrompeu brevemente
`DesignTokenContractTests.cs` (inseriu o teste novo no meio do corpo do
`MotionDurations…`). Reparado por script com asserts de unicidade antes de
qualquer teste rodar; arquivo final com 198 linhas, dentro do orçamento de
400, e a suíte passou na primeira execução após o reparo.

## Validação

- Suíte unitária: **928/928** (era 926; +2 testes de contrato).
- `Sufficit.Blazor.UI.Demos`, `Sufficit.Blazor.UI.Showcase`,
  `Sufficit.Blazor.UI.BrowserTests`: builds verdes, 0 warnings.
- `eng/PublicApiBaseline.txt` regenerado: +9 propriedades (`DensityScale`,
  7×`Z*`, `FsField`).

## Não feito (fora do escopo desta onda)

- F5 (convergência snackbar/toast + pausa hover WCAG 2.2.1) — exige
  verificação de timer, avaliação pede passos separados.
- P3 (generics `TValue`/`TItem`) — janela de quebra de fonte a planejar.
- `dotnet format --verify-no-changes` no CI — proposta da avaliação para
  acompanhar o `.editorconfig`; adicionar em change próprio do pipeline.
