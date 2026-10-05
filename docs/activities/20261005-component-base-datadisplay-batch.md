# Consolidação de design — P1: família DataDisplay em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Quarta leva da migração progressiva para `SUIComponentBase` (pilotos em
`20261004-design-consolidation-component-base.md`, Layout em
`20261005-component-base-layout-batch.md`, Actions em
`20261005-component-base-actions-batch.md`). Catorze componentes da família
DataDisplay — a maior família até agora, incluindo os dois que ainda
publicavam `UserAttributes` legado (`SUITd`/`SUITh`).

## Decisões

- **`SUITd`/`SUITh`:** o par de células da tabela era o último da família com
  `Dictionary<string, object?> UserAttributes` capturando unmatched. Virou
  ponte `[Obsolete]` + `MergeLegacyAttributes` em `OnParametersSet` (padrão
  `SUILink`); o markup splatta `AdditionalAttributes` no mesmo `td`/`th`.
- **`Style` passa a valer de verdade:** só `SUIAvatar` publicava `Style`
  antes; nos demais chegava por acaso pelo catch-all (`SUIList` chegava a
  documentar isso) ou não chegava (`SUIListItem`, `SUITableEmpty`, `SUITd`,
  `SUITh`, `SUIText` via AddMultipleAttributes sobrescrevendo). Agora o raiz
  de todos aplica `style="@Style"`; `SUIText` concatena com o `ColorStyle`
  que o parâmetro `Color` produz.
- **Novos pontos de splat:** `SUIListItem` e `SUITableEmpty` não capturavam
  nada (um `id=` do consumidor crashava em runtime). Ambos agora splattam
  `AdditionalAttributes` no raiz.
- **`SUITableEmpty`:** substituída a concatenação manual
  (`$"sui-table-empty sui-width-full {Class}"`) por
  `EffectiveClass("sui-table-empty sui-width-full")`.
- **`SUIIcon`:** preservado o parâmetro `CssClass` (público, usado pelos
  consumers); a classe do `svg` agora é `CssClass + Class` da base, e o
  atributo continua omitido quando ambos vazios, como antes.
- **Lista congelada** (`NamingConventionTests`): inalterada — os aliases
  `[Obsolete]` de `SUITd`/`SUITh` ainda expõem `UserAttributes`.

## Validação

- Suíte unitária 933/933; build limpo.
- `generate-catalog --check` idempotente (73 componentes) +
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- `check:css` dentro do orçamento (raw 59.138 B).
- Pack + `validate-package.sh` (ApiCompat contra 2.26.918.1935 + smoke
  consumer net10.0 em root e pathbase) verdes.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): os 14
  migram de `ComponentBase` para `SUIComponentBase`.
- Navegador chromium local: catálogo + subpath + baselines visuais 59/59;
  showcase estático 110/110.
