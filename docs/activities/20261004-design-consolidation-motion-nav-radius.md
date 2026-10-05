# Consolidação de design — V4/V5/V7: tokens de motion, raio de controle e nav no sistema

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `sui-foundations.css` (tokens), navegação (3 módulos), feedback transitório, portais, popover, sliding tabs, drawer.

## Problema

- Duração e easing eram inseparáveis: `--sui-transition` era a string combinada `160ms cubic-bezier(...)`. Módulos que precisavam "só da duração" inventavam literais (`120ms ease`, `220ms cubic-bezier(...)`, `var(--sui-transition-duration, 200ms)` com fallback mágico).
- O raio de botão vivia como fator `calc(var(--sui-radius) * 1.25)` no CSS de botões — consumidor que mudasse o raio base dependia desse fator escondido.
- A navegação era a única família fora da escala de tamanhos: `min-height: 50px`, `padding: 11px 14px`, alvos de 48px repetidos em literais.

## Implementado

**V4 — tokens de motion decompostos** (`sui-foundations.css`):
- `--sui-dur-fast: 120ms`, `--sui-dur: 160ms`, `--sui-dur-slow: 280ms`
- `--sui-ease` (padrão) e `--sui-ease-enter` (entrada com overshoot suave)
- `--sui-transition`/`--sui-transition-slow` viram atalhos compostos dos tokens atômicos
- `--sui-transition-duration` mantido como alias para o atraso de visibilidade do drawer e overrides de consumidores
- Migrados: tooltip do portal (120ms→fast/enter), toast (220ms→slow/enter), snackbar (180ms→dur), dialog (160ms→dur), popover (120ms→fast), painel das sliding tabs (240ms→slow)
- Loops mantidos literais por decisão documentada: spinner (.7s/1.4s), shimmer (1.4s), e o movimento-assinatura das sliding tabs (380ms/320ms com overshoot próprio)

**V5 — navegação na escala**:
- `--sui-nav-item-h: 48px` e `--sui-nav-item-h-nested: 40px` em foundations
- Item de nav: `min-height 50px → var(--sui-nav-item-h)` (48px, o valor que o rail já usava), `padding 11px 14px → var(--sui-space-3) var(--sui-space-4)`
- Rail/flyout: 48px literais → token; `border-radius: 14px` → `var(--sui-radius-lg)`

**V7 — raio de controle**:
- `--sui-radius-control: calc(var(--sui-radius) * 1.25)` em foundations; botões consomem o token

**Contrato** (`StyleContractTests.MotionDurations_Easings_AndControlShape_ComeFromTokens`):
- tokens declarados e consumidos;
- nenhuma duração literal em declarações de `transition`/`animation` fora da lista de exceções documentadas (loops de progresso e movimento-assinatura).

**Orçamento de bytes** (`FileSizeBudgetTests`): teto cru do bundle elevado de 56 KiB para 59.136 B seguindo a regra de headroom documentada (57.400 medidos × 1,03) — o custo de trocar literais por `var()`.

## Validação

- `node scripts/build-css.mjs`: passou (raw=57400, gzip=10567, brotli=9224 — dentro dos orçamentos do `AssetBudgetTests`).
- `dotnet test tests/Sufficit.Blazor.UI.Tests`: 904 passaram, 0 falhas.

## Próximo

Etapa P0 final: paridade dark CSS×C# (V6), guarda de estilos inline (L2) e alias de ícone (P5).
