---
name: sui-design
description: Projete, construa, critique e refine interfaces frontend em projetos Blazor da Sufficit que usam a biblioteca Sufficit.Blazor.UI (SUI). Use quando o usuário quiser criar ou refinar uma página, componente, formulário, layout, tema ou token de design em Blazor; detectar campos desalinhados em linhas horizontais; aplicar a marca Sufficit (âmbar #ee6321, modelo hex + color-mix, NÃO OKLCH); seguir as convenções SUI (prefixo SUI, tokens --sui-*, SUIThemeProvider, enums próprios); ou auditar acessibilidade, contraste, responsividade e lifecycle do Blazor. Consolida o sistema de marca Sufficit, o catálogo de componentes SUI, as regras editoriais anti-"AI slop" e o guia de do/don't de Blazor. Não serve para tarefas só de backend, não-Blazor ou deploy de infraestrutura/consumers.
---

# SUI Design

Skill de frontend da Sufficit para UIs Blazor com
**Sufficit.Blazor.UI (SUI)**. Reúne marca, contratos dos
componentes, acessibilidade, responsividade e lifecycle.

## Como usar

Invocável como skill. Sem subcomando, faça triagem (abaixo) e siga o fluxo. Com um verbo implícito, mapeie para um fluxo:

| Fluxo | Quando | Saída |
|---|---|---|
| `craft [recurso]` | construir algo novo ponta a ponta | código production-grade |
| `shape [recurso]` | planejar antes do código | brief de design (sem código) |
| `audit [alvo]` | revisão técnica (a11y, perf, responsivo, theming, anti-patterns) | scorecard 0–20 + P0–P3 |
| `critique [alvo]` | revisão de UX/heurística | avaliação editorial |
| `polish [alvo]` | passada final pré-ship | detalhes que separam bom de ótimo |
| `colorize` / `typeset` / `layout` / `animate` / `quieter` / `bolder` | realces focados | ajustes na dimensão escolhida |

Em fluxos de implementação, entregue código pronto para produção. Em `shape`,
`audit` e `critique`, entregue o artefato descrito na tabela, com achados
verificáveis e próximos passos concretos.

## Setup antes de projetar (sempre)

1. **Identifique o consumer e a marca.** A biblioteca SUI é brand-agnóstica; cada app traz seu tema via `ISUITheme`. Confirme a cor primária do projeto-alvo (`sufficit-blazor` e `sufficit-cloud-mobile` = âmbar `#ee6321`; `Sufficit.Identity` = vermelho `#cc0000`). **Identity-preservation vence:** se o projeto já tem cor committed, NÃO reinvente a paleta.
2. **Leia `references/brand-and-theme.md`** para os tokens committed (paleta, neutros, raio, tipografia) e como o `SUIThemeProvider` os emite.
3. **Defina o registro:** `brand` (design É o produto: marketing, landing) ou `product` (design SERVE o produto: app, dashboard, painel). O registro muda as regras (cor, motion, densidade). Detalhe em `references/brand-and-theme.md`.
4. **Leia pelo menos um arquivo real do projeto** (um `.razor`, o tema, o CSS). Não reinvente; use o que existe quando funciona.
5. **Se o projeto é Blazor Server/interactive**, leia `references/blazor-patterns.md` (lifecycle, headings semânticos, contexto/auth).
6. **Se houver formulário em duas ou mais colunas**, leia
   `references/alignment-audit.md`. Para fields SUI, prefira `SUIFormGrid`, que
   já emite `data-sui-align-row`; em layouts mistos/legados, marque cada linha
   equivalente manualmente. Execute o gate geométrico em desktop e no último
   viewport antes do empilhamento. Zero containers ou zero comparações é falha.

## Marca Sufficit (resumo — detalhes em `references/brand-and-theme.md`)

- **Primária:** âmbar `#ee6321` (`--sufficit-amber`). Hover `#d1530e`, lighten `#f58a4b`.
- **Papéis de ação:** preserve o âmbar vivo em acentos, foco, tabs e contornos.
  Quando preto-sobre-âmbar deixar um botão preenchido visualmente pesado, use
  `PrimaryAction`/`PrimaryActionContrast` no tema (ember profundo + branco
  quente), nunca um hardcode por botão.
- **Modelo de cor: hex + `color-mix(in srgb, …)`. NÃO use OKLCH** no CSS Sufficit — não existe lá; introduzir OKLCH cria inconsistência.
- **Neutros (claro):** canvas `#f7f8fa`, surface `#ffffff`, surface-2 `#f4f5f7`, ink `#1f2226`, text-secondary `#475569`, muted `#6b7178`. Divisores tintados de âmbar (`#ebdbd5`).
- **Raio:** default `0.75rem` (0.625–0.875rem por superfície). Escala CSS `--radius-sm 4 / -md 8 / -lg 12 / -full 9999`.
- **Tipografia — armadilha conhecida:** o MudTheme referencia Poppins/Open Sans, mas o `App.razor` carrega de fato Roboto/Ubuntu/Montserrat; SUI defaulta pra system stack. **Nunca assuma que uma fonte referenciada está renderizando** — ou carregue-a ou alinhe o tema ao que está carregado.
- **Dark mode:** `DefaultSUITheme` permanece azul e brand-agnostic. Consumers
  âmbar devem fornecer `Primary` e, se aplicável, os tokens `PrimaryAction` no
  próprio tema antes de ativar dark mode.

## Convenções SUI (resumo — detalhes em `references/components.md`)

- Prefixo **`SUI`** (`SUIButton`, `SUIPageHeader`, …); classes **`.sui-*`**; tokens **`--sui-*`** em `:root`.
- **Não é Material Design:** flat, sombras suaves, raios generosos.
- Enums **próprios** em `SUIColor`, `SUIVariant` (Text/Outlined/Filled), `SUISize`, `SUIButtonType`, `SUIEdge`, `SUITypo`, `SUIAlign`, `SUITone` — **nunca** os de outra biblioteca visual.
- Tema via `services.AddSufficitUI(o => o.Theme = new MeuTema())` + `<SUIThemeProvider>`; sem provider, cai em `DefaultSUITheme` (azul claro). Dark via `[data-sui-theme="dark"]`; `.theme-dark` é somente alias legado.
- Class builder: `SUIClassBuilder.Default("sui-btn").AddClass(...).Slug(valor).Build()`.
- Assets: carregue `_content/Sufficit.Blazor.UI/sufficit-ui.css` **e** o
  `{Consumer}.styles.css`. Nunca inclua script SUI manualmente; Select,
  Tooltip, NavGroup, Tabs e Dialog importam módulos `.razor.js` colocalizados.
- Em APIs visuais novas use `ColorValue`, `VariantValue`, `SizeValue`,
  `ButtonTypeValue`, `EdgeValue` e `ToneValue`. Os nomes antigos sem `Value`
  são pontes `object`/string obsoletas até v2.

## Registros (definem as regras)

- **Brand (design É o produto):** tipografia com personalidade, `clamp()` fluido (razão ≥1.25), cor committed/drenched permitida, motion ambicioso (porém com `prefers-reduced-motion`). Para marketing/landing.
- **Product (design SERVE o produto):** uma família de fonte bem afinada, escala `rem` fixa (razão 1.125–1.2), cor contida (restringida ≤10% de acento), motion 150–250ms, alta densidade de informação. Para app/dashboard/painel — é o caso do `sufficit-cloud-mobile`.

## Regras gerais

**Cor**
- Contraste: texto de corpo **≥4.5:1**; texto grande (≥18px ou bold ≥14px) ≥3:1. Placeholder precisa dos mesmos 4.5:1, não o cinza mutado default. Falha mais comum: cinza mutado sobre quase-branco tintado.
- Cinza sobre fundo colorido fica lavado. Use tom mais escuro do próprio matiz do fundo, ou transparência da cor de texto.

**Tipografia**
- Linha de corpo entre 65–75ch. `text-wrap: balance` em h1–h3; `pretty` em prosa.
- Não case duas fontes parecidas (duas sans geométricas). Case num eixo de contraste (serif+sans) ou uma família em vários pesos.
- Heading de display: `clamp()` ≤6rem; letter-spacing ≥−0.04em.

**Layout**
- Varie espaçamento pra ter ritmo. **Cards são a resposta preguiçosa** — use só quando são a melhor affordance. Cards aninhados são sempre errado.
- Flexbox 1D, Grid 2D. Grid responsivo sem breakpoint: `repeat(auto-fit, minmax(280px, 1fr))`.
- Em formulários SUI horizontais use `SUIFormGrid`; reserve CSS manual para
  grids mistos ou relações que não sejam fields equivalentes.
- Campos equivalentes lado a lado devem alinhar topo do wrapper, label, topo e
  altura do controle. Use `align-items: start`, `min-width: 0`, elimine margens
  verticais herdadas e reserve a mesma altura de label quando ele puder quebrar
  linha. Valide a geometria renderizada; revisar apenas o CSS não fecha o gate.
- Escala semântica de z-index (dropdown → sticky → modal-backdrop → modal → toast → tooltip). Nunca `999`/`9999` arbitrário.

**Motion**
- Intencional, não reflexo. Não anime propriedades de layout. Ease-out exponencial (quart/quint/expo). Sem bounce/elastic.
- **`prefers-reduced-motion` não é opcional** — toda animação precisa de alternativa (crossfade ou instantâneo).
- Reveal deve realçar algo já visível por padrão; nunca gatear visibilidade de conteúdo numa transição triggerada por classe (pausa em aba oculta/renderer headless = section em branco).

**Interação**
- Dropdowns com `position:absolute` dentro de `overflow:hidden/auto` são cortados. Use `<dialog>`/popover API, `position:fixed` ou portal.

## Proibições absolutas (recuse e reescreva)

- **Side-stripe borders:** `border-left/right` >1px como acento colorido em cards/itens/alertas.
- **Gradient text:** `background-clip:text` + gradiente. Use cor sólida; ênfase via peso/tamanho.
- **Glassmorphism como default:** blur/vidro decorativo. Só raro e proposital.
- **Hero-metric template:** número grande + rótulo + stats + acento gradiente (clichê SaaS).
- **Grid de cards idênticos** repetido (ícone + título + texto, igualzinho).
- **Eyebrow uppercase tracked** acima de toda seção (`ABOUT`, `PROCESS`…). Um kicker deliberado é voz; em toda seção é gramática de IA.
- **Marcadores numerados `01/02/03`** como scaffolding default — só quando a seção É uma sequência real.
- **Texto que transborda o container** em algum breakpoint. Teste o copy em todos.

## AI slop test

Se alguém pode olhar e dizer "IA fez isso" sem dúvida, falhou. Cheque em duas altitudes: (1) o tema+paleta é adivinhável só pela categoria? (retrabalhe); (2) a família estética é adivinhável por categoria+anti-referências? (a armadilha um nível abaixo).

## Engenharia Blazor (resumo — detalhes em `references/blazor-patterns.md`)

- **Lifecycle:** não use `Task.Run` para I/O assíncrono. Faça `await` no hook
  apropriado quando o primeiro render puder aguardar; quando o shell precisar
  aparecer antes dos dados, inicie `LoadAsync` após o primeiro render, controle
  loading/error e finalize via `InvokeAsync(StateHasChanged)`. Mostre skeleton
  enquanto carrega e descadastre eventos no `Dispose`.
- **Headings semânticos:** títulos de página devem ser `<h1>` reais.
  `SUIText Typo="SUITypo.h1"`–`h6` já renderiza o heading correspondente com
  `Tag=Auto`; use `Tag="SUITextTag.H1"` quando aparência e nível semântico
  forem diferentes. `FocusOnNavigate Selector="h1"` precisa desse elemento.
- **Naming:** "Manager" não "Admin" (`User.IsManager()`); Guid PKs; comentários/commits em inglês.
- **Contexto multi-tenant (sufficit-cloud-mobile):** detail-by-id (instância/operação por id) deve buscar sempre em escopo agregado (`scope=all`); o id é o escopo, o server autoriza via `AccessibleTenantIds`/`CanSelectAnyTenant`. Coleções e mutações respeitam o contexto selecionado (`X-Sufficit-Context-Id`).

## Checklist de engenharia frontend

Princípios consolidados: lifecycle async, `StateHasChanged` via `InvokeAsync`, `Dispose`, skeletons, brand via `--sufficit-amber`, hex/`color-mix` não OKLCH, enums SUI, render-mode por rota, touch targets ≥44px. Ver `references/components.md` e `references/blazor-patterns.md`.
