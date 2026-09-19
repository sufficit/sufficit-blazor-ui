# Avaliação independente: Sufficit.Blazor.UI (SUI)

| Campo | Valor |
|---|---|
| Data | 2026-09-19 |
| Modelo | Claude Fable 5.1 (`claude-fable-5-1`) |
| Revisão avaliada | `91f016e` (branch `main`, árvore limpa) |
| Método | Leitura integral do código-fonte (`src/`, `samples/`, `tests/`, `scripts/`, `eng/`, `.github/`), build Release com `-warnaserror`, execução dos testes unitários e de navegador, inspeção do pacote publicado no NuGet.org, consulta à API do GitHub para o estado de proteção do repositório, pesquisa web sobre concorrentes. Quatro agentes de leitura por área e um de pesquisa de mercado, com cada achado de severidade Alta ou Crítica reverificado manualmente no código. |
| Regra dura | Nenhum documento do próprio projeto foi lido (README, CHANGELOG, DESIGN.md, PRODUCT.md, `docs/`, `skills/`, `.impeccable/`). Comentários e XML docs foram tratados como afirmações a verificar, não como fatos. Onde o comentário e o código divergem, o código venceu e a divergência aparece como achado (seção 1.13). |

Convenção de referência: `arquivo:linha` relativo à raiz do repositório. Severidades: **Crítico** (exploração direta ou perda de dados sem pré-condição improvável), **Alto** (exploração ou falha grave sob cenário realista), **Médio** (defeito visível ao usuário ou violação normativa), **Baixo** (ruído, dívida, inconsistência).

---

## 1. Reconhecimento: o que a biblioteca realmente é

### 1.1 Forma do repositório

- Solução `Sufficit.Blazor.UI.slnx` com 1 biblioteca (`src/`), 3 amostras (`samples/`) e 2 projetos de teste (`tests/`).
- Biblioteca: Razor Class Library `net10.0`, `Microsoft.NET.Sdk.Razor`, dependências únicas `Microsoft.AspNetCore.Components` e `.Web` 10.0.12 fixas (`src/Sufficit.Blazor.UI.csproj:75-76`). Nenhum framework de UI de terceiros. `Directory.Build.props` e `Directory.Packages.props` existem só para isolar a árvore de um worktree irmão (Identity).
- Tamanho: 69 componentes `.razor` em 7 pastas (Actions 5, DataDisplay 15, Feedback 13, Forms 16, Layout 15, Navigation 15, Overlays 5) mais `SUIThemeProvider`; 642 declarações `[Parameter]`; 12 módulos JS colocalizados; 37 arquivos CSS fonte; 13.707 linhas em `src/` (excluindo saída gerada), 7.259 linhas de teste, 2.814 linhas de amostras.
- Histórico: 260 commits entre 2026-08-09 e 2026-09-19 (seis semanas), 230 de um único autor, 15 do bot do GitHub Actions, 9 do Dependabot. 27 versões publicadas no NuGet.org no mesmo período.
- Consumidores reais em `/mnt/sufficit`: 20 projetos em 13 repositórios (identity, ai, ai-genius, blazor, fleet, checkout, background, network-control, services-run, telephony-panel, cloud-mobile). Dez deles usam faixa flutuante `2.*`, dois fixam `2.26.911.2323`, três via `Directory.Packages.props` com `2.*`.

### 1.2 Taxonomia e composição dos componentes

**Não existe classe base.** Cada componente deriva diretamente de `ComponentBase` (o único `partial class` explícito é `SUINavGroup`, `src/Components/Navigation/SUINavGroup.razor.cs:17`). Consequência medida:

| Preocupação transversal | Como é feita hoje | Quantas cópias |
|---|---|---|
| Classe CSS do consumidor | parâmetro `Class` (66 componentes), `CssClass` (`SUIIcon`), nenhum (`SUISelect`, `SUIDateField`, `SUIAutocomplete`, `SUIDrawer`, `SUINavGroup`, `SUIDialogHost`…) | 3 convenções |
| Atributos não mapeados | `UserAttributes` (Forms, Actions, Tooltip, NavLink), `AdditionalAttributes` (Feedback, DataDisplay, Tabs, Drawer), `Attributes` (Card*, NavLink também), `IReadOnlyDictionary` só em `SUIFormGrid` | 3 nomes, 2 tipos |
| Posição do splat `@attributes` | por último (vence `class`, `id`, `role`, `aria-*`, `type`, `on*`) na maioria; primeiro (perde `class`/`style` do consumidor) em `SUITooltip`, `SUINavGroup`, `SUINavLink`, `SUIFormGrid` | 2 semânticas opostas |
| Id efetivo do campo | `Id.Trim()`, `Id` cru, `Id ?? _id`, `IsNullOrWhiteSpace` | 4 variantes (`SUISelect.razor.cs:97`, `SUITextField.razor:157`, `SUISwitch.razor:50`, `SUICheckbox.razor:102`) |
| `aria-describedby` composto | função privada por componente | 6 cópias |
| Import do módulo JS + descarte | `OnAfterRenderAsync` + `DisposeAsync` com `catch (JSDisconnectedException)` | 12 cópias, 3 delas com gate `_disposed` diferente |
| Strings de UI | literais pt-BR ("Limpar seleção", "Fechar", "Sim", "Não", "Cancelar", "Nenhum registro") | espalhadas em 14 arquivos, sem serviço de localização |

Composição real: `RenderFragment` para conteúdo (ChildContent, `ItemTemplate<T>`, `RowTemplate<T>`, slots de cabeçalho de card), genéricos onde há valor (`SUISelect<T>`, `SUIAutocomplete<T>`, `SUITextField<T>`, `SUINumericField<T>`, `SUIChoiceCard<TValue>`, `SUITable<T>`, `SUITableSortLabel<T>`), valores em cascata para registro pai-filho (`SUISelect` cascateia a si mesmo como `ISUISelectRegistry`, `SUITabs`/`SUISlidingTabs` idem, `SUIFilterTree`, `SUINavGroup` com dois valores nomeados `SufficitRailMode`/`SufficitRailFlyoutMode` e um `SUINavigationContext`), e um `DynamicComponent` no host de diálogo. Enumerações compartilhadas em `src/Components/SUIEnums.cs` (`SUIColor`, `SUIVariant`, `SUISize`, `SUITone`, `SUIEdge`, `SUIButtonType`), mas vários parâmetros continuam `string` onde caberia enum (`SUIDrawer.Variant`, `SUIContainer.MaxWidth`, `SUIAppBar.Color`, `SUIStack.AlignItems/Justify`, `SUITimeline.TimelinePosition`, `SUINavLink.ActiveClass`).

### 1.3 Camada de formulários

- Ligação bidirecional padrão `Value`/`ValueChanged`/`ValueExpression`; o componente nunca muta `Value` (exceção: `SUISwitchButton` muta `Checked`, `src/Components/Forms/SUISwitchButton.razor:39`).
- Integração com `EditContext` via `[CascadingParameter] EditContext? FormContext` e o utilitário interno `SUIFieldBinding<T>` (`src/Utilities/SUIFieldBinding.cs`), que reassina em `Configure`, mantém um `ValidationMessageStore` próprio e expõe `Error` (erro de parse ou primeira mensagem de validação). Faz `FieldIdentifier.Create` em todo `OnParametersSet` (`:20`).
- Ids gerados com `Guid.NewGuid():N` e derivados `-label/-helper/-error/-menu/-listbox/-calendar/-status/-option-{i}`.
- `SUISelect<T>`: gatilho `<button role="combobox">` + listbox em `popover="manual"` com `showPopover()` e posicionamento fixo com inversão em JS; `SUISelectItem` não renderiza nada, só se registra. Comparação de valor por `Equals(object, object)` entre `SUISelectItem.Value : object?` e `T` (`SUISelect.razor.cs:126`).
- `SUIAutocomplete<T>`: input + listbox; reexporta o módulo do Select (`SUIAutocomplete.razor.js:1`) para compartilhar o `activeMenu`; debounce por `Task.Delay` com `CancellationToken`.
- `SUIDateField`: grade de calendário própria (`SUIDateField.Calendar.cs`), `DotNetObjectReference` para `CloseFromJs`, input oculto ISO com `name` (`SUIDateField.razor:36-39`). `Today` vem de `DateTime.Today` do servidor (`SUIDateField.razor.cs:112`).
- `SUINumericField<T>`: `type="number"`, roda do mouse/setas/spinner configurados via JS a cada render.
- Realidade em SSR estático: `SUICheckbox`, `SUIChoiceCard` e `SUIDateField` emitem `name`; `SUITextField`, `SUINumericField`, `SUISwitch` não; `SUISelect` e `SUIAutocomplete` não renderizam controle nativo algum. Nenhum componente deriva `name` de `ValueExpression` como faz o `InputBase<T>` do framework. A biblioteca não contém nenhuma referência a `RendererInfo`, `AssignedRenderMode`, `SupplyParameterFromForm`, `FormName` ou `data-permanent`.

### 1.4 Overlays e serviços

- `AddSufficitUI` (`src/ServiceCollectionExtensions.cs:24-37`) registra **Scoped**: a instância única de `ISUITheme` (capturada na configuração), `ISUISnackbar → SUISnackbarService`, `ISUIDialogService → SUIDialogService`.
- Diálogos: `ShowAsync<T>` cria `SUIDialogRequest` (tipo, título, parâmetros, `SUIDialogReference` com `TaskCompletionSource<object?>`), evento com fila de replay até o host assinar (`SUIDialogService.cs:23-50`). `SUIDialogHost` renderiza um único `DynamicComponent`; abrir um segundo diálogo fecha o primeiro com `null` (`SUIDialogHost.razor.cs:68-72`). Resultado não tipado. JS: foco inicial no primeiro focável (o botão X), armadilha de Tab e Escape em `document` fase de captura, restauração de foco com três tentativas.
- Snackbar: evento sem fila e sem lock (`SUISnackbarService.cs:14-25`); mensagens antes do host assinar são perdidas. Host com `aria-live="polite" aria-atomic="true"`, limite de 5 entradas, expiração por `Task.Delay`.
- `SUIToast` é um componente visual isolado, não ligado ao serviço.
- `SUITooltip`: portal singleton `#sui-tooltip-portal` no `body`, texto via `textContent`, posicionamento por `getBoundingClientRect` com candidatos de inversão. Sem CSS anchor positioning.

### 1.5 Navegação e layout

- `SUILayout`, `SUIAppBar`, `SUIDrawer` não trocam contexto; o consumidor liga `@bind-Open`. O drawer usa `matchMedia` em JS com breakpoint parametrizado (900) enquanto o CSS do app shell usa `900px` fixo (`sui-shared-app-shell-…css:112`).
- `SUINavGroup` tem modo "rail" com flyouts; coordenação entre grupos por **evento estático de processo** `private static event Action<SUINavGroup>? RailFlyoutOpened` (`SUINavGroup.Rail.cs:42`).
- `SUINavLink` tem quatro ramos de render (`<button>`, `<a>` com `NavigateTo(forceLoad:true)`, `<span>` desabilitado, `NavLink` do framework) e dois dicionários de atributos (`Attributes` e `UserAttributes`).
- Tabs: roving tabindex e setas em .NET; só o painel ativo é renderizado (estado de formulários é perdido ao trocar de aba).

### 1.6 Tema, fim a fim

`ISUITheme` = `SUIPalette` + `SUITypography` + `SUILayout` (records de **strings cruas**) + `IsDark` (`src/Themes/ISUITheme.cs`). `SUIThemeProvider.razor` resolve `Theme` (parâmetro) ?? DI ?? `DefaultSUITheme.Instance`, envolve o conteúdo em `<div class="sui-root" data-sui-theme="dark|light">` e emite `<style nonce="…">` com ~80 propriedades `--sui-*` concatenadas por `StringBuilder` e injetadas via `(MarkupString)` (`SUIThemeProvider.razor:9,52-144`), sob o seletor `:root,.sui-root[data-sui-theme]`. Claro/escuro é decidido pelo `IsDark` do tema; não existe `prefers-color-scheme` em nenhum lugar da biblioteca (a preferência do sistema fica a cargo do host, como faz `samples/…/Showcase/wwwroot/theme.js`). `sui-foundations.css:15-121` traz o fallback claro em `:root` e `:123-147` o escuro em `[data-sui-theme="dark"], :root.theme-dark, .theme-dark`. Portais anexados ao `body` leem a cópia de `:root`.

### 1.7 CSS: autoria, build, entrega, escopo

- Fonte: `src/styles/sui-entry.css` importa foundations, portais, 7 folhas de componente e `sui-components.css`, que importa 26 folhas `sui-shared-*`. `scripts/build-css.mjs` roda `lightningcss` (1.33.0, fixado, lockfile presente, `npm audit` limpo) com `bundle({minify:true})` para `src/wwwroot/sufficit-ui.css` e, além disso, **copia os 37 fontes sem minificar** para `src/wwwroot/styles/` (`build-css.mjs:60-72`). `--check` compara bytes com o commitado. Verificado: `node scripts/build-css.mjs --check` passa; bundle = 55.437 bytes brutos, 10.212 gzip, 8.937 brotli.
- Escopo: duas estratégias convivem. Classes globais prefixadas `.sui-*` (a maioria) e isolamento CSS `.razor.css` (17 arquivos: `SUITable`, `SUITimeline`, `SUIEmptyState`, `SUIPendingChangesBar`, `SUIStatusBadge`, `SUIAutocomplete`, `SUIDateField`, `SUIChoiceCard`, `SUIFormGrid`, `SUINumericField`, `SUISwitch`, `SUICardHeader`, `SUIPageHeader`, `SUITabs`, `SUISlidingTabs`, `SUICopyToClipboard`, `SUIDecisionDialog`). Dentro de `SUITable` as regras base são escopadas com `::deep` e a apresentação móvel e o botão de ordenação são globais.
- Entrega: o pacote publicado (2.26.918.1935, 260,9 KB) embarca **três formas** do mesmo CSS: `sufficit-ui.css` (55 KB), `styles/*.css` (37 arquivos, ~90 KB, sem nenhum referenciador em `src/` ou `samples/`) e `Sufficit.Blazor.UI.<hash>.bundle.scp.css` (37 KB). Consumidores linkam `_content/Sufficit.Blazor.UI/sufficit-ui.css` e o `{App}.styles.css` do próprio app (que importa o bundle escopado). O nome `sufficit-ui.css` não é fingerprintado; as amostras usam `@Assets[]` só para o CSS delas.

### 1.8 Interop JS

Padrão uniforme e correto na forma: cada componente importa o módulo colocalizado em `OnAfterRenderAsync` via `JS.InvokeAsync<IJSObjectReference>("import", "./_content/Sufficit.Blazor.UI/…")`, guarda `_module`, descarta em `DisposeAsync` engolindo `JSDisconnectedException`. Três componentes criam `DotNetObjectReference` (DateField, Drawer, DialogHost); quatro métodos `[JSInvokable]`. Verificado por grep: nenhum `window.*` global, nenhum `innerHTML`/`insertAdjacentHTML`/`eval`. Estado JS em `WeakMap` de módulo. Cache-busting manual `?v=2`/`?v=1` só em Tabs, SlidingTabs e Select. `SUILoadingButton.razor.js` registra um listener permanente de `click` em captura no `document`.

### 1.9 API pública e guardas

- `eng/PublicApiBaseline.txt`: 1.578 linhas geradas pelo próprio teste por reflexão (`tests/…/PublicApiCompatibilityTests.cs:77-142`), regeneradas com `SUI_UPDATE_PUBLIC_API=1`. A comparação é `baseline − atual`: **detecta remoções, nunca adições**. ~25 % das linhas são interfaces herdadas (`IComponent`, `IHandleEvent`, `IHandleAfterRender`) e `ComponentBase`, ruído que não é API da biblioteca.
- `EnablePackageValidation=true` sem `PackageValidationBaselineVersion` (`csproj:50`): com um único TFM, valida efetivamente nada contra a versão anterior.
- **Nenhum** `TreatWarningsAsErrors`, `AnalysisLevel`, `AnalysisMode`, `EnforceCodeStyleInBuild` ou `.editorconfig` no repositório. `-warnaserror` existe só nas linhas de comando do CI. Build local aceita CS1591 e avisos de nulidade. Confirmado: `dotnet build -c Release --no-incremental -warnaserror` passa com 0 avisos.
- Testes de contrato em xUnit lendo o repositório: convenção de nomes, orçamento de linhas por arquivo, orçamento de bytes gzip/brotli do CSS, ausência de framework de terceiros, sincronia README↔catálogo, sincronia `catalog.json`↔parâmetros.

### 1.10 Versionamento, empacotamento e publicação

- Versão: `VersionSuffix` é usado como versão inteira. Fora de Debug e sem suffix vindo do CI: `2.$(UtcNow:yy.MMdd.HHmm)`; Debug: `2.99.0.0`. `AssemblyVersion = Version` (muda a cada release). NuGet normaliza zeros à esquerda (`2.26.0918.1935` vira `2.26.918.1935`); as tags Git misturam os dois formatos (`v2.26.912.1208` e `v2.26.0918.1935`).
- CI (`.github/workflows/build.yml`): jobs css, build, pack, component-tests, browser-tests (matriz chromium/firefox/webkit contra o Catalog em `/` e `/app`), lighthouse; `publish` só em `refs/tags/v*`, após `pack` + `validate-package.sh`, com **Trusted Publishing OIDC** (`NuGet/login@v1`). Ações de terceiros fixadas por SHA. Sem `pull_request_target`. Nenhum `${{ github.event.* }}` chega a `run:`. `scripts/release_version.py` valida a tag (regex estrita com calendário) e rejeita qualquer sufixo: **não existe canal pré-release**.
- `validate-package.sh` verifica formato de versão, 11 entradas obrigatórias (só 6 dos 12 `.razor.js`), ausência de `lib/net9.0`, presença do bundle escopado, e faz um smoke test real: cria um consumidor RCL e um app Blazor Server a partir do feed local, sobe o app em `/` e `/app`, faz curl do HTML, do CSS global, de dois módulos JS e verifica que o `.styles.css` do host importa o bundle SUI. Versão do ASP.NET Core hardcoded `10.0.12` (`:77`).
- Estado real de proteção (API do GitHub em 2026-09-19): ambiente `production` com `protection_rules: []`, `can_admins_bypass: true`; `main` sem revisão obrigatória, `enforce_admins: false`; zero rulesets; sem proteção de tags. `maintenance.yml` (que deslistaria as versões legadas com um `NUGET_API_KEY` de vida longa) **nunca rodou**; as 27 versões, incluindo 1.27.0, 1.28.0, 2.0.0, 2.1.1 e 2.2.1, continuam listadas.
- Consequências para faixas flutuantes: `2.*` resolve para a maior data (funciona, mas recebe **toda** tag no instante em que é publicada, sem estágio de pré-release); `1.*` resolve para `1.28.0` para sempre (publicado em 2026-08-17 acima de qualquer `1.26.x` de calendário); a `Packing` configuration declarada no csproj não é usada em lugar nenhum; `CatalogBuildInfo.IsDevelopment` testa prefixo `0.0.0` enquanto o sentinela real é `2.99.0.0`.

### 1.11 Amostras

- **Catalog**: Blazor Web App com `InteractiveServer` global e prerender (`samples/…/Catalog/Components/App.razor:19,23`); páginas-fixture para os testes de navegador; suporta `PathBase`. Referencia a biblioteca por projeto.
- **Showcase**: WebAssembly standalone publicado no GitHub Pages por `pages.yml` + `scripts/prepare-pages.py`; editor de tema no navegador; `theme.js` clássico com `localStorage`.
- **Demos**: RCL com um `Examples/<Componente>Example.razor` por componente (70 = 70), `DemoBase.cs` com estado compartilhado. `scripts/generate-catalog.py` extrai `[Parameter]` + `<summary>` por regex e gera `catalog.json` (177 KB, recurso embutido) e `DemoCatalog.Generated.cs`; `check-catalog-examples.py` compila cada exemplo extraído. Esses dois checks rodam **só em `pages.yml`**, que não dispara em tags.
- Modos de render exercitados: InteractiveServer e WebAssembly. **Não** exercitados: InteractiveAuto, SSR estático sem interatividade, streaming, WebView/MAUI (embora `sufficit-ai-genius` consuma a biblioteca em Blazor Hybrid).
- Nenhum segredo, telemetria, analytics ou endpoint externo nas amostras.

### 1.12 Testes

| Camada | Ferramenta | Volume | Resultado local | No CI |
|---|---|---|---|---|
| Unitário + render | xUnit 2.9.3 + bUnit 2.9.0 (`JSRuntimeMode.Loose`) | 698 | 698 aprovados, 0,7 s | sim |
| Contrato de repositório | xUnit lendo arquivos | incluído acima | | sim |
| Navegador + axe | NUnit 4.3.2 + Playwright 1.62 + Deque.AxeCore | 110 casos | 57 aprovados, 52 pulados (Showcase só em Pages), **1 falha**: baseline visual `expected 1440x5380, actual 1440x5398` | matriz 3 navegadores |
| Visual | 4 PNGs de página inteira, diff de pixels em canvas, tolerância 0,5 % | 1 | falha fora do runner exato | chromium; refresh commitado direto em `main` pelo bot (5 vezes em 10 dias) |
| Performance | `performance` API + Lighthouse 12 (`npx --yes lighthouse@12`, não fixado) com orçamento `eng/lighthouse-budget.json` (perf ≥ 0,95; a11y/BP/SEO = 1,0) | 5 + 1 | | sim |
| Scripts Python | unittest | 12 | | sim |

Cobertura de código não é coletada (coverlet referenciado, nunca invocado). Os testes de navegador varrem o estado inicial do catálogo; nenhum abre diálogo, drawer, flyout ou tooltip, então o axe nunca audita overlays.

### 1.13 Divergências comentário ↔ código (achados por si)

| Onde | O comentário/doc diz | O código faz |
|---|---|---|
| `src/Sufficit.Blazor.UI.csproj:51-53` | "with warnings as errors, CS1591 keeps it that way" | nada no csproj; só no CI |
| `src/Components/DataDisplay/SUIIcon.razor:115-116` | "only library-defined, trusted markup belongs here" | qualquer string iniciada por `<` é renderizada crua |
| `src/Components/Layout/SUIDrawer.razor.cs:9-12` | armadilha de foco | seletor omite `input/select/textarea` (`SUIDrawer.razor.js:16`) |
| `src/Components/Forms/SUISelect.razor.cs:10` | aceita `Guid` por parse | exibição compara `Equals(object,T)` e nunca casa |
| `src/Components/DataDisplay/SUITableSortLabel.razor:4-7` | `aria-sort` é responsabilidade do consumidor | `SUITh` não tem parâmetro para isso |
| `src/Components/Feedback/SUIAlert.razor:40-41` | ícone implícito por tom | nunca renderizado |
| `tests/…/RenderContractLayoutTests.cs:8-9` | primitivos de layout não capturam atributos | `SUISection`, `SUIPageHeader`, `SUICard*` capturam |
| `tests/…/CatalogBrowserTests.cs:407` | "End only moves the caret" | o teste pressiona ArrowDown; o código captura End |
| `samples/…/CatalogBuildInfo.cs:12` e `scripts/test_release_version.py:15` | sentinela `0.0.0` | csproj usa `2.99.0.0` |
| `src/Components/Navigation/SUISlidingTabPanel.razor:16-17` | ícone confiável | mesmo sink de `SUIIcon` |

---

## 2. Vulnerabilidades e defeitos

Cada item traz cenário concreto e remediação de nível de projeto. Itens marcados **[op]** são puramente operacionais.

### 2.1 Superfícies de injeção

| # | Sev | Achado | Cenário | Remediação |
|---|---|---|---|---|
| I1 | **Crítico** | Valores do tema entram em `<style>` sem validação nem codificação: `SUIThemeProvider.razor:143-144` concatena e `:9` injeta via `MarkupString`. `SUIColorContrast.IsHexColor` existe e nunca é usado por `src/`. | Cores de marca por tenant vindas do banco (o Showcase já lê Success/Error de input, `ShowcaseTheme.cs:32-33`). `Primary = "#fff}</style><script>…"` executa em prerender/SSR; `<img onerror>` executa em render interativo; CSS puro (`url()` exfiltração, `@import`, restilizar `.sui-*` para spoofing) funciona sempre. | Introduzir `SUIThemeCssWriter` (mapeamento único e testável) que só aceita tipos `CssColor`, `CssLength`, `CssFontStack`, `CssShadow`, `CssTiming` com gramática estrita e fallback para o default em falha; `SUIPalette` passa a ser tipada, falhando na construção. Escape hatch explícito `CssValue.Unsafe(string)`. Trade-off: quebra consumidores que passam `oklch()`/`light-dark()` até a gramática cobri-los. |
| I2 | **Alto** | Ícones são strings que podem ser markup: `SUIIcon.razor:14,128`, `SUIButton.razor:146,153`, `SUIIconButton.razor:128`, `SUILoadingButton.razor:54,59,125-126` (interpola dentro de `<svg>`), `SUINavLink.razor:188`, `SUINavGroup.razor:86,124`, `SUITableEmpty.razor:5`, `SUIEmptyState.razor:8` (nem checa `<`), `SUIProgressCircular.razor:19`, e por encaminhamento `SUITextField.AdornmentIcon`, `SUIAutocomplete.AdornmentIcon`, `SUIChip.Icon`, `SUIAlert.Icon`, `SUIStatusBadge.Icon`, `SUIStat.Icon`, `SUICardHeader.Icon`, `SUISlidingTabPanel.Icon`. | Menu, abas ou categorias montados a partir de tabela editável com coluna `icon` → XSS armazenado via `<svg onload>`, `<foreignObject>`, `<image onerror>`. | Tipo selado `SUIIconRef` (`readonly record struct`) construível só por `Named("trash")` ou `Path(d, stroke…)` com dados de caminho parseados; constantes de `SUIIcons` viram `SUIIconRef`; `SUIIcon` renderiza `<path d=@d>` por `AddAttribute` (codificado), nunca `MarkupString`. Manter `[Obsolete] SUIIconRef.Markup(string)` por um release atrás de um sanitizador com allowlist (`path|circle|rect|g|line|polyline|polygon`, sem `on*`/`href`). Teste: string com `<script` vira texto. |
| I3 | **Alto** | Splat `@attributes` por último vence o contrato do componente: `SUITextField.razor:26,44`, `SUINumericField.razor:27`, `SUISelect.razor:33` (`role`, `aria-expanded`, `id`), `SUIDateField.razor:29`, `SUIToast.razor:4`, `SUIDrawer.razor:23`, `SUITabs.razor:6`, `SUISlidingTabs.razor:11`, `SUIProgressSteps.razor:4`, `SUICard*.razor:3`, `SUIPageHeader.razor:3`, `SUISection.razor:3`, todos os Actions. Splat primeiro descarta `class`/`style` do consumidor em `SUITooltip.razor:8-10`, `SUINavGroup.razor:57-60`, `SUINavLink.razor:5-7`. | `class="mt-2"` remove `sui-field__input` (controle sem estilo); `id="x"` quebra `<label for>` e a cadeia `aria-describedby`; um atributo `onclick` vindo de dados vira handler inline (XSS/CSP). `<SUIDrawer role="navigation">` apaga a semântica de diálogo. | Política única `SUIAttributes.Merge` na classe base (seção 4): splat primeiro em toda parte, `class`/`style` concatenados, `id` honrado como `EffectiveId`, `aria-describedby` anexado, recusa de `type`/`role`/`href`/`on*`. Um único nome `AdditionalAttributes`; `Attributes` e `UserAttributes` marcados `[Obsolete]`. |
| I4 | Médio | `Href` não validado em `SUINavLink.razor:150,171,218` (`NavigateTo(Href, forceLoad:true)` executa `javascript:`), `SUIButton.razor:22`, `SUIIconButton.razor:25`, `SUILoadingButton.razor:25`, `SUILink.razor:3`. `SUILink` não adiciona `rel=noopener` como `SUIButton.razor:88-96` faz. `SUIButton` renderiza `rel=""`. | Links de navegação alimentados por CMS/tenant. | `SUIUrl.IsSafeHref` (relativo, `http(s)`, `mailto`, `tel`) aplicado por todos os renderizadores de `Href`; `rel=noopener` automático em `SUILink`. |
| I5 | Médio | `style` inline a partir de parâmetros: `SUISkeletonLoader.razor:15` (`Size`), `SUITable.razor:37,48` (`RowStyleFunc` cru), `SUITooltip` (14 parâmetros de estilo copiados para o portal), `TooltipClass` aplicado ao portal no `body` (`SUITooltip.razor.js:123-126`). | Injeção de CSS/classe a partir de dados; quebra de CSP. | Tipos `CssLength`/`CssStyle` de I1; tooltip recebe um `SUITooltipStyle` tipado. |
| I6 | Médio | O `Nonce` de `SUIThemeProvider` dá falsa confiança em CSP estrita: a biblioteca exige `style-src 'unsafe-inline'` por causa de `style=` inline em `SUIPagination.razor:4,10`, `SUIStat.razor:3`, `SUIText.razor:84`, `SUISkeletonLoader.razor:15,21`, `SUIProgressLinear.razor:7`, `SUIAvatar.razor:8,17`, `SUIAlert.razor:3`, `SUITextField.razor:17,30,49,60,73`, `SUINumericField.razor:48`. | Host com `style-src 'self' 'nonce-…'` vê paginação, estatísticas e campos de texto sem layout. | Mover layout para classes (`.sui-pagination`, `.sui-stat` hoje não têm CSS); `SUIText.Color` emite as classes `.sui-color-*` existentes; barra de progresso via `el.style.setProperty` em JS (CSSOM é isento de CSP) ou custom property. Teste que enumera atributos `style` no render. |
| I7 | Baixo | `SUISnackbarHost.razor:9` interpola severidade não validada em classe; `SUITimeline.TimelinePosition` string → classe. | Injeção de classe apenas. | Enum `SUISeverity`; enum de posição. |

### 2.2 Fronteira de confiança do interop JS

| # | Sev | Achado | Cenário | Remediação |
|---|---|---|---|---|
| J1 | **Alto** | Importação de módulo sem `try/catch` em `OnAfterRenderAsync`: `SUIDialogHost.razor.cs:38`, `SUITooltip.razor:141`, `SUITabs.razor:138`, `SUISlidingTabs.razor:155`, `SUIDrawer.razor.cs:95`, `SUINavGroup.razor.cs:65`, mais Forms. Só `SUILoadingButton.razor:152-164` protege. | 404 transitório, CSP bloqueando o módulo ou timeout de rede → "Unhandled exception rendering component", circuito inteiro morre. | Helper `SUIJsModule` (`Lazy<Task<IJSObjectReference?>>`, `try/catch`, `ILogger`, flag `IsAvailable`, `DisposeAsync` idempotente capturando `JSDisconnectedException`, `ObjectDisposedException`, `TaskCanceledException`) de posse da classe base. Componentes degradam para o caminho sem JS. |
| J2 | Médio | Corridas de descarte: `SUINumericField.razor:64-90` (`Dispose()` marca `_disposed`; `OnAfterRenderAsync` pendente descarta `_module` mas deixa o campo; `DisposeAsync` invoca em referência descartada e só captura `JSDisconnectedException`); `SUISelect.Interop.cs:16` (import pendente atribui `_module` depois do descarte → referência JS vazada); `SUIDateField.razor.cs:200-208` (sem gate; listener `pointerdown` em `document` fica com `DotNetObjectReference` descartado); `SUIDialogHost.razor.cs:146→80-84→114-134` (fire-and-forget em componente descartado); `SUINavGroup.Rail.cs:106,128`; `SUIFilterTree.razor:75,85`. | Navegação rápida entre páginas em Blazor Server → `ObjectDisposedException` não observada; em alguns casos derruba o circuito. | O mesmo `SUIJsModule`; `CancellationTokenSource` de vida do componente cancelando imports e delays pendentes. |
| J3 | Médio | Escape engolido em `document` fase de captura por `SUIDialogHost.razor.js:71-77,102` e `SUIDrawer.razor.js:10-14,25`; Select/Autocomplete/DateField tratam Escape no próprio elemento. Escape com `preventDefault` mesmo com o popup fechado (`SUISelect.razor.js:69`, `SUIDateField.razor.js:117`). | `SUISelect` aberto dentro de um diálogo: Escape fecha o diálogo, o listbox nunca vê a tecla. Drawer compacto + diálogo: Escape fecha os dois. Campo fechado dentro de diálogo: Escape não fecha o diálogo. | `SUIOverlayManager` (seção 4, P4): pilha de camadas, um único listener de teclado, Escape roteado só para a camada do topo. |
| J4 | Médio | `SUILoadingButton.razor.js:26-27` adiciona `sui-btn--pending` (`pointer-events:none`, `sui-buttons.css:110`) em captura de clique; só é removida por timer de 8 s ou quando o Blazor reescreve `class`. `InstantFeedback` padrão `true`. | `<SUILoadingButton OnClick="TogglePanel">` sem tocar `IsLoading`: segundo clique ignorado por 8 s. | `InstantFeedback` padrão `false` a menos que `IsLoading` esteja ligado; JS limpa a classe por `MutationObserver` em `aria-busy` ou após animação curta fixa. |
| J5 | Médio | Ativação dupla quando o JS chega tarde ou não chega: gatilhos com `@onclick` **e** keydown Enter/Space → `Toggle` (`SUIDateField.razor:27-28`, `SUISelect.razor:31-32`); dia do calendário idem (`SUIDateField.razor:88-89`). `<button>` sintetiza `click` nativamente em Enter/Space; a supressão depende de `preventDefault` em JS que só liga após o primeiro `OnAfterRenderAsync` (e no Select só com itens). | Asset 404 ou CSP → Enter abre e fecha; teclado inutilizável; Enter em um dia dispara `ValueChanged` duas vezes. | Remover Enter/Space dos handlers .NET (deixar o click nativo), manter só ArrowDown/Escape; extrair um fragmento `SUIPopupTrigger` compartilhado. |
| J6 | Baixo | Sem fingerprint dos módulos JS (`?v=2` manual em três lugares, nenhum nos outros nove); `sufficit-ui.css` sem fingerprint nas amostras. | Cache do navegador serve JS antigo após atualização do pacote. | Usar o fingerprint de static web assets do .NET 9/10 (`@Assets[]`, `ImportMap`) e importar por caminho resolvido; remover `?v=`. |
| J7 | Baixo | Listeners permanentes em `document`/`window` sem contagem por página em `SUILoadingButton.razor.js:16-37`; observers de flyout do rail por render (`SUINavGroup.razor.cs:58-72`). | Custo cumulativo em SPA longa. | Registrar via `SUIOverlayManager`/`lib.module.js` com ciclo de vida de `enhancedload`. |

### 2.3 Correção de componentes

| # | Sev | Achado | Cenário | Remediação |
|---|---|---|---|---|
| C1 | **Alto** | Evento estático de processo `SUINavGroup.Rail.cs:42` `private static event Action<SUINavGroup>? RailFlyoutOpened`; `OpenFlyout` (`:54`) notifica **todas** as instâncias do processo; handlers chamam `InvokeAsync(StateHasChanged)` em componentes de outros usuários (`:85-95`). | Blazor Server: usuário A passa o mouse no rail → flyout de todo outro usuário fecha; N circuitos → N handlers por hover; referências presas pelo delegate estático até o descarte. | `SUIRailFlyoutCoordinator` Scoped registrado em `AddSufficitUI` ou `SUINavRailScope` cascateado pelo drawer. Teste com dois `TestContext` bUnit provando isolamento. |
| C2 | **Alto** | `SUITable.razor:9` passa `Indeterminate="true"` a `SUIProgressLinear`, que não tem esse parâmetro (`SUIProgressLinear.razor:10-31`); vira atributo HTML; `Value=0` → `scaleX(0)`; AT ouve `aria-valuenow="0"`. `AccessibilityContractTests.cs:284-287` só verifica que existe um progressbar. | Toda tabela com `Loading=true` mostra barra invisível e anuncia 0 %. | Adicionar `Indeterminate` a `SUIProgressLinear` (omitir `aria-valuenow`, animação de deslize); teste de que a barra não está em escala 0. Gate: `DynamicComponent`/parâmetros desconhecidos falharem em Debug. |
| C3 | Médio | `SUISelect<T>`: `Equals(item.Value, Value)` entre `object?` e `T` (`SUISelect.razor.cs:126`) nunca casa para `Guid`×string ou `long`×int → placeholder exibido com valor definido, `aria-selected` nunca `true`; Enter usa `_items[_activeIndex]` sem limite superior após `Unregister` (`:243-245,177-184`) → `IndexOutOfRange` mata o circuito; `Convert.ChangeType` pode lançar `OverflowException` (`:380`). | Lista de itens muda enquanto o menu está aberto; formulário com `Guid` e valores string. | `SUISelectItem<T>` e `ISUISelectRegistry<T>` genéricos com `EqualityComparer<T>`; clamp de `_activeIndex` em `Unregister`. |
| C4 | Médio | `<textarea …>@Value</textarea>` (`SUITextField.razor:26`) define `defaultValue`; após o usuário digitar, mudanças programáticas de `Value` não aparecem. | "Limpar formulário" deixa o texto antigo. | `value="@Value"` como atributo (o renderer define `.value`). |
| C5 | Médio | Fuso e cultura: `Today = DateOnly.FromDateTime(DateTime.Today)` no servidor (`SUIDateField.razor.cs:112`); formatação dirigida por `CurrentUICulture` (`:110`); strings via `IsPortuguese` (`:152-159`) e literais pt-BR em `SUIAutocomplete.razor.cs:91-103,175`, `SUINumericField.razor:150-153,205`, `SUITextField.razor:147,215`, `SUIPagination.razor:14-61` (com `N0` em `CurrentCulture`), `SUIDialogHost.razor:21`, `SUIConfirmDialog.razor:9-10`, `SUISnackbarHost.razor:11`, `SUICopyToClipboard.razor:100-111`, `SUITableEmpty.razor:33,36` (que ainda quebra descrições em `". "`, `:59-63`, partindo "Sr. Silva"). | Usuário pt-BR às 21h30 em host UTC: "Hoje" escolhe amanhã. App com UI `en` e cultura `pt-BR` mostra `mm/dd/yyyy`. | `TimeProvider` injetado (+ offset do navegador por circuito); `ISUILocalizer` com recurso pt-BR padrão; `Culture` (formato) separado de strings; remover a heurística de frase. |
| C6 | Médio | `SUITable<T>`: `Items?.ToArray()` por render (`SUITable.razor:4`) reenumera `IQueryable`; `@key` cai para a referência do item (`:36,47`) → instância repetida ou records com igualdade de valor lançam exceção de chave duplicada. | Lista com o mesmo objeto duas vezes derruba o render. | Materializar em `OnParametersSet`; `@key` por tupla `(RowKey, index)` quando `RowKey` é nulo; exigir `RowKey` para tipos de valor. |
| C7 | Médio | Diálogos: resultado não tipado `Task<object?>`; um diálogo por vez (o segundo fecha o primeiro com `null`, `SUIDialogHost.razor.cs:68-72`); `SUIDialogService.cs:61` faz cast duro `IDictionary → IReadOnlyDictionary` (`InvalidCastException` para implementações que não são `Dictionary`); chave de parâmetro errada em `DynamicComponent` lança durante o render. Snackbar sem fila (mensagens antes do host assinar são perdidas) e sem lock. | `ShowAsync<T>(…, {["Mesage"]=…})` derruba o circuito. Confirmação aninhada em outro diálogo cancela o pai. | `ShowAsync<TDialog,TResult>` → `SUIDialogReference<TResult>`; `Stack<SUIDialogRequest>` no host; `SUIDialogOptions`; validar chaves contra `[Parameter]` de `T` (reflexão cacheada) e faltar a `SUIDialogReference` em vez de renderizar; copiar parâmetros para novo `Dictionary`. Snackbar com fila limitada e replay como o de diálogo. |
| C8 | Médio | Tabs: `ActiveIndexChanged.InvokeAsync` fire-and-forget (`SUITabs.razor:108`, `SUISlidingTabs.razor:127`); painéis inativos desmontados (`SUITabs.razor:29-38`); setas param em aba desabilitada em SlidingTabs. | Exceção do pai não observada; estado de formulário perdido ao trocar de aba. | Opção `KeepPanelsMounted`; `await` do callback; pular desabilitadas; espelho local de estado como `SUINavGroup._expandedState`. |
| C9 | Médio | Botões (`SUIButton.razor:17,31,112-121`, `SUIIconButton.razor:20,34,101-110`, `SUILoadingButton.razor:36`) setam `disabled` enquanto o handler roda → o navegador solta o foco. | Usuário de teclado pressiona "Salvar": ao terminar, foco em `<body>`, leitor de tela mudo. | Estado ocupado com `aria-busy` + `aria-disabled` + CSS `pointer-events:none`, mantendo foco. |
| C10 | Médio | Churn de render na navegação: `RailFlyoutContext` novo por render (`Rail.cs:46-47`); `SUINavigationContext` substituído em todo `OnParametersSet` (`SUINavGroup.razor.cs:252-261`) → todos os descendentes re-renderizam a cada render do pai; uma ida ao JS por grupo raiz por render (`:71`). | Menu com 40 links re-renderiza inteiro ao abrir um grupo. | Cachear o record e substituí-lo só quando `Disabled`/`Expanded` mudam. |
| C11 | Baixo | Parâmetros mortos ou enganosos: `SUINavLink.TabIndex` (nunca renderizado), `SUINavGroup.Ripple`, `SUILayout.DrawerOpenChanged` (nunca invocado), `SUITable.SortBy`/`InitialDirection`, `SUIProgressCircular.CounterClockwise` (classe `--reverse` sem CSS), `SUICheckbox.ColorValue` (sem CSS), `SUIAutocomplete.ChildContent` (nunca renderizado), `SUIAvatar ColorValue` (emite `sui-color-*`, colore o texto em vez do fundo). `GetInitials` pula palavras ≤ 3 letras ("Bob Lee" → avatar vazio). `SUIIcon.Name == null` → NRE. | API promete o que não entrega. | Remover ou implementar; teste de paridade classe↔CSS (seção 4, P9). |
| C12 | Baixo | `SUISwitchButton` muta o próprio parâmetro `Checked` (`:39`); `SUIAutocomplete` com `T` struct: botão limpar sempre visível e "limpar" publica `default` (`SUIAutocomplete.razor.cs:150-151,215`); `Convert.ToString(double)` produz `"1E-07"` que `type=number` rejeita (`SUINumericField.razor:186`); `aria-describedby=""` renderizado quando vazio. | Ruído de estado. | Corrigidos pela classe base de inputs. |

### 2.4 Acessibilidade (WCAG 2.2 AA + WAI-ARIA APG)

| # | Sev | Achado | Cenário | Remediação |
|---|---|---|---|---|
| A1 | **Alto** | `<tr role="button">` em linhas clicáveis (`SUITable.razor:36-41`) remove semântica de linha/célula; toda célula vira conteúdo do botão; `aria-required-children` falha. | Leitor de tela perde navegação por tabela em qualquer tabela com `OnRowClick`. | Manter `tr` como linha; modo `ActivateOnRowClick` renderiza `<button>` visualmente oculto na primeira célula (ou coluna de link nomeada) e mantém o handler de ponteiro na linha; ou `role="row"` + `aria-selected` para grades de seleção. |
| A2 | **Alto** | Contraste (computado): claro `--sui-text-disabled #94a3b8` sobre surface **2,56:1** e sobre surface-2 **2,34:1** (é a cor de texto de inputs desabilitados, `sui-foundations.css:255`); borda de input/checkbox `--sui-border-strong #cbd5e1` sobre branco **1,48:1** (1.4.11 pede 3:1); `--sui-border` **1,23:1**. Escuro: `--sui-border #334155` **1,72:1**; fallback CSS escuro (sem provider) texto secundário **3,75:1** / **3,07:1** porque `SUITheme.Dark` (`SUITheme.cs:23-30`) e `sui-foundations.css:123-147` divergem. `SUIStatusBanner` hardcoded: sinal info `#0369a1` sobre `#202326` **2,66:1**, primário **3,06:1** (`sui-shared-status-banner.css:55,63`). Pares que passam: primário/contraste 5,17; texto primário 17,85; secundário 7,58; semânticos 5,9 a 7,1. | Usuário com baixa visão não distingue campo desabilitado nem borda de campo. | `SUIPaletteContrastTests` iterando os pares de papel para `SUIPalette.Default` e `SUITheme.Dark` via `SUIColorContrast`; `TextDisabled ≥ #64748b` (4,76) ou desabilitado por `opacity` sobre cor aprovada; `BorderStrong` para controle ≥ 3:1; sinais do banner derivados do tema. Gerar o bloco de tokens do CSS a partir do C# (P3). |
| A3 | Médio | Tooltip falha 1.4.13: sem Escape em `SUITooltip.razor.js`; `pointer-events:none` (`sui-portals.css:32`) + esconder em `pointerleave` → não "hoverable"; âncoras sem descendente focável recebem `aria-describedby` num `<span>` (`js:142-146`) e nunca abrem por foco. | Conteúdo do tooltip inacessível por teclado e impossível de ler com zoom. | Portal gerido pelo `SUIOverlayManager` com Escape; permitir hover no portal; exigir âncora focável ou `tabindex=0` automático. |
| A4 | Médio | Flyout do rail: `role="menu"`/`aria-haspopup="menu"` (`SUINavGroup.razor:11,21`) com filhos `<div><a>` e `<nav>` aninhados → `aria-required-children`; sem setas, sem Escape, sem `@onfocusout`. Cada grupo não-rail emite `<nav aria-label>` inclusive aninhados (`:57-61`) → dezenas de landmarks de navegação. | Teclado: flyout abre no foco e fica aberto após o foco sair. | Padrão disclosure (`aria-expanded` no gatilho, `role=group aria-labelledby` no painel); só o grupo raiz emite `<nav>`, aninhados `<div role="group">`. |
| A5 | Médio | Diálogo: foco inicial no botão X (`SUIDialogHost.razor.js:113-114`) e não na ação segura; sem `aria-describedby` para a mensagem; sem bloqueio de rolagem do `body`; fundo depende de `aria-modal`, não de `inert`; armadilha não redireciona Tab vindo de fora (`:26-30,93-99`). Drawer: seletor de focáveis omite `input/select/textarea` (`SUIDrawer.razor.js:16`). | Drawer móvel com input de filtro como último focável: Tab escapa do modal. | `SUIOverlayManager` com `focusables()` compartilhado (inputs, `[inert]`, `visibility:hidden`), `inert` nos irmãos, scroll lock, `SUIDialogOptions.InitialFocus`. |
| A6 | Médio | Formulários: `SUISwitch.razor:5` sem `role="switch"`; `SUISwitchButton.razor:4` sem `aria-pressed`; só `SUIDateField` tem `Required`/`aria-required`, nenhum marca visualmente (3.3.2); grade do calendário com `role="row"`/`columnheader` fora do `role="grid"` (`SUIDateField.razor:65-72`) e `<button role="gridcell">` (não permitido pelo ARIA in HTML); Tab sai do calendário sem fechar; combobox só-seleção sem type-ahead (APG); Autocomplete captura Home/End impedindo mover o cursor (`SUIAutocomplete.razor.cs:299-312`); `SUIChoiceCard` sem `radiogroup`/fieldset e com N erros `role=alert` idênticos por grupo (`SUIChoiceCard.razor:48-51`); `role="alert"` no erro só em Checkbox/Switch/ChoiceCard; TextField/Numeric reservam espaço de erro só com `EditContext` (linhas mudam de altura em `SUIFormGrid`). | Cada item é uma falha de conformidade por componente. | Classe base `SUIInputBase<T>` com `Required`, marcador, `aria-required`, erro uniforme; `SUIListboxNavigator` com type-ahead; `SUIChoiceGroup<T>`; `<table role=grid>` com `<th>` e `<td role=gridcell tabindex>` roving. |
| A7 | Médio | Tabela: sem `Caption`/`AriaLabel`; `aria-busy` no `div` e não na `<table>`; `aria-sort` impossível; ícone de ordenação revelado só em `:hover` (`sui-sort-label.css:31,34`), nunca em `:focus-visible`. Paginação com strings pt-BR fixas. | Cabeçalhos ordenáveis não anunciam direção. | `SUITh.SortDirection` emitindo `aria-sort`; `SUITableSortContext` cascateado; `Caption` renderiza `<caption class="sui-sr-only">`. |
| A8 | Médio | Live regions: `SUIAlert` sempre `role="alert"` (assertivo) mesmo para Info persistente; `SUIStatusBanner` põe `role="status"`+`aria-live` numa região inteira com `<h2>` fixo (`:6-8,18`); snackbar host `aria-atomic="true"` reanuncia todas as entradas a cada mudança e `danger` usa `status` polido; sem pausa em hover/foco. | Leitor de tela interrompido por conteúdo estático; erro crítico anunciado como polido. | Papel derivado do tom com override; `HeadingLevel`; live region limitada à descrição; `aria-atomic` por entrada; `role=alert` para danger/warning; `PauseOnHover`. |
| A9 | Médio | Nomes acessíveis: `SUIIconButton` com `Icon` e sem `Title` não tem nome (`SUIIconButton.razor:26,34`, svg `aria-hidden`); `SUICopyToClipboard` em modo ChildContent é `<span>` só-clique com `stopPropagation` (`:18-22`); `SUIStatusBadge` só-ícone sem nome; `SUIAvatar` com `Alt=null` omite `alt`; `SUIProgress*` com `AriaLabel=null` sem nome; `SUISkeletonLoader` sem `aria-hidden`/`aria-busy`. | Botões de ícone anunciados como "botão". | `Title` `[EditorRequired]` + teste; `SUICopyToClipboard` envolve em `<button type=button>`; `alt=""` decorativo; skeleton `aria-hidden`. |
| A10 | Baixo | Movimento reduzido: `sui-shared-dialog-host.css:63-67` zera a animação do spinner indeterminado (arco estático indistinguível de "pronto"); blocos `prefers-reduced-motion` duplicados em 12 arquivos. | Usuário com movimento reduzido não sabe que está carregando. | Desacelerar (2,4 s como já faz `sui-buttons.css:124-126`) em vez de remover; centralizar em um custom-media. |
| A11 | Baixo | Vazamento global de CSS: `:root { overflow-wrap: anywhere }` (`sui-foundations.css:173`) reescreve a quebra de linha do documento inteiro; `:root { color-scheme: light }` (`:120`) força controles claros em host escuro sem SUI; `.theme-dark`/`:root.theme-dark` sem prefixo; `.sui-root *` box-sizing vira reset global porque o provider envolve o app. Um `!important` (`nav-2.css:116`) mas `StyleContractTests.cs:123` permite um por arquivo (37 arquivos). | Host que mistura SUI com outra biblioteca vê quebras de linha e scrollbars mudarem. | Escopar `overflow-wrap` e `color-scheme` a `.sui-root`; `.theme-dark` para um `sui-compat-theme-dark.css` opcional; orçamento global de `!important` = 0 com allowlist. |

### 2.5 Cadeia de suprimentos e entrega

| # | Sev | Achado | Cenário | Remediação |
|---|---|---|---|---|
| S1 | **Alto** | Release = push de tag sem proteção (`build.yml:448`); ambiente `production` sem regras, `can_admins_bypass: true`; `main` sem revisão obrigatória, `enforce_admins: false`; zero rulesets; tags livres. | Credencial de mantenedor comprometida ou `git push --tags` por engano publica versão GA irrevogável no NuGet.org em ~5 minutos, sem PR nem revisão; os dez consumidores em `2.*` a recebem no próximo restore. | **[op]** Ruleset para criação de tags `v*` restrito a papel de release; revisores obrigatórios no ambiente `production`; revisão obrigatória em `main` com `enforce_admins`. Lever de design: publicar somente a partir de `release` criado por workflow revisado. |
| S2 | **Alto** | `permissions: contents: write` no nível do workflow (`build.yml:20-23`) alcança css, build, pack, component-tests, browser-tests e lighthouse, jobs que rodam `npm ci`, `dotnet restore`, Playwright `install --with-deps` e `npx --yes lighthouse@12` (`:426`, não fixado, sem lockfile). | Comprometimento de qualquer dependência transitiva desses jobs executa com token de escrita no repositório e pode empurrar commit em `main` (que já recebe pushes do bot). | **[op]** `contents: read` no topo; job de refresh de baseline em workflow próprio com `contents: write`; `lighthouse` como devDependency fixada no `package-lock.json`. |
| S3 | Médio | Refresh de baselines visuais commitado direto em `main` pelo bot (`build.yml:316-326`; 5 commits). | Regressão capturada vira verdade do gate sem revisão. | Abrir PR em vez de `git push`. |
| S4 | Médio | `NUGET_API_KEY` de vida longa mantido para `maintenance.yml`, que nunca rodou; publicação já usa OIDC. Política do NuGet.org expira chaves antigas em 2026-11-01. | Vazamento da chave permite push/unlist de qualquer versão. | **[op]** Apagar o segredo; deslistar uma vez pela UI e remover o workflow. |
| S5 | Médio | `validate-package.sh:77` e `Showcase.csproj:19-20` hardcodam `10.0.12`; Dependabot só vigia `/src`; `tests/` e `samples/` desatualizados (bUnit 2.9.0 → 2.11.3, NUnit 4.3.2 → 4.6.1, Test.Sdk 17.14 → 18.10, coverlet 6 → 10). | Próximo bump para 10.0.13 → NU1605 no consumidor sintético → `pack` vermelho até edição manual. | Script lê a versão do nuspec; Dependabot cobre `samples/` e `tests/`. |
| S6 | Médio | Baseline de API só detecta remoções; sem `PackageValidationBaselineVersion`; analyzers/warnaserror só no CI; sem canal pré-release; `AssemblyVersion` muda a cada release. | Membro público novo ou quebra binária passa sem revisão; consumidor compilado contra SUI mais novo que o implantado falha com `FileLoadException`. | Falhar em `atual − baseline`; remover interfaces herdadas do arquivo; `PackageValidationBaselineVersion` = última tag (CI passa por `git describe`); `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild` em `Directory.Build.props`; permitir `2.yy.MMdd.HHmm-<label>.N` atrás de ambiente `prerelease`; `AssemblyVersion` fixa `2.0.0.0` com calendário em `FileVersion`/pacote. |
| S7 | Baixo | Pacote embarca CSS três vezes (55 + 90 + 37 KB); `validate-package.sh` checa 6 de 12 módulos JS; checks de catálogo só em `pages.yml`; CodeQL só C# (13 arquivos JS enviados não são analisados); `Packing` configuration morta; colisão de tags no mesmo minuto → 409. | Bloat e cegueira parcial do gate. | Parar de copiar fontes para `wwwroot/styles`; derivar a lista de módulos de `src/**/*.razor.js`; mover checks de catálogo para `build.yml`; `javascript-typescript` no CodeQL. |

Resultado dos scanners: `npm audit` 0 vulnerabilidades; `lightningcss` 1.33.0 é a última; `dotnet list package --vulnerable --include-transitive` limpo em `src`, testes e amostras; `Microsoft.AspNetCore.Components` 10.0.12 é a última. Pacote publicado carrega assinatura de repositório do NuGet.org, `.snupkg` com pdb portátil, metadados SourceLink (`commit`, `branch`), README e ícone.

### 2.6 Defaults frágeis que o consumidor recebe "de graça"

- Vazamento CSS global em `:root` (A11) e `color-scheme` forçado.
- Estado JS global de processo no rail (C1).
- `InstantFeedback=true` por padrão (J4).
- Strings pt-BR não substituíveis (C5).
- Listbox/tooltip/dialog/snackbar sem coordenação de camadas (J3).
- Nenhuma chamada de rede, telemetria ou analytics: verificado por grep em `src/` e `samples/`.

---

## 3. Comparação de mercado (estado em 2026-09-19)

Fontes primárias consultadas hoje: NuGet.org, repositórios GitHub, Microsoft Learn, páginas de preço e acessibilidade dos fornecedores, W3C DTCG, npm. **[V]** verificado, **[U]** incerto.

### 3.1 Tabela de fatos

| Biblioteca | Versão estável (data) | .NET | Licença / preço | Componentes | Stars / downloads |
|---|---|---|---|---|---|
| MudBlazor | 9.10.0 (2026-09-13) [V] | 8/9/10 | MIT | ~83 famílias | 10,6 k / 37,3 M |
| Radzen Blazor | 11.4.1 (2026-09-17) [V] | 8/9/10 | MIT (temas premium pagos) | 145+ | 4,35 k / 21,8 M |
| Microsoft Fluent UI Blazor | 4.14.4 (2026-07-30); v5 ainda RC5 (2026-08-09) [V] | 8/9/10 | MIT | 56 | 4,8 k / 3,7 M |
| Blazorise | 2.3.2 (2026-09-08) [V] | 8/9/10 | Dual: Community restrito, Pro €590/dev/ano, Enterprise €990 | 90+ | 3,5 k / 7,1 M |
| Ant Design Blazor | 1.6.2 (2026-06-16) [V] | 5–10 | MIT | 75 | 6,2 k / 2,8 M |
| Havit.Blazor | 4.27.0 (2026-09-15) [V] | 8/9/10 | MIT | 70+ | 604 / 749 k |
| BootstrapBlazor | 10.10.2 (2026-09-11) [V] | 6–10 | Apache-2.0 | 135+ | 4,9 k / 5,8 M |
| Blazor Bootstrap (vikramlearning) | 4.0.0 (2026-08-30) [V] | 8/9/10 | Apache-2.0 | — | 1,2 k / 2,6 M |
| Telerik UI for Blazor | 15.0.1 (2026-08-27) [V] | — | US$ 749 a 1.249/dev/ano | 120+ | fechado |
| Syncfusion Blazor | 2026 Vol 2 (Grid 34.2.8, 2026-09-14) [V] | 8/9/10 | US$ 1.199/dev/ano (mín. 5); Community License com limites | 145+ | fechado |
| DevExpress Blazor | 26.1.5 (2026-09-18); 26.2 em dez/2026 exige .NET 10 [V] | 8 | assinatura ≈ US$ 1.078/ano [U] | — | fechado |
| Headless Blazor: Blazor Blueprint, NeoUI, Blazix.BaseUI, Blaizio, Navius | 0.x, ativos em 2026 [V] | 8/10 | MIT/Apache | 16 a 28 primitivos + camada estilizada | 0 a 600 |
| **SUI (este projeto)** | 2.26.918.1935 (2026-09-18) | 10 | MIT | 69 | privado / 27 versões em 6 semanas |

### 3.2 Eixos

1. **Arquitetura de componentes.** Todas as bibliotecas estabelecidas têm classe base própria (`MudComponentBase`, `RadzenComponent`, `FluentComponentBase`, `BaseComponent`), genéricos onde há dados, templates e providers em cascata. A novidade de 2026 é a camada headless em Blazor (Blueprint, NeoUI, Blazix.BaseUI, Blaizio, Navius), espelhando Radix/Base UI/Headless UI: contrato ARIA + teclado + foco + `data-state` sem CSS, com pele por cima. Fluent v5 traz "Default Values" globais por tipo; Blazorise 2.0 unificou `Value/ValueChanged/ValueExpression` e publicou analyzer de migração. **SUI**: sem classe base, o que o coloca abaixo de todos os estabelecidos nesse eixo; a separação comportamento/pele não existe.
2. **Tema e tokens.** Dois campos: objeto C# emitindo custom properties (MudBlazor, Blazorise, Radzen) e pele só-CSS (Havit, BootstrapBlazor, shadcn-style). DTCG publicou a primeira versão estável 2025.10 em 2025-10-28 (Format, Color, Resolver), adotada por Figma, Penpot, Style Dictionary, Terrazzo. Tailwind 4 usa `@theme` CSS-first. Material Web está em manutenção desde 2024-06. Nenhuma biblioteca Blazor exporta DTCG hoje. **SUI**: modelo C# → custom properties é o padrão do mercado, mas as strings cruas e a duplicação C#/CSS (dark divergente) ficam abaixo de MudBlazor/Blazorise, que tipam cores e geram o CSS a partir do modelo.
3. **Entrega e isolamento de CSS.** Todos enviam um bundle global; ninguém usa isolamento CSS em escala de biblioteca. Tamanhos medidos sem compressão: MudBlazor 625 KB, Radzen 761 KB, BootstrapBlazor 569 KB, AntDesign 690 KB, Havit 244 KB, Telerik 747 KB a 1,15 MB, Syncfusion 537 a 552 KB, Radix Themes 813 KB. **SUI: 55 KB minificado (+ 37 KB escopado), 10 KB gzip.** É a melhor posição do mercado nesse eixo, mas a dupla estratégia de escopo e as três cópias no pacote são dívida.
4. **Modelo de interop JS.** Script único (MudBlazor 70 KB, Radzen 288 KB), runtime de web components (Fluent) ou módulos ES por componente (Havit, Blazix.BaseUI). A orientação oficial para bibliotecas é `{PackageId}.lib.module.js` com `afterWebStarted` e, para SSR estático, o padrão `PageScript` ouvindo `enhancedload`. **SUI** usa módulos ES por componente (moderno) mas sem initializer de biblioteca e sem tratamento de navegação aprimorada.
5. **Matriz de modos de render.** O framework tem SSR estático, InteractiveServer, WebAssembly, Auto, streaming; `RenderFragment` não cruza a fronteira estático→interativo. MudBlazor: "static rendering is not supported"; Radzen e Telerik exigem interatividade; Havit tem "some support for static SSR"; Fluent v5 melhorou SSR estático nas RCs. **SUI**: sem qualquer consciência de modo; Select/Autocomplete não renderizam controle nativo; só Server e WASM são exercitados. Equivalente a MudBlazor/Radzen, abaixo de Havit/Fluent.
6. **Postura de acessibilidade.** Comerciais lideram: Telerik com tabela WCAG 2.2 por componente e VPAT; Syncfusion com WCAG 2.2 + 508 + testes manuais com leitor de tela; DevExpress 26.1 com 45+ melhorias; Radzen com paleta AA, alto contraste AAA (pago) e VPAT 2.5. MudBlazor sem declaração e com issues abertas de foco/contraste. Blaizio roda axe por família em claro e escuro no CI. **SUI**: intenção forte (aria por campo, sr-only, axe no navegador, Lighthouse a11y = 1,0, alvos de 44 px, `focus-visible`), mas os achados A1 a A11 mostram que a conformidade real está no nível de MudBlazor, abaixo de Radzen e dos comerciais.
7. **Estabilidade de API e versionamento.** SemVer com major seguindo o .NET (MudBlazor 9, Radzen 11, Blazorise 2 com CLI de migração, BootstrapBlazor 10 no dia seguinte ao GA). Nenhuma usa `PublicApiAnalyzers`. **SUI**: calendário `2.yy.MMdd.HHmm` é compatível com `2.*`, mas esconde quebras; o baseline que só detecta remoções e a ausência de pré-release ficam abaixo de Blazorise e Fluent.
8. **Tamanho e orçamentos.** Ninguém publica orçamentos de CSS/JS, Lighthouse ou gates de tamanho. **SUI está à frente do mercado**: orçamentos de bytes gzip/brotli, orçamento de linhas por arquivo, Lighthouse com limiares, `performance` API em testes.
9. **Testes.** bUnit é universal (2.11.3 em 2026-09-13); Fluent tem Verify + Playwright; Blazorise tem Playwright.NUnit; MudBlazor renderiza exemplos de docs como testes; baseline visual e axe em CI são raros (Blaizio). **SUI**: 698 testes bUnit + 110 Playwright com axe + baselines visuais + orçamentos é a segunda melhor infraestrutura do grupo, atrás só de Fluent em profundidade; a lacuna é o que os testes não cobrem (overlays, SSR estático, JS ausente, caminhos negativos).
10. **Cadência e cadeia de suprimentos.** Radzen várias vezes por semana; MudBlazor mensal; Telerik trimestral. Trusted Publishing do NuGet.org desde set/2025; chaves antigas expiram em 2026-11-01; adoção OSS ainda rasa (BootstrapBlazor usa OIDC; MudBlazor, Radzen e Ant Design ainda usam segredos longos). **SUI já usa OIDC e fixa ações por SHA**, à frente da maioria; a governança de tag/ambiente (S1, S2) é o que falta.

### 3.3 Baseline "moderno" de 2026 para uma biblioteca Blazor, e onde SUI está

| Item | Estado do SUI |
|---|---|
| Multi-target net8/9/10 | só net10 (aceitável para consumidor interno, limita adoção) |
| Matriz de render explícita, SSR estático seguro | ausente |
| JS ciente de navegação aprimorada (`lib.module.js`, `enhancedload`) | ausente |
| Tokens como custom properties com camada semântica; export DTCG | parcial (tokens sim, sem camada semântica separada, sem DTCG) |
| Troca de tema em runtime sem FOUC | sim (inline `<style>` do provider) |
| Bundle único fingerprintado com `@layer` e prefixo | prefixo sim; sem fingerprint; sem `@layer`; três cópias |
| Orçamentos de tamanho no CI | **sim, acima do mercado** |
| WCAG 2.2 AA por componente + axe no CI + declaração | axe sim; conformidade não; sem declaração |
| Camada headless separável | ausente |
| Baseline de API pública com detecção de adições + política de quebra | parcial |
| bUnit + Playwright em Server/WASM/SSR + baselines visuais | Server e WASM sim; SSR não |
| Localização e RTL | ausente (pt-BR fixo) |
| Padrões de dados compatíveis com QuickGrid | ausente (`SUITable` é básico) |
| Trusted Publishing OIDC, assinatura, SBOM | OIDC sim; SBOM não |
| Sistema de defaults globais e CLI/analyzer de migração | ausente |

### 3.4 Ranking

Por prontidão como fundação única de UI de produção (arquitetura, a11y, breadth, estabilidade), em 2026-09:

1. Telerik UI for Blazor
2. Syncfusion Blazor
3. DevExpress Blazor
4. MudBlazor
5. Radzen Blazor
6. Blazorise
7. Microsoft Fluent UI Blazor (v4 estável; v5 quando sair de RC sobe para 4.º)
8. BootstrapBlazor
9. Havit.Blazor
10. Ant Design Blazor
11. **SUI**
12. Blazor Blueprint / NeoUI / Blaizio (headless, jovens)

SUI vence todos em tamanho de bundle e orçamentos, empata com a média em infraestrutura de testes e pipeline OIDC, e perde em classe base, conformidade de acessibilidade, SSR estático, breadth (sem grid real, sem menu/popover genérico, sem time picker, sem upload, sem árvore, sem localização), estabilidade de API e governança de release.

---

## 4. Pontuação e melhorias de arquitetura

### 4.1 Notas (0 a 10)

| Dimensão | Nota | Justificativa objetiva |
|---|---|---|
| Segurança | **5,0** | 1 Crítico (I1), 3 Altos (I2, I3, S1) e 1 Alto de pipeline (S2) contra: zero dependências vulneráveis, zero globais JS, zero `innerHTML`, OIDC, ações fixadas por SHA, smoke test de pacote real, sem telemetria. Os sinks exigem que o consumidor roteie dados não confiáveis, mas um design system deve ser seguro por construção. |
| Arquitetura | **5,5** | Taxonomia limpa, módulos ES por componente, tokens em custom properties, sem dependência de terceiros, gates de contrato e orçamento. Contra: nenhuma classe base (6 cópias de cada preocupação), evento estático de processo, três gerenciadores de foco/Escape concorrentes, ícones e variantes stringly-typed, dois escopos de CSS e três cópias no pacote, tema duplicado em C# e CSS, nenhuma consciência de render mode, nenhuma abstração de localização, diálogo não tipado. |
| Qualidade de código | **6,0** | Estilo consistente, nullable, XML docs completos, 698 testes rápidos, testes de contrato lendo o repositório. Contra: duplicação sistemática, 10 parâmetros mortos, 10 divergências comentário↔código, corridas de descarte fire-and-forget, analyzers e warnaserror ausentes do csproj. |
| Acessibilidade | **5,0** | Intenção e ferramental acima da média OSS (aria por campo, axe, Lighthouse 1,0, alvos 44 px, movimento reduzido). Contra: 2 Altos (`tr role=button`, contraste de texto desabilitado 2,56:1 e bordas 1,48:1), 8 Médios normativos (tooltip 1.4.13, Escape, menu do rail, diálogo, switch sem role, grade do calendário, `aria-sort`, live regions). |
| Completude de funcionalidades | **5,0** | 69 componentes cobrem o núcleo de formulários, navegação, overlays e feedback. Faltam para ser fundação única: grid com ordenação/paginação/virtualização integradas, menu/dropdown/popover genéricos, time/date-time picker, upload, tree view, breadcrumb, stepper de formulário, localização, RTL, SSR estático. |
| Experiência do consumidor | **6,0** | Um `AddSufficitUI`, um `<link>`, XML docs, símbolos, SourceLink, catálogo gerado e Showcase publicado. Contra: `2.*` recebe toda tag no ato sem pré-release, `AssemblyVersion` instável, três nomes para o dicionário de atributos, `Class` vs `CssClass`, strings pt-BR fixas, CSP exige `unsafe-inline` apesar do `Nonce`, JS sem fingerprint. |
| Prontidão para produção | **5,5** | Build e testes verdes, CI com matriz de 3 navegadores, pacote validado por smoke test, já consumido por 13 repositórios. Contra: seis semanas de idade, 27 releases GA em seis semanas, release sem proteção, exceções que derrubam o circuito (J1, C3, C7), estado estático entre circuitos (C1), baseline visual que só passa no runner exato. |
| **Geral** | **5,4** | Média simples. Biblioteca promissora, bem instrumentada, com dívida estrutural concentrada em um único problema raiz: ausência de camadas compartilhadas (base de componente, base de input, gerenciador de overlay, escritor de tema). |

### 4.2 Melhorias de arquitetura, por impacto

**P1. `SUIComponentBase` e `SUIInputBase<T>` (+ `SUIPopupInputBase<T>`).**
Introduzir `src/Components/SUIComponentBase.cs` com `Class`, `Style`, `AdditionalAttributes` único, política `SUIAttributes.Merge` (splat primeiro, `class`/`style` concatenados, `id` honrado, recusa de `role`/`type`/`href`/`on*`), `SUIClassBuilder` e `Slug` compartilhados, `SUIJsModule` opcional. `SUIInputBase<T>` acrescenta `Id`/`EffectiveId` único, `Label`, `HelperText`, `ErrorText`, `Invalid`, `Required` (marcador + `aria-required`), `Name` com `NameAttributeValue` derivado de `ValueExpression`, `AriaDescribedBy` composto, `Disabled`, ciclo de vida de `SUIFieldBinding`, fragmentos de erro/ajuda uniformes com `role=alert`, `ISUILocalizer`. `SUIPopupInputBase<T>` adiciona `_open`, controlador de focus-out, transições do popover e `SUIListboxNavigator` (índices, type-ahead, pular desabilitados). Migrar os 9 inputs. Resolve I3, J2, J5, A6, C4, C12, L1-L10 da seção de formulários. Trade-off: toca todos os arquivos de `src/Components/Forms`; fazer em um PR por componente com o baseline de API atualizado.

**P2. Valor de ícone tipado `SUIIconRef`.**
`src/Components/DataDisplay/SUIIconRef.cs`: `readonly record struct` com fábricas `Named(string)` e `Path(string d, SUIIconStroke)`; `SUIIcons` vira `SUIIconRef`; todos os parâmetros `Icon`, `StartIcon`, `EndIcon`, `ExpandIcon`, `LoadingIcon`, `AdornmentIcon`, `IconPath` mudam de tipo; `SUIIcon` renderiza por `AddAttribute`. Unificar os três dialetos de ícone (switch de nomes, constantes `SUIIcons`, mini-set do `SUIEmptyState`) em um registro único. Resolve I2 e o "círculo silencioso" de nome desconhecido. Trade-off: quebra de API; um release com `[Obsolete]` e sanitizador.

**P3. `SUIThemeCssWriter` com tipos CSS e geração do fallback a partir do C#.**
`src/Themes/Css/` com `CssColor`, `CssLength`, `CssFontStack`, `CssShadow`, `CssTiming`; `SUIPalette`/`SUITypography`/`SUILayout` tipados; `SUIThemeCssWriter.Write(ISUITheme)` único e testado; ferramenta `tools/TokenGen` que gera o bloco de tokens de `sui-foundations.css` a partir de `SUIPalette.Default` e `SUITheme.Dark`, verificado pelo mesmo `--check`; teste que exige que todo `--sui-*` do CSS tenha propriedade no modelo ou esteja em allowlist; `SUIPaletteContrastTests`. Publicar tokens só no wrapper e dar aos portais `data-sui-theme` copiado por JS (resolve o conflito `.theme-dark` no `body`). Resolve I1, I5, A2, D6-D9, D12 do tema. Trade-off: consumidores que passam CSS exótico precisam de `CssValue.Unsafe`.

**P4. `SUIOverlayManager` + `sui-overlay.js`.**
Serviço Scoped registrado em `AddSufficitUI` e módulo único com pilha de camadas: um listener de teclado no `document`, Escape roteado ao topo, `inert` nos irmãos, `focusables()` compartilhado (inputs, `[inert]`, `visibility:hidden`), scroll lock, restauração de foco, z-order por `--sui-z-*`. DialogHost, Drawer compacto, flyout do rail, tooltip, Select/Autocomplete/DateField registram e desregistram. Resolve J3, J7, A3, A5 e parte de A6. Trade-off: um módulo maior compartilhado e migração dos três popovers de formulário.

**P5. Coordenador de rail Scoped e ARIA de navegação.**
Substituir o evento estático por `SUIRailFlyoutCoordinator` Scoped (ou `SUINavRailScope` cascateado); flyout como disclosure (`aria-expanded` + `role=group aria-labelledby`), `@onfocusout` e Escape; só o grupo raiz emite `<nav>`; cachear `SUINavigationContext`. Resolve C1, C10, A4.

**P6. Estratégia de modo de render.**
Base de input expõe `NameAttributeValue`; `SUISelect` renderiza `<select name>` nativo e `SUIAutocomplete` `<input name>` quando `RendererInfo.IsInteractive == false`; `Sufficit.Blazor.UI.lib.module.js` com `afterWebStarted` e `enhancedload` para os listeners globais; testes com `HtmlRenderer` para SSR estático e um sample em InteractiveAuto. Resolve H3 de formulários e o item 2 e 3 do baseline 2026. Trade-off: dois caminhos de render por componente de popup.

**P7. API tipada de diálogo e paridade do snackbar.**
`ShowAsync<TDialog,TResult>` → `SUIDialogReference<TResult>`, pilha no host, `SUIDialogOptions` (backdrop, Escape, foco inicial, rótulos), validação de chaves de parâmetro, cópia defensiva do dicionário; `SUISnackbarService` com fila limitada e replay, `SUISeverity` enum, `aria-atomic` por entrada, `PauseOnHover`. Resolve C7, A8, I7.

**P8. Governança de release e gates de API.**
Ruleset de tags + revisores no ambiente `production`; `contents: read` no topo do workflow; baseline visual por PR; apagar `NUGET_API_KEY`; baseline de API falhando em adições e sem interfaces herdadas; `PackageValidationBaselineVersion` = última tag; `TreatWarningsAsErrors` + `AnalysisLevel=latest-recommended` + `EnforceCodeStyleInBuild` + `.editorconfig` em `Directory.Build.props`; canal pré-release `2.yy.MMdd.HHmm-<label>.N`; `AssemblyVersion` fixa `2.0.0.0`; Dependabot em `samples/` e `tests/`; `javascript-typescript` no CodeQL; checks de catálogo em `build.yml`. Resolve S1 a S7 e S6. Majoritariamente operacional; o lever de design é o gate de adições e a validação de pacote contra a versão anterior.

**P9. Consolidação de CSS.**
Uma estratégia de escopo por decisão explícita (recomendação: global prefixado para tudo que é composto por filhos, isolamento só para folhas sem filhos SUI); parar de copiar `wwwroot/styles/*` no pacote; `ClassCssParityTests` (seletores `.sui-*` do bundle+escopado × literais de classe em `.razor`/`.cs`, falhando nas duas direções); orçamentos em um único `eng/budgets.json`; `!important` global = 0; custom-media/mixins do lightningcss para os quatro padrões repetidos (sr-only ×4, alvo 44 px ×12, anel de foco ×9, movimento reduzido ×12); escopar `overflow-wrap` e `color-scheme` a `.sui-root`; `@layer sui.base, sui.components, sui.overrides`. Resolve A11, D10-D12 do CSS, S7.

**P10. `ISUILocalizer` + `TimeProvider`.**
Serviço com recurso pt-BR padrão e override por `IStringLocalizer`; todas as strings fixas e mensagens de parse passam por ele; `TimeProvider` injetado no DateField com offset do navegador por circuito. Resolve C5.

**P11. Tabela e progresso.**
`SUITh.SortDirection` → `aria-sort`; `SUITableSortContext`; `Caption`; `aria-busy` na `<table>`; `ActivateOnRowClick` sem `role=button`; `Indeterminate` em `SUIProgressLinear`; `@key` por tupla. Resolve A1, A7, C2, C6.

**P12. Estado ocupado de botões sem `disabled`.**
`aria-busy` + `aria-disabled` + `pointer-events:none`, foco preservado; `InstantFeedback` padrão `false` sem `IsLoading` ligado. Resolve C9, J4.

---

## 5. Veredito

**Pontos fortes.** Bundle de 55 KB contra 500 KB a 1,1 MB dos concorrentes; zero dependências de UI de terceiros; módulos ES por componente sem globais nem `innerHTML`; tokens em custom properties com troca de tema em runtime; infraestrutura de qualidade rara em OSS (698 testes bUnit, 110 testes Playwright com axe em três navegadores, baselines visuais, orçamentos de bytes e Lighthouse, smoke test do pacote em um consumidor real, catálogo gerado e compilado); Trusted Publishing OIDC e ações fixadas por SHA; XML docs, símbolos e SourceLink.

**Riscos que bloqueiam a adoção como fundação única de produção hoje.**
1. Segurança por construção ausente em dois sinks sistêmicos: valores de tema em `<style>` (I1) e ícones como markup em ~15 pontos de entrada (I2). Basta um consumidor rotear um campo de configuração de tenant para lá.
2. Estado estático de processo no rail (C1) e exceções não tratadas no import de JS e no Select (J1, C3) derrubam circuitos em Blazor Server, o modo que a maioria dos consumidores usa.
3. Acessibilidade abaixo do que o ferramental sugere: `tr role=button`, contraste de desabilitado e bordas, Escape engolido, tooltip sem teclado, menu do rail com papéis errados.
4. Release GA irrevogável por push de tag sem revisão, com dez consumidores em `2.*` recebendo no ato e sem canal pré-release.
5. Nenhum caminho de SSR estático e nenhuma localização: a biblioteca só funciona interativa e só fala pt-BR.

**Recomendação.** Uma aplicação Sufficit **nova** não deveria padronizar no SUI hoje como fundação única se precisar de grid de dados, SSR estático, múltiplos idiomas ou conformidade de acessibilidade auditável; nesses casos MudBlazor (OSS) ou Telerik/Syncfusion (comercial) são mais seguros. Para aplicações internas interativas em Blazor Server ou WASM, em pt-BR, com formulários e navegação como carga principal, o SUI é adotável **desde que** o consumidor trate `Icon*`, tema e `Href` como dados confiáveis e fixe a versão em vez de usar `2.*`.

**Roteiro para virar fundação de produção**, na ordem em que cada passo destrava o seguinte:
1. **P8 primeiro, em um dia**: proteger tags e ambiente, `contents: read`, apagar a chave longa, baseline de API com adições, analyzers no csproj, canal pré-release. Sem isso, todo passo seguinte chega aos consumidores sem revisão.
2. **P2 e P3**: tipar ícones e tema. Fecham o Crítico e um dos Altos de injeção e tornam o CSS de fallback derivado do C#, eliminando a divergência claro/escuro e dando testes de contraste.
3. **P1**: classe base de componente e de input. É a mudança que converte seis cópias de cada preocupação em uma, corrige a política de atributos (I3), dá `Required`, `Name` e localização a todos os campos, e é pré-requisito de P6.
4. **P4 e P5**: gerenciador de overlay e coordenador de rail. Fecham Escape, foco, `inert`, tooltip e o evento estático.
5. **P6, P7, P11, P12, P9, P10** em seguida, cada um independente.

Com P8, P2, P3, P1, P4 e P5 entregues e o baseline de API refeito sobre a nova camada base, a nota de arquitetura sobe para a faixa de 7,5 a 8 e a de segurança para 8; nesse ponto, o SUI passa a ser recomendável como fundação única para aplicações Sufficit interativas, mantendo a vantagem de tamanho e de orçamento que nenhum concorrente tem.

---

## Apêndice A. Comandos executados e resultados

```
dotnet build Sufficit.Blazor.UI.slnx -c Release --no-incremental -warnaserror   # 0 avisos, 0 erros
dotnet test tests/Sufficit.Blazor.UI.Tests -c Release                            # 698 aprovados
dotnet test tests/Sufficit.Blazor.UI.BrowserTests -c Release (chromium local)    # 57 aprovados, 52 pulados, 1 falha (baseline visual 1440x5380 vs 1440x5398)
node scripts/build-css.mjs --check                                                # ok; raw 55437, gzip 10212, brotli 8937
npm audit / npm outdated                                                          # 0 vulnerabilidades, nada desatualizado
dotnet list package --vulnerable --include-transitive (src, tests, samples)      # nada
curl api.nuget.org flatcontainer + registration (sufficit.blazor.ui)              # 27 versões, todas listadas
unzip -l sufficit.blazor.ui.2.26.918.1935.nupkg                                   # 63 entradas, 260,9 KB
gh api environments/production, branches/main/protection, rulesets, run list     # sem proteção; maintenance nunca rodou
```

## Apêndice B. O que não foi verificado

- Comportamento em Blazor Hybrid/WebView (consumido por `sufficit-ai-genius`), sem ambiente de execução aqui.
- Testes de navegador em firefox/webkit (só chromium instalado localmente; o CI cobre).
- Estado do SSR estático e preços exatos da DevExpress (documentação bloqueia fetch).
- Números de contraste foram computados a partir dos hexadecimais de `SUIPalette.cs`, `SUITheme.cs` e do CSS; não foram medidos em renderização real com anti-aliasing.
