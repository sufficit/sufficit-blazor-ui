# Design consolidation — interaction state tint scale (`--sui-state-*`)

**Date:** 2026-10-04
**Branch:** `work/design-consolidation` (worktree `../sufficit-blazor-ui-consolidacao`)
**Evaluation item:** V1 (tints de interação são números mágicos espalhados)
**Commit:** `7d7d0f7`

## O que mudou

- Adicionada a escala de estados em `src/styles/sui-foundations.css`:
  - `--sui-state-hover` (8%), `--sui-state-selected` (12%),
    `--sui-state-pressed` (16%), `--sui-state-soft` (20%) —
    todos derivados de `--sui-color-primary` via `color-mix`.
- `--sui-color-primary-soft` virou **alias de compatibilidade** de
  `--sui-state-selected` (light e dark). O default C#
  (`SUIPalette.PrimarySoft`, 14%) **não** foi alterado nesta etapa; o
  alinhamento do default fica para a etapa de paridade dark/escrita de tema,
  que decide a fonte única do valor.
- Consumidores migrados:
  - Nav hover: `color-mix(… 7%)` → `var(--sui-state-hover)`
  - Nav ativa: `color-mix(… 11%)` → `var(--sui-state-selected)`
  - Rail trigger hover/aberto: `color-mix(… 12%)` → `var(--sui-state-selected)`
  - `SUICardHeader` icon container: `color-mix(… 10%)` → `var(--sui-state-selected)`
- `src/wwwroot/sufficit-ui.css` recompilado (`npm run build:css`):
  raw 56931 B, gzip 10517 B, brotli 9154 B.

## Decisões de design

- 7%→8% e 11%→12%: diferença imperceptível; a escala ganha memorização
  (8/12/16/20, passos de 4) sem mudança visual relevante.
- Tints **não-interativos** ficaram fora da escala de propósito:
  `--sui-nav-accent` (38%/62% de acento textual) e a borda de
  `SUIPageHeader--prominent` (14% sobre `--sui-border`) têm semântica
  própria; forçá-los na escala criaria acoplamento errado.

## Validação

- `dotnet build src/Sufficit.Blazor.UI.csproj` — OK, 0 warnings.
- `node scripts/build-css.mjs` — OK, sem estouro de budget.
- Grep confirma que nenhum módulo mais define porcentagem própria de
  tint de primário para estados (restam apenas as definições da escala e
  os casos de acento deliberados acima).

## Próximas etapas (roadmap P0)

1. Tokens de foco (`--sui-focus-ring`, `--sui-focus-offset`, inset) — V2.
2. Regra global de `prefers-reduced-motion` — V3.
3. Tokens de motion (`--sui-dur-*`, `--sui-ease-*`), raio de controle e nav — V4/V5/V7.
