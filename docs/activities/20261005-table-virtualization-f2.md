# Consolidação de design — F2: `SUITable` com `Virtualize` opt-in

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Item 11 do roadmap da avaliação (F2): `SUITable` materializava todas as
linhas (`@foreach` sobre `Items`) sem alternativa para listagens grandes.
Entrega o parâmetro `Virtualize` como opt-in; o caminho padrão não muda.

## Decisões

- **Um só renderizador de linha.** O `<tr>` (key, classe, estilo, `aria-label`,
  `tabindex`, `@onclick`, `@onfocus`) vive numa função local usada pelos dois
  caminhos — materializado e virtualizado. A fusão dos dois `<tr>` antigos
  (interativo × leitura) mantém o markup somente-leitura idêntico: os atributos
  interativos resolvem para `null` quando não há `OnRowClick`.
- **Índice original viaja com o item.** `Virtualize<T>` entrega ao
  `ItemContent` só o item; recuperar o índice por busca na lista erraria com
  valores duplicados. O corpo virtualizado usa um `record IndexedRow(Item, Index)`
  privado — classes de linha, roving tabindex e `aria-label` dependem do índice.
- **Spacers como `tr`.** O spacer padrão é `div`; dentro de `tbody` o parser
  HTML o joga para fora da tabela. `SpacerElement="tr"` mantém a geometria
  dentro do grupo de linhas.
- **O wrapper é o contêiner de rolagem.** Primeira tentativa (rolagem da própria
  página) materializou as 100 linhas da demo: sem um contêiner com altura
  finita o `Virtualize` não calcula uma janela. A classe
  `sui-table-wrapper--virtual` aplica `max-height: min(30rem, 60vh)` +
  `overflow-y: auto` e `table-layout: fixed`. Medido no navegador: 23 linhas no
  DOM de 100, spacer final de ~3.647 px.
- **Acessibilidade do scroller.** Contêiner rolável sem foco por teclado falha
  WCAG 2.1.1 — o axe pegou (`scrollable-region-focusable`) no
  `CaptureNewSurfaces` do showcase. Em tabela somente leitura o wrapper recebe
  `tabindex=0`, `role=region` e `aria-label` (`ScrollLabel`); com `OnRowClick`
  as linhas já são focáveis e o wrapper não ganha um segundo tab stop.
- **Orçamento de arquivo.** O `.razor` passou de 250 linhas (252). Em vez de
  subir o orçamento, os parâmetros e o ciclo de vida foram para
  `SUITable.razor.cs` (markup 78 linhas, code-behind ~200).
- **Custo em CSS:** 59.138 B raw / 10.853 gzip / 9.473 brotli, dentro dos
  orçamentos do script (sem ratchet).

## Limitações documentadas

- Exige modo de render interativo; página estática mostra só as linhas
  iniciais (prerender enumera toda a lista antes do circuito assumir).
- `ItemSize` errado causa jitter de rolagem, não perda de dados.
- Não combina com `StackOnMobile` (cartões de altura variável): a estimativa
  de altura deixa de valer. Preferir paginação nesse caso.

## Validação

- Suíte unitária 943/943 (+10: virtualização, wrapper focável, caminho padrão
  intocado, linhas interativas, vazio com `Virtualize`).
- `generate-catalog --check` idempotente (73 componentes) e
  `check-catalog-examples.py` compilando os 73 exemplos standalone.
- Baseline de API pública regenerado (`SUI_UPDATE_PUBLIC_API=1`): `ItemSize`,
  `OverscanCount`, `ScrollLabel`, `Virtualize`.
- Pack com ApiCompat (exit 0, sem CP) + `validate-package.sh` (consumer net10.0
  em root e pathbase).
- Chromium local: catálogo + subpath + a11y + baselines visuais + performance
  44/44; showcase estático 53/53, incluindo o novo teste
  `Patterns_VirtualizedTableKeepsARowWindowWhileScrolling`.
