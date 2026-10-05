# Consolidação de design — P1: família Feedback em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Quinta leva da migração progressiva para `SUIComponentBase` (pilotos em
`20261004-…-component-base.md`, Layout/Actions/DataDisplay nas notas de
2026-10-05). Onze componentes da família Feedback, incluindo o motor
compartilhado `SUINotificationHostBase` e os dois hosts que o usam.

## Decisões

- **`SUINotificationHostBase` herda de `SUIComponentBase`:** os hosts
  (`SUISnackbarHost`, `SUIToastHost`) não declaravam `Class`/`Style` nem
  capturavam unmatched. Com a mudança de base única, ambos expõem a
  superfície herdada e aplicam `Class`/`Style`/splat no elemento raiz
  (`.sui-snackbar-host` / `.sui-toast-host`).
- **Legados:** `SUIToast` e `SUIStatusBanner` tinham
  `Dictionary<string, object?> UserAttributes` capturando unmatched. Viram
  ponte `[Obsolete]` + `MergeLegacyAttributes` (padrão `SUILink`). O `SUIToast`
  também trocou a concatenação manual `class="sui-toast sui-toast--x @Class"`
  por `EffectiveClass(...)`.
- **`SUIStatusBadge`:** nunca aceitou `Class` (o `Classname` ignora o
  consumidor); agora usa `EffectiveClass` e ganha `Style`/splat no raiz.
- **`SUISkeletonLoader`:** concatenação manual `class="sui-skeleton-loader
  @Class"` substituída por `EffectiveClass`; ganha `Style` e splat.
- **`SUIEmptyState`:** idem (`string.IsNullOrWhiteSpace(Class) ? …` →
  `EffectiveClass("sui-empty")`), agora com `Style` no raiz.
- **Gerador de catálogo:** `scripts/generate-catalog.py` só seguia
  `@inherits SUIComponentBase` ao anexar os parâmetros herdados; com os hosts
  herdando via `SUINotificationHostBase<…>` o
  `CatalogMetadataTests.Catalog_DocumentsEveryPublicComponentParameter`
  falhou (`SUISnackbarHost missing: AdditionalAttributes, Class, Style`).
  A regex agora cobre as duas formas de herança.

## Validação

- Suíte unitária 933/933; build limpo.
- `generate-catalog --check` idempotente (73 componentes) +
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- `check:css` dentro do orçamento (raw 59.138 B).
- Pack + `validate-package.sh` (ApiCompat contra 2.26.918.1935 + smoke
  consumer net10.0 em root e pathbase) verdes.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): os 11 +
  `SUINotificationHostBase` migram de `ComponentBase` para
  `SUIComponentBase`.
- Navegador chromium local: catálogo + subpath + baselines visuais 59/59;
  showcase estático 110/110.
