# Changelog

Todas as mudanças relevantes deste pacote serão registradas neste arquivo.

O projeto segue [Semantic Versioning](https://semver.org/). Versões publicadas
são derivadas exclusivamente de tags Git no formato
`vMAJOR.MINOR.PATCH[-prerelease]`.

## [Unreleased]

### Fixed

- Tamanhos de botões e ícones agora reconhecem as classes `small`/`medium`/`large`
  emitidas pelos componentes; botões de ícone preservam a altura escolhida e o formato quadrado.
- `SUIChip` implementa o tamanho grande oferecido no playground; medidas são
  verificadas em navegador, com alvos mínimos de toque preservados.


### Added

- `SUISwitch.HelperText`, `Id`, `ErrorText` e `Invalid`: ajuda e erros associados ao checkbox, preservando nome acessível, binding e aparência sem ajuda.

- `SUITabs.Vertical` com teclado por orientação e seleção horizontal sempre visível.
- `SUIColorContrast` para verificar pares de cores opacas.
- Playground de quatro componentes, composição operacional completa, editor avançado
  com comparação isolada de temas e medição reproduzível do carregamento WASM.

- Vitrine estática Blazor WebAssembly com 68 componentes reais, busca,
  exemplos copiáveis verificados por compilação, API por componente, padrões
  compostos e publicação GitHub Pages por Actions.
- Temas claro/escuro/sistema, presets, densidade, persistência e exportação C#
  na vitrine; presets públicos `SUITheme.Light`/`Dark` e contrastes semânticos.
- Integração opcional dos campos com `EditContext` via expressão de valor,
  validação/parsing e notificação de alterações, preservando a herança atual.
- `SUIAutocomplete.SearchFuncAsync` com `CancellationToken` e `SUITable.RowKey`.

### Changed

- Drawer compacto fechado inerte, foco visível em switches e contraste secundário escuro.
- Exportações copiadas refletem o código exibido; validação e alterações pendentes
  da vitrine acompanham o estado real. Documentação mostra a versão compilada.

- Exemplos extraídos para RCL compartilhada pelos hosts Server e WebAssembly.
- CSS compartilhado dividido por responsabilidade; build também sincroniza
  arquivos de compatibilidade. Budgets existentes preservados.
- Baseline de API atualizado sem remover assinaturas existentes; referências
  visuais atualizadas para a composição corrigida do layout no catálogo Server.

- Ícones `SUIIcons.Shield`, `SUIIcons.Bolt` e `SUIIcons.Devices`, no mesmo
  traço leve dos ícones de ação, com amostra no grupo de navegação do catálogo.

### Fixed

- Seletores de tema, paleta e densidade da vitrine usam `SUISelect`, com menu
  estilizado e navegação por teclado; opção selecionada mantém contraste no escuro.
- `SUISelect` ignora fechamento por blur antigo quando o foco já voltou ao
  controle, preservando o menu ao reabrir rapidamente.

- CodeQL init/analyze alinhados na versão 4.37.9 e agrupados no Dependabot
  para evitar atualizações parciais incompatíveis.

- `SUIStack` aplica `Style` e espaçamento zero; o exemplo de layout posiciona
  appbar, drawer e conteúdo em seus eixos corretos.
- `SUINumericField` omite limites não fornecidos e reporta parsing inválido ao
  formulário. Callback de texto não tem exceções mascaradas como erro de parsing.
- Snackbar usa o dispatcher do Blazor, limita mensagens visíveis e cancela
  expirações no descarte; cores semânticas preservam contraste no escuro.
- Tabela materializa a sequência uma vez por atualização dos parâmetros;
  autocomplete cancela a busca anterior e ignora resultados obsoletos.

- `SUIThemeProvider` publica os tokens também em `.sui-root[data-sui-theme]`.
  Antes, em modo escuro, o fallback das fundações em `[data-sui-theme="dark"]`
  vencia a palette do consumidor dentro do wrapper e todo tema escuro vestia o
  âmbar padrão; a cópia em `:root` continua servindo os portais no body.

## [2.2.1] — 2026-08-30

### Changed

- O orçamento de transferência CSS do catálogo passa a 92 KiB após medição
  Chromium de 89,3 KiB, acomodando os estados responsivos do drawer e dos
  choice cards com menos de 3 KiB de margem explícita.

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
