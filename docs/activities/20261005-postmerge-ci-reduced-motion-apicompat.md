# Pós-merge: CI da main — reduced-motion, pack ApiCompat e baselines visuais

**Data:** 2026-10-05  
**Branch:** `main` (após merge `afe00b4` da onda de consolidação)

## Contexto

O push do merge disparou o CI completo — que não rodava no worktree de
consolidação (só bUnit + build lá). Três classes de falha apareceram, todas
causadas pela própria onda:

## 1. `prefers-reduced-motion` como `.01ms` quebrava contratos de estado

**Sintomas:**
- `ReducedMotion_DisablesComponentAnimationsAndTransitions` (chromium/firefox/
  webkit): `animationName` do `.sui-progress-circular` ainda reportava
  `sui-spin` em vez de `none`.
- 6× `Button*` da vitrine (pages.yml): `hover == pressed` — a mudança de
  cor nunca era observada.

**Causa raiz:** a política centralizada (V3) usava
`transition-duration/.01ms !important`. Sob emulação
(`EmulateMediaAsync(ReducedMotion.Reduce)`), uma transição de ~0 ms ainda
devolve o **valor pré-mudança** quando lida no mesmo frame do estado novo —
`getComputedStyle` vê o valor de partida da transição ativa. O teste lia
`hover`, apertava (`Mouse.Down`), lia `pressed`… e via o hover de novo.

**Correção:** `transition: none !important; animation: none !important`
(nenhum script SUI escuta `transitionend`/`animationend` — verificado por
grep, então `none` é seguro). A exceção do spinner pending voltou por
**shorthand** (`animation: sui-btn-pending-spin 2.4s linear infinite
!important`), porque o bloqueio universal agora zera *todos* os longhands
do shorthand; durar só `animation-duration` não restaura o name/iteration.
Contratos de teste (`DesignTokenContractTests`, `StyleContractTests`)
atualizados para a nova forma.

**Validação local:** catálogo na 5180 → teste de motion verde; showcase
publicado + http.server na 5286 → os 6 testes `Button*` verdes.

## 2. Pack: ApiCompat rejeitava as quebras deliberadas

`CP0001` (os 6 tipos `T`→`TValue`/`TItem`), `CP0002`/`CP0006`
(`Add(string severity…)` removido, sobrecarga `SUITone` adicionada) contra
o baseline `2.26.918.1935`. Quebras documentadas no CHANGELOG e no baseline
interno, mas o gate de pack valida contra o **pacote publicado**. Gerado
`src/CompatibilitySuppressions.xml` (`ApiCompatGenerateSuppressionFile`)
com as 8 supressões exatas; `dotnet pack` local passa.

## 3. Baselines visuais: 2 px de drift legítimo

`catalog-light-desktop.png` 5466 → 5468 px de altura: os tokens de
densidade (`calc()`) mudam métricas reais. Recaptura segue o canal
sancionado: `workflow_dispatch` com `update-baselines=true` → job
`baseline-refresh` abre PR com os screenshots para revisão.

## Lição

Trabalho de UI em worktree precisa validar **os testes de navegador** antes
de merge na main — bUnit sozinho não cobre cascata/emulação de media. O CI
de branch (pull_request) teria pego isso; merges diretos não têm essa rede.

**Estado:** correções commitadas na main; baselines recapturados via
workflow e mesclados por PR de revisão.
