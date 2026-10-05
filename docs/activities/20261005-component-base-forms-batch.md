# Consolidação de design — P1: família Forms completa; 100% na base

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Oitava leva da migração progressiva para `SUIComponentBase`
(pilotos em `20261004-…-component-base.md`; Layout/Actions/DataDisplay/
Feedback/Navigation/Overlays nas notas de 2026-10-05). Oito componentes
Forms restantes. **Correção:** a primeira versão desta nota (e do
CHANGELOG) afirmava que a leva completava 100% da base — incorreto: o
`SUIDrawer` (code-behind + interop JS, adiado desde o lote Layout) ainda
estava em `ComponentBase`. Ele é migrado no commit imediatamente
seguinte, que fecha a cobertura em 100% de `src/Components/`.

## Decisões

- **`SUIAutocomplete`/`SUIDateField`/`SUINumericField`:** `UserAttributes`
  legado (capturava unmatched) virou ponte `[Obsolete]` +
  `MergeLegacyAttributes` dentro do `OnParametersSet` que já existia
  (configuração do `SUIFieldBinding`). O destino do splat canônico é o
  mesmo elemento de antes: raiz do autocomplete, trigger do date field,
  input do numeric.
- **`SUIFormGrid`:** tinha parâmetro próprio
  `IReadOnlyDictionary<string, object?>? AdditionalAttributes` (tipo
  diferente da base). Removido em favor do `Dictionary` da base. **Essa
  troca de tipo é uma quebra de API deliberada** (o getter público muda de
  assinatura) e foi suprimida em `src/CompatibilitySuppressions.xml`
  (CP0002, commit seguinte) — o mesmo mecanismo usado para as quebras
  deliberadas anteriores. Nota de processo: a validação local de pack
  (`dotnet pack | grep`) escondeu o erro CP0002 porque o ApiCompat roda
  DEPOIS de criar os pacotes; só o CI o expôs. A validação local agora
  confere o exit code.

  ApiCompat verde porque a mudança é apenas de declaração (o parâmetro
  herdado mantém o mesmo nome; o tipo mudou de
  `IReadOnlyDictionary` para `Dictionary`, o que o pack aceita por
  atribuição implícita de dictionary expressions em markup Razor).
- **`SUISwitch`/`SUIChoiceCard`:** já canônicos; só removeram a duplicação.
- **`SUISwitchButton`:** renderiza um `SUIButton` interno e nada mais —
  `Class`/`Style`/splat são repassados ao botão interno (o raiz do
  consumidor É o botão).
- **`SUISelectItem`:** componente de registro no `SUISelect` (sem markup
  próprio); herda a base por consistência da família.

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
