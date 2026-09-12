# Changelog

Todas as mudanças relevantes deste pacote serão registradas neste arquivo.

O projeto segue o calendário Sufficit `1.yy.MMdd.HHmm` em UTC, derivado de
tags Git `v1.yy.MMdd.HHmm`. Entradas SemVer anteriores são histórico; não
definem a numeração das próximas publicações.

## [Unreleased]

### Removed (breaking)

- As 26 pontes `object`/`string` marcadas `[Obsolete]` foram removidas:
  `Color`, `IconColor`, `Variant`, `Size`, `IconSize`, `ButtonType`, `Edge`
  em `SUIButton`, `SUIIconButton`, `SUILoadingButton`, `SUIChip`,
  `SUITimelineItem`, `SUIProgressLinear`, `SUISwitch`, `SUICheckbox`;
  `SUIAlert.Severity` e `SUIStatusBadge.Tone`. Use os parâmetros tipados
  (`ColorValue`, `VariantValue`, `SizeValue`, `ButtonTypeValue`, `EdgeValue`,
  `IconColorValue`, `IconSizeValue`, `ToneValue`). Padrões visuais inalterados.
  Última versão com as pontes: `2.26.911.2323`. Guia em
  `docs/components/api-migration.md`.
- `SUIItem`: `xs`, `sm`, `md`, `lg`, `xl` renomeados para `Xs`, `Sm`, `Md`,
  `Lg`, `Xl` (parâmetros Blazor são PascalCase). Rename duro, sem período de
  transição: o Razor casa atributos de componente sem distinguir maiúsculas,
  então o nome antigo não pode coexistir como obsoleto. Troca mecânica:
  `rg -l '<SUIItem\b' --glob '*.razor' | xargs sed -i -E 's/(<SUIItem\b[^>]*\s)(xs|sm|md|lg|xl)=/\1\u\2=/g'`
  (repetir até não haver ocorrência).
- `NavAccordionScope` renomeado para `SUINavAccordionScope` (tipo interno ao
  `SUINavGroup`; nenhum consumidor conhecido o referenciava).

### Changed

- `SUIAlert`: `OnClose` substitui `CloseIconClicked`. O nome antigo foi
  publicado como encaminhador obsoleto em `2.26.911.2356` e removido na
  release seguinte, depois que todos os consumidores conhecidos migraram.

### Packaging

- Pacote inclui `Sufficit.Blazor.UI.xml` (IntelliSense), publica símbolos `.snupkg` com Source Link (`PublishRepositoryUrl`, `EmbedUntrackedSources`) e deixa de usar `PackageIconUrl`, obsoleto. Todos os 320 membros públicos que faltavam receberam `<summary>` (tokens de tema com o `--sui-*` que alimentam, enums com o efeito real, parâmetros com padrões e ARIA); `CS1591` passa a valer com warnings como erro. `SUIDateField.Calendar.cs` e `SUINavGroup.Rail.cs` nascem de divisões mecânicas para respeitar o teto de 450 linhas.
- Mensagens `[Obsolete]` das pontes não prometem mais "v2.0.0": apontam para `docs/PLAN-API-CLEANUP.md` (antigo `PLAN-SUI-V2.md`).
- Budgets de assets seguem a regra "medido × 1,03, arredondado para 256 B": JS Brotli 9,75 KiB, CSS isolation 27,75 KiB, gzip do bundle 10,25 KiB, transferência de CSS no catálogo 98 KiB; valores medidos registrados ao lado de cada teto.
- ASP.NET Core Components `10.0.12`; `Microsoft.NET.Test.Sdk` unificado em `17.14.1`.

- Linha de versão movida para o major 2: Debug `2.99.0.0`, Release/Packing
  `2.yy.MMdd.HHmm` UTC. As SemVer `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1` e `2.2.1`
  foram publicadas por engano, o NuGet.org não as apaga e `dotnet restore`
  resolve versões não listadas em ranges flutuantes; `1.yy` nunca venceria.
  Consumidores devem referenciar `Version="2.*"`. Tags Git das versões legadas
  removidas.
- Restaurado o padrão de versão do Sufficit.Identity.Core: Debug `1.99.0.0`,
  Release/Packing `1.yy.MMdd.HHmm` UTC; pacote, assembly e arquivo usam a mesma versão.
- CI rejeita SemVer legado e datas inválidas; limpeza NuGet limitada às cinco
  versões com numeração incorreta, preservando restores por versão exata.

### Added

- `SUIThemeProvider.Nonce`: copiado para o `<style>` inline que publica os tokens, para hosts com `Content-Security-Policy: style-src` estrita. Sem o parâmetro nada muda.
- Testes de contrato de render (bUnit) para os 22 componentes que não tinham nenhum, e `ReadmeCatalogTests`, que mantém a tabela de famílias do README igual a `src/Components`.
- `docs/components` passa a mencionar os 16 componentes que faltavam (CopyToClipboard, Avatar, TableSortLabel, TableEmpty, Td, ProgressCircular, SkeletonLoader, PendingChangesBar, ProgressSteps, CardActions, ConfirmDialog, DecisionDialog, SnackbarHost, FilterTree, FilterScope, TabPanel).

- Botões: variante `Soft` com superfície tonal, peso mais leve e raio do tema; disponível em SUIButton, SUILoadingButton e SUIIconButton. Filled/Outlined/Text mantêm seus valores e padrões.

### Fixed

- SUINavGroup e SUINavLink: `aria-disabled` era ligado a um `bool` e saía como `aria-disabled=""`; agora emite `"true"` quando desabilitado e omite o atributo caso contrário (tecnologia assistiva ignora o token vazio).

- SUIAutocomplete: o adorno (ícone de busca/loading) usa `display: flex`, para a caixa deslocada por `translateY(-50%)` ter a altura do glifo e não da linha de texto; o ícone fica centralizado no campo.

- Select e Autocomplete registram o estado enviado ao JavaScript antes de aguardar
  o retorno, evitando que chamadas concorrentes suprimam a rolagem da seleção
  por teclado ou reabram a lista (regressão reproduzida no WebKit).

- IconButton playground no longer offers a misleading “Show icon” toggle; its Add icon, accessible title and copied configuration stay consistent. The toggle remains available for text buttons.

- SUISwitch: o trilho desligado usa `--sui-color-secondary` em vez de `--sui-surface-3`, garantindo contraste mínimo de 3:1 contra a superfície nos dois temas (WCAG 1.4.11); antes o controle quase desaparecia em fundos claros. (#20)

- SUITextField helper text is slightly smaller and sits closer to its input, using component-scoped CSS and the existing caption/spacing tokens.

- Button showcase restores the Sufficit orange and automatically spaces action groups with nested stacks. Filled buttons retain the brand color with a subtle gradient; contrast is checked at both endpoints.

- Large nos botões mantém a tipografia de Medium; SUIButton usa ícone compacto por padrão, independente do alvo, respeitando IconSizeValue explícito. IconButton limita seu glifo a 1,5em.
- Filled mantém o fundo sólido com peso menor, cantos equilibrados, sombra curta e transições de estado mais discretas. O âmbar de ação da vitrine ficou menos saturado; Soft continua como alternativa.

- Alinhamento: removido o deslocamento vertical artificial dos ícones iniciais/finais dos botões. A vitrine permite comparar Suave e Preenchida e copiar a configuração.
- SUIButton respeita o alinhamento do conteúdo definido por Style e permite quebra de texto longo sem depender de seletores internos no consumidor.

- Limpeza por teclado devolve foco em TextField/Autocomplete; erros de checkbox
  referenciam o input e campos inválidos não apontam para mensagens inexistentes.
- Select e Autocomplete revelam a opção ativa sem rolar a página; Tab sai do menu.
  Autocomplete usa a camada de popover, preserva Enter do formulário durante seleção
  e cancela busca quando desabilitado.
- LoadingButton conserva largura com ícones, informa `aria-busy` e respeita Href.
  Botões acomodam texto longo; ícones herdam o contraste do texto por padrão.
- Botões semânticos usam os tokens de contraste de cada cor, inclusive em temas próprios.

- Tamanhos de botões e ícones agora reconhecem as classes `small`/`medium`/`large`
  emitidas pelos componentes; botões de ícone preservam a altura escolhida e o formato quadrado.
- `SUIChip` implementa o tamanho grande oferecido no playground; medidas são
  verificadas em navegador, com alvos mínimos de toque preservados.


### Added

- `SUIFormGrid.MinColumnWidth` (14rem): adaptação à largura do painel quando
  `StackOnMobile=true`; `false` preserva colunas fixas. Exemplos de interação
  copiáveis na vitrine cobrem painel estreito, listas longas e ações com loading.

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
