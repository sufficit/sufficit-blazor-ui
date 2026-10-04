# Consolidação de design — V3: política central de movimento reduzido

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `sui-foundations.css` (política central), 11 arquivos CSS (blocos periféricos removidos), `StyleContractTests`.

## Problema

`prefers-reduced-motion` era tratado bloco a bloco: 11 arquivos cobriam seus próprios seletores e qualquer componente novo nascia fora da política por esquecimento. Switch, select, drawer e nav já tinham ficado de fora antes de correções pontuais anteriores.

## Implementado

- Política única em `sui-foundations.css`:
  - `.sui-root *` cobre o conteúdo sob o provider;
  - `[class*="sui-"]` cobre componentes sem provider e superfícies portadas ao `document.body` (tooltips), que sempre carregam classe `sui-*`.
  - Declarações forçadas (`duration: .01ms`, `iteration-count: 1`) são deliberadas: CSS scoped carrega depois e não pode devolver movimento a quem pediu menos.
- Exceção documentada preservada: spinner de botão pendente (`animation-duration: 2.4s`, iteração infinita) é feedback de progresso, não decoração.
- 11 blocos periféricos removidos (globais e scoped), incluindo os que faltavam cobertura total.
- `StyleContractTests`:
  - `ReducedMotion_IsCentralizedInFoundations` — nenhum outro CSS autoral pode declarar `prefers-reduced-motion`;
  - varredura de `!important` agora ignora comentários (prosa não é regra) e sanciona apenas as declarações de duração/iteração da política central.

## Validação

- `node scripts/build-css.mjs`: passou (raw=56652, gzip=10487, brotli=9173).
- `dotnet test tests/Sufficit.Blazor.UI.Tests`: 903 passaram, 0 falhas.

## Próximo

V4/V5/V7: tokens de duração/easing, raio de controle derivado do token base e navegação na escala de tamanhos.
