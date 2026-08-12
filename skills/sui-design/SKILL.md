---
name: sui-design
description: Projete, construa, critique, refine e publique interfaces frontend em projetos Blazor da Sufficit que usam a biblioteca Sufficit.Blazor.UI (SUI) e/ou MudBlazor. Use quando o usuário quiser criar ou refinar uma página, componente, layout, tema ou token de design em Blazor; aplicar a marca Sufficit (âmbar #ee6321, modelo hex + color-mix, NÃO OKLCH); seguir as convenções SUI (prefixo SUI, tokens --sui-*, SUIThemeProvider, enums próprios); auditar acessibilidade, contraste, responsividade ou lifecycle do Blazor; ou publicar uma release (eveo-apps). Consolida o sistema de marca Sufficit, o catálogo de componentes SUI, as regras editoriais anti-"AI slop", o guia de do/don't de Blazor e os padrões de engenharia e deploy praticados no sufficit-cloud-mobile. Não serve para tarefas só de backend ou não-Blazor.
---

# SUI Design

Skill de frontend da Sufficit para UIs Blazor que usam a biblioteca **Sufficit.Blazor.UI (SUI)** — e projetos irmãos em MudBlazor. Consolida o melhor de duas skills externas (impeccable = filosofia/qualidade; ui-ux-pro-max = do/don't por stack) **mais** o sistema de marca Sufficit, o catálogo SUI e os padrões de engenharia/deploy que vivemos no `sufficit-cloud-mobile`.

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
| `ship` | publicar no `eveo-apps` | release ativa + health check (ver `references/shipping.md`) |

Saída sempre em **código pronto para produção**, não protótipo. Até estar completo: bonito, responsivo, rápido, acessível, on-brand.

## Setup antes de projetar (sempre)

1. **Identifique o consumer e a marca.** A biblioteca SUI é brand-agnóstica; cada app traz seu tema via `ISUITheme`. Confirme a cor primária do projeto-alvo (`sufficit-blazor` e `sufficit-cloud-mobile` = âmbar `#ee6321`; `Sufficit.Identity` = vermelho `#cc0000`). **Identity-preservation vence:** se o projeto já tem cor committed, NÃO reinvente a paleta.
2. **Leia `references/brand-and-theme.md`** para os tokens committed (paleta, neutros, raio, tipografia) e como o `SUIThemeProvider` os emite.
3. **Defina o registro:** `brand` (design É o produto: marketing, landing) ou `product` (design SERVE o produto: app, dashboard, painel). O registro muda as regras (cor, motion, densidade). Detalhe em `references/brand-and-theme.md`.
4. **Leia pelo menos um arquivo real do projeto** (um `.razor`, o tema, o CSS). Não reinvente; use o que existe quando funciona.
5. **Se o projeto é Blazor Server/interactive**, leia `references/blazor-patterns.md` (lifecycle, headings semânticos, contexto/auth).

## Marca Sufficit (resumo — detalhes em `references/brand-and-theme.md`)

- **Primária:** âmbar `#ee6321` (`--sufficit-amber`). Hover `#d1530e`, lighten `#f58a4b`.
- **Modelo de cor: hex + `color-mix(in srgb, …)`. NÃO use OKLCH** no CSS Sufficit — não existe lá; introduzir OKLCH cria inconsistência.
- **Neutros (claro):** canvas `#f7f8fa`, surface `#ffffff`, surface-2 `#f4f5f7`, ink `#1f2226`, text-secondary `#475569`, muted `#6b7178`. Divisores tintados de âmbar (`#ebdbd5`).
- **Raio:** default `0.75rem` (0.625–0.875rem por superfície). Escala CSS `--radius-sm 4 / -md 8 / -lg 12 / -full 9999`.
- **Tipografia — armadilha conhecida:** o MudTheme referencia Poppins/Open Sans, mas o `App.razor` carrega de fato Roboto/Ubuntu/Montserrat; SUI defaulta pra system stack. **Nunca assuma que uma fonte referenciada está renderizando** — ou carregue-a ou alinhe o tema ao que está carregado.
- **Dark mode — bomba latente:** o dark palette default do SUI usa **azul `#3b82f6`** (off-brand). Se for ligar dark mode num consumer âmbar, sobrescreva o primário no tema do consumer.

## Convenções SUI (resumo — detalhes em `references/components.md`)

- Prefixo **`SUI`** (`SUIButton`, `SUIPageHeader`, …); classes **`.sui-*`**; tokens **`--sui-*`** em `:root`.
- **Não é Material Design:** flat, sombras suaves, raios generosos.
- Enums **próprios** em `SUIColor`, `SUIVariant` (Text/Outlined/Filled), `SUISize`, `SUIButtonType`, `SUIEdge`, `SUITypo`, `SUIAlign`, `SUITone` — **não** os do MudBlazor.
- Tema via `services.AddSufficitUI(o => o.Theme = new MeuTema())` + `<SUIThemeProvider>`; sem provider, cai em `DefaultSUITheme` (azul claro). Dark via `[data-sui-theme="dark"]`.
- Class builder: `SUIClassBuilder.Default("sui-btn").AddClass(...).Slug(valor).Build()`.

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

- **Lifecycle:** NUNCA bloqueie a UI em `OnInitializedAsync`/`OnParametersSetAsync`. Dispare `_ = Task.Run(LoadAsync)` em `OnAfterRender(firstRender)`, trackee `IsLoading`, e sempre `await InvokeAsync(StateHasChanged)` de threads de background. Mostre `<SUISkeletonLoader>`/`<MudSkeleton>` enquanto carrega. Descadastre eventos no `Dispose`.
- **Headings semânticos:** títulos de página devem ser `<h1>` reais. `SUIText` renderiza `<div>` — não serve para heading; aplique a classe `sui-text--h5` num `<h1>` (visual idêntico, semântica real) para o `FocusOnNavigate` ter alvo e o leitor de tela navegar.
- **Naming:** "Manager" não "Admin" (`User.IsManager()`); Guid PKs; comentários/commits em inglês.
- **Contexto multi-tenant (sufficit-cloud-mobile):** detail-by-id (instância/operação por id) deve buscar sempre em escopo agregado (`scope=all`); o id é o escopo, o server autoriza via `AccessibleTenantIds`/`CanSelectAnyTenant`. Coleções e mutações respeitam o contexto selecionado (`X-Sufficit-Context-Id`).

## Do/Don't por stack (do ui-ux-pro-max)

Princípios consolidados: lifecycle async, `StateHasChanged` via `InvokeAsync`, `Dispose`, skeletons, brand via `--sufficit-amber`, hex/`color-mix` não OKLCH, enums SUI, render-mode por rota, touch targets ≥44px. Ver `references/components.md` e `references/blazor-patterns.md`.

## Publicar (ship)

Release manual no host `eveo-apps` (`/opt/sufficit-cloud-mobile`, symlinks por componente). Sem CD — o CI só builda/testa. Flow completo, health checks e rollback em **`references/shipping.md`**. Resumo:

1. `dotnet publish` (net10, Release, framework-dependent) do projeto alterado;
2. `rsync` para `/opt/sufficit-cloud-mobile/releases/<slug>/<component>/`;
3. swap atômico do symlink + `systemctl restart sufficit-cloud-mobile-<component>`;
4. health checks (`systemctl is-active`, `/` → 302 Identity, CSS novo, `/health/ready` 200, `nginx -t`).
