# Changelog

Todas as mudanças relevantes deste pacote serão registradas neste arquivo.

O projeto segue [Semantic Versioning](https://semver.org/). Versões publicadas
são derivadas exclusivamente de tags Git no formato
`vMAJOR.MINOR.PATCH[-prerelease]`.

## [Unreleased]

## [2.2.0] — 2026-08-30

### Added

- `SUIDrawer` responsivo com breakpoint configurável, modo compacto em tela
  cheia, botão de fechamento, backdrop, fechamento após navegação, safe areas,
  controle de foco e atributos ARIA de diálogo.
- Testes unitários e de navegador para o drawer responsivo e para conteúdo
  textual no slot final de `SUIChoiceCard`.

### Fixed

- `SUIChoiceCard` diferencia o indicador circular de resumos textuais: no
  desktop o resumo recebe uma trilha dimensionada pelo conteúdo e, no mobile,
  ocupa uma linha própria sem comprimir título ou descrição.
- `SUINavLink` preserva navegação quando também possui callback, dispara o
  callback antes da navegação e renderiza links desabilitados sem destino
  interativo.
- O bundle compartilhado inclui os estados responsivos do drawer, bloqueio de
  rolagem, backdrop e tratamento de movimento reduzido.

## [2.0.0] — 2026-08-27

Primeira release da linha `net10.0`-only (o pacote exige tag v2+). Inclui
tudo acumulado em main: política global de quebra de texto, ícones
financeiros e header refinado, suites de acessibilidade/visual/performance
com baselines em CI, e a correção do build do catálogo de amostras.

### Fixed

- Política global de quebra de texto: `overflow-wrap: anywhere` em `:root`
  nas foundations — tokens indivisíveis (ids, URLs, JSON) nunca mais
  transbordam cards nem forçam trilhas de grid/flex além do contêiner;
  herda para toda superfície consumidora. Orçamento brotli do bundle
  elevado 8000→8100 para acomodar a regra.
- Catálogo de amostras volta a compilar sob `-warnaserror` (o build de
  main estava vermelho desde 25e5424): `SUITableSortLabel` requer `T`
  explícito quando nenhum parâmetro tipado é passado.

### Added

- Plano da próxima etapa de engenharia da SUI.
- Primitive responsiva `SUIFormGrid` e contratos de alinhamento.
- Visual regression light/dark, matriz de browsers e forced-colors.
- Smoke executável do pacote sob raiz e `PathBase`.
- Política de versionamento/TFMs e plano explícito da v2.
- Guarda automatizada contra dependência de biblioteca visual de terceiros
  (pacote, assembly, código, classes CSS e custom properties).
- Testes de convenção (prefixo `SUI`, namespaces, parâmetros, arquivos
  colocalizados), de tamanho de arquivo com débito congelado e de contrato de
  estilo (`!important`, z-index tokenizado, foco visível, OKLCH).
- Budgets de payload em bytes brutos, gzip e Brotli para o bundle CSS e os
  módulos JS.
- Suíte de acessibilidade em viewport mobile, dark, reduced-motion, espaçamento
  de texto (WCAG 1.4.12), ordem de tabulação, foco visível, landmarks e
  hierarquia de headings.
- Budgets de runtime no navegador (requests, DOM, bytes transferidos, LCP, CLS,
  estabilidade ao abrir overlays) e gate de Lighthouse no CI com
  `eng/lighthouse-budget.json` e `scripts/check-lighthouse.mjs`.

### Changed

- A linha de desenvolvimento passa a suportar somente `net10.0`; o próximo
  pacote deve ser publicado como v2 por remover o asset `lib/net9.0`.
- O stylesheet deixa de ler custom properties de biblioteca visual de terceiros:
  os componentes resolvem exclusivamente tokens `--sui-*`. Consumidores que
  dependiam do fallback precisam fornecer o tema via `SUIThemeProvider`.

- Builds locais usam a versão não publicável `0.0.0-local`.
- O pipeline de release passa a publicar apenas tags SemVer depois de todos os
  gates obrigatórios.
- O entrypoint CSS passa a ser um bundle único e minificado, sem `@import` em
  runtime.
- `SUIChoiceCard` passa a reservar colunas somente para conteúdo opcional
  realmente renderizado, mantém título/descrição legíveis e alinha o indicador
  de seleção ao início do conteúdo detalhado.
- O catálogo adota ritmo vertical consistente entre cabeçalhos, descrições e
  grupos de conteúdo em desktop e mobile.
- A palette separa o acento primário da superfície opcional de ações
  preenchidas; temas existentes mantêm o comportamento por fallback, enquanto
  o catálogo âmbar usa ember profundo e texto branco quente nos botões.

## Histórico anterior

As versões `1.26.*` anteriores à adoção deste changelog usavam versionamento
temporal. As evidências da última versão local validada permanecem em
`docs/CONSUMER-ROLLOUT.md`.
