# Consolidação de design — P1: família Overlays em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Sétima leva da migração progressiva para `SUIComponentBase` (pilotos em
`20261004-…-component-base.md`; Layout/Actions/DataDisplay/Feedback/
Navigation nas notas de 2026-10-05). Cinco componentes da família
Overlays, incluindo o host de diálogos com focus trap em JS.

## Decisões

- **`SUIDialogHost`:** o overlay raiz (`.sui-dialog-overlay`) era uma
  `div` fixa sem nenhuma superfície de customização. Agora usa
  `EffectiveClass("sui-dialog-overlay")` + `Style` + splat canônico. O
  painel do diálogo (`.sui-dialog.sui-card`) permanece intocado — é
  estrutura do sistema, o consumidor não deveria estilizá-lo por acidente.
- **`SUIConfirmDialog`/`SUIDecisionDialog`:** são fragmentos de conteúdo
  renderizados dentro do `DynamicComponent` do host, sem elemento raiz
  próprio. Herdam a base pela consistência da família (superfície
  disponível se algum dia precisarem), sem splat no markup.
- **`SUIPopover`/`SUITooltip`:** os dois únicos da família com
  `UserAttributes` legado capturando unmatched. Ponte `[Obsolete]` +
  `MergeLegacyAttributes` em `OnParametersSet` (padrão `SUILink`); o splat
  canônico vai para o mesmo elemento de antes (o anchor `span`).

## Validação

- Suíte unitária 933/933; build limpo.
- `generate-catalog --check` idempotente (73 componentes) +
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- `check:css` dentro do orçamento (raw 59.138 B).
- Pack + `validate-package.sh` (ApiCompat contra 2.26.918.1935 + smoke
  consumer net10.0 em root e pathbase) verdes.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): os 5
  migram de `ComponentBase` para `SUIComponentBase`.
- Navegador chromium local: catálogo + subpath + baselines visuais 59/59;
  showcase estático 110/110.
