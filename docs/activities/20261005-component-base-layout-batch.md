# Consolidação de design — P1: lote Layout em SUIComponentBase

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Segunda leva da migração progressiva para `SUIComponentBase` (a primeira, de 8
pilotos, está em `20261004-design-consolidation-component-base.md`). Treze
componentes da família Layout: `SUIAppBar`, `SUICard`, `SUICardHeader`,
`SUICardContent`, `SUICardActions`, `SUIContainer`, `SUIDivider`, `SUIGrid`,
`SUIItem`, `SUILayout`, `SUIPageHeader`, `SUISpacer`, `SUIStack`. `SUIDrawer`
fica para o próximo lote (code-behind + interop JS, migração maior).

## Decisões

- **Novos pontos de splat:** `SUIAppBar`, `SUIStack`, `SUIGrid`, `SUILayout` e
  `SUIDivider` não capturavam atributos não modelados (estilo `id=` crashava em
  runtime, o mesmo acidente que `SUIContainer` já documentava). Com a base,
  todos recebem `@attributes="AdditionalAttributes"` no raiz — comportamento
  novo, aditivo e alinhado ao resto do sistema.
- **Família Card:** o dicionário `Attributes` (não capturava unmatched, era
  passado explicitamente) virou alias `[Obsolete]` no padrão de `SUILink`:
  merge via `MergeLegacyAttributes` em `OnParametersSet`, canônico vence.
  Testado em `AttributeForwardingTests` (ponte legada + caminho canônico).
- **`SUIDivider`:** antes concatenava `class="sui-divider @Class"` na mão — o
  zero-margin reset sumia quando o consumidor passava `Class`. Agora usa
  `EffectiveClass("sui-divider")`; a asserção de contrato em
  `StyleContractTests` foi atualizada para a forma canônica.
- **Baseline de API pública:** regenerado com `SUI_UPDATE_PUBLIC_API=1`
  (herança `ComponentBase → SUIComponentBase` nos 13; declarações de
  `Class/Style/AdditionalAttributes` saem da classe concreta e permanecem
  acessíveis via base). Pack verde contra o baseline de pacote publicado.

## Validação

- `dotnet build` da solução com 0 warnings/0 erros; suíte unitária 933/933.
- `generate-catalog.py --check` e `check-catalog-examples.py`: 73 componentes.
- `validate-package.sh`: ApiCompat + smoke consumer net10.0 verdes.
- Navegador (chromium, local): Catalog+Accessibility 37 ✔ (visual baselines
  comparados contra `docs/baselines/catalog` — sem drift de pixel, como
  esperado para uma migração sem mudança visual) e Showcase estático 52 ✔.

## Próximo

Migrar `SUIDrawer` e as famílias DataDisplay/Feedback/Navigation/Overlays em
lotes por família, encolhendo a janela até remover os aliases congelados.
