# Consolidação de design — P1: família Navigation em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Sexta leva da migração progressiva para `SUIComponentBase` (pilotos em
`20261004-…-component-base.md`; Layout/Actions/DataDisplay/Feedback nas
notas de 2026-10-05). Oito componentes da família Navigation, incluindo o
`SUINavGroup` com code-behind e interop de rail flyout.

## Decisões

- **`SUINavGroup`:** base trocada no code-behind
  (`ComponentBase, IAsyncDisposable` → `SUIComponentBase, IAsyncDisposable`)
  e no `.razor` (`@inherits SUIComponentBase`, exigido para as duas partes
  da partial concordarem — CS0263 de outra forma). `UserAttributes` legado
  virou ponte `[Obsolete]` + `MergeLegacyAttributes` dentro do
  `OnParametersSet` existente (que já gerenciava o estado local de
  `Expanded`); o rail mode agora aplica `Style` e splat no raiz
  (`.sui-rail-group`), o non-rail já splattava no `nav`.
- **`SUINavLink`:** além da ponte `[Obsolete]` de `UserAttributes`, o
  parâmetro explícito `Attributes` foi PRESERVADO: ele tem destino
  deliberado diferente — vai para o elemento interativo interno
  (`<a>`/`<NavLink>`/`<span>`/`<button>`), não para o wrapper raiz. O splat
  canônico (`AdditionalAttributes`) permanece no wrapper.
- **`SUITabs`/`SUISlidingTabs`:** removidos `Class`/`AdditionalAttributes`
  duplicados; ganham `Style` no raiz (não publicavam).
- **`SUITabPanel`/`SUISlidingTabPanel`:** herdam `Class` da base — sem
  mudança de comportamento, pois quem consome `panel.Class` é o componente
  pai ao montar o painel ativo (`PanelClass`).
- **`SUIFilterScope`/`SUIFilterTree`:** componentes de lógica (cascata),
  sem elemento raiz; herdam a base pela consistência da família, sem
  splat no markup.
- **Gerador de catálogo:** `partial class … : SUIComponentBase` é a terceira
  forma de herança (além de `@inherits SUIComponentBase` e
  `SUINotificationHostBase<…>`); a regex do `generate-catalog.py` agora cobre
  as três, senão o `CatalogMetadataTests` falhava para o `SUINavGroup`.

## Validação

- Suíte unitária 933/933; build limpo.
- `generate-catalog --check` idempotente (73 componentes) +
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- `check:css` dentro do orçamento (raw 59.138 B).
- Pack + `validate-package.sh` (ApiCompat contra 2.26.918.1935 + smoke
  consumer net10.0 em root e pathbase) verdes.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): os 8
  migram de `ComponentBase` para `SUIComponentBase`.
- Navegador chromium local: catálogo + subpath + baselines visuais 59/59;
  showcase estático 110/110.
