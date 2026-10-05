# Consolidação de design — P1: família Actions em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Terceira leva da migração progressiva para `SUIComponentBase` (pilotos em
`20261004-design-consolidation-component-base.md`, lote Layout em
`20261005-component-base-layout-batch.md`). Quatro componentes da família
Actions: `SUIButton`, `SUIIconButton`, `SUILoadingButton` e
`SUICopyToClipboard`. Com esta leva, `Actions` fica 100% na base.

## Decisões

- **`UserAttributes` legado:** os três botões publicavam
  `Dictionary<string, object?> UserAttributes` com captura de unmatched. O
  alias `[Obsolete]` + `MergeLegacyAttributes` em `OnParametersSet` (padrão
  `SUILink`) preserva os consumidores atuais; o markup splatta
  `AdditionalAttributes` no mesmo elemento de antes (`<button>`/`<a>`/raiz).
- **`SUICopyToClipboard`:** já declarava `Class`/`Style`/`AdditionalAttributes`
  com nomes canônicos — a migração removeu apenas a duplicação (herda da
  base). Sem alias necessário.
- **Lista congelada** (`NamingConventionTests.LegacyUserAttributes…`) segue
  com os 17 nomes: o alias `[Obsolete]` ainda expõe `UserAttributes`
  publicamente, então o teste continua passando sem edição — a lista só
  encolherá quando os aliases forei removidos na janela de quebra.
- **Catálogo:** os parâmetros herdados agora aparecem documentados (a base
  tem XML docs); `UserAttributes` marcado `deprecated: true` nos três botões.

## Validação

- Suíte unitária 933/933; build `-warnaserror` limpo.
- `generate-catalog --check` idempotente (73 componentes) +
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- `check:css` dentro do orçamento (raw 59.138 B).
- Pack + `validate-package.sh` (ApiCompat contra 2.26.918.1935 + smoke
  consumer net10.0 em root e pathbase) verdes.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): os 4
  componentes migram de `ComponentBase` para `SUIComponentBase`.
- Navegador chromium local (lição da onda anterior): catálogo + subpath +
  baselines visuais 59/59; showcase estático 110/110.
