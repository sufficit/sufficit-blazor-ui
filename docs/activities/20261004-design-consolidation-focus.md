# Consolidação de design — V2: foco de teclado

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `src/styles/sui-foundations.css`, folhas de estilo de componentes e `StyleContractTests`.

## Implementado

- Tokens `--sui-focus-ring`, `--sui-focus-offset` (externo, 2px) e `--sui-focus-offset-inset` (interno, -2px) em foundations. O anel herda `--sui-focus-color` do tema, com fallback para a cor primária.
- Substituição dos outlines de 2px e offsets literais em estilos globais e isolados. Controles full-bleed preservam o foco interno por token; ChoiceCard e SlidingTabs preservam seus anéis de cor/espessura específicos, mas usam offset do sistema.
- Offsets externos isolados de 1px/3px padronizados em 2px. `forced-colors` conserva seu `Highlight` nativo e seus 2px literais para contraste do sistema operacional.
- Teste de contrato garante tokens declarados e ausência de offsets 1px/3px em CSS autoral.

## Validação

- `node scripts/build-css.mjs`: passou (raw=57134 bytes, gzip=10542, brotli=9211).
- `dotnet test tests/Sufficit.Blazor.UI.Tests/Sufficit.Blazor.UI.Tests.csproj -v q --nologo`: 902 passaram, nenhuma falha.

## Próximo

V3: centralizar política de movimento reduzido, cobrindo CSS de portais fora de `.sui-root`.
