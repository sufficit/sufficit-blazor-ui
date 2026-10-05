# Consolidação de design — V6/L2/P5: paridade dark, estilos inline e alias de tamanho

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `SUITheme`/`SUIThemeCssWriter` × `sui-foundations.css` (V6), componentes Forms/DataDisplay/Feedback/Navigation (L2), marcação interna + CSS (P5), testes de contrato.

## Problema

- **V6** — a paleta dark existia em dois lugares (`SUITheme.Dark` em C# e o bloco `[data-sui-theme="dark"]` do CSS) sem proteção contra divergência.
- **L2** — 13 estilos `style="..."` estáticos hardcoded em componentes contradiziam o suporte a CSP estrita da biblioteca.
- **P5** — aliases de classe duplicados (`--sm/--small`, `--md/--medium`, `--lg/--large`) coexistiam sem eleição documentada.

## Implementado

**V6 — teste de paridade (`ThemeParityTests`, novo arquivo):**
- Renderiza `SUITheme.Light`/`SUITheme.Dark` via `SUIThemeCssWriter.Write` e compara, token a token, cada cor literal com o bloco autoral correspondente (`:root` / `[data-sui-theme="dark"]`).
- O teste **encontrou uma divergência real**: o bloco dark do CSS não declarava `--sui-color-secondary`/`-contrast` (herdava o slate claro do `:root`) enquanto `SUITheme.Dark` publica `#cbd5e1`. Corrigido no CSS — o fallback sem provider agora conversa com o preset.
- Normalização: hex curto expandido (`#fff` ≡ `#ffffff`), comentários removidos antes do parse, valores computados (`var()`, `color-mix()`) fora do escopo.

**L2 — migração de estilos inline para classes:**
- `SUITextField`: textarea multiline (`--multiline`), wrapper `position:relative`, botão clear e adornments → classes em foundations; padding dinâmico do input (`3rem`/`5rem`) vira `InputClassname` com classes `--pad-trailing`/`--pad-clear`.
- Reserva de altura do slot de erro (`min-height:1.3em`) → `sui-field__error--reserve` (TextField + NumericField).
- `SUIPagination` (nav + controls), `SUIStat` (card layout), `SUIProgressSteps` (lista/item/botão; `li.sui-progress-steps__item` com especificidade calculada para vencer `.sui-spacer` na cascata), `SUISkeletonLoader` (margem da linha de tabela) → classes em `sui-shared-base.css`/`sui-shared-skeleton.css`.
- Sobreviveu **de forma deliberada**: `style="width: @Size; height: @Size"` do skeleton circular e demais expressões Razor (`style="@..."`) — valores por instância que nenhuma classe expressa.
- Guarda: `Components_DoNotShipStaticInlineStyles` falha se qualquer `style="literal"` voltar a aparecer em `.razor`.

**P5 — nomes canônicos de tamanho:**
- `--small/--medium/--large` eleitos canônicos (casam com `SUISize`); `--sm/--md/--lg` marcados como legado em comentário de remoção na próxima janela de quebra.
- Marcação interna migrada: `SUITableEmpty` (`--lg`→`--large`), `SUIButton`/`SUILoadingButton` (`sui-icon--sm`→`--small`), `SUIDateField` + Calendar (`sui-btn--sm`→`--small`), `SUIProgressSteps`.
- Guarda: `InternalMarkup_UsesCanonicalSizeClasses_NotLegacyAliases` proíbe os formas curtas em `.razor`/`.cs` internos.

**Organização de testes:** os contratos de foco/motion/nav/inline/alias saíram do `StyleContractTests` (estourando o orçamento de 400 linhas) para o novo `DesignTokenContractTests`.

**Orçamentos** (regra de headroom ×1,03, medidos 58.993/10.848/9.476 B): `build-css.mjs` 58.368→59.136 raw, 10.752→11.008 gzip, 9.472→9.728 brotli; `AssetBudgetTests` 58.624/11.008/9.472 → 60.928/11.264/9.984.

## Validação

- `node scripts/build-css.mjs`: passou (raw=58993, gzip=10848, brotli=9476).
- `dotnet test`: 910 passaram, 0 falhas (907 antes dos guardas novos + 2 de paridade + 1 alias).
- `dotnet build` da library e dos BrowserTests: 0 erros.

## Próximo

Etapa P1: `SUIComponentBase` com Class/Style/AdditionalAttributes (P1/P2).
