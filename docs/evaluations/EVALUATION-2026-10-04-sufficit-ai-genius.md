# Avaliação Sufficit.Blazor.UI — 2026-10-04

> **Status da implementação (atualizado 2026-10-04):** roadmap em execução na branch
> `work/design-consolidation` (worktree `../sufficit-blazor-ui-consolidacao`).
> ✅ **V1 concluído** — escala `--sui-state-*` criada e consumida
> (commit `7d7d0f7`; nota de atividade: `docs/activities/20261004-design-consolidation-state-tints.md`).
> ✅ **V2 concluído** — tokens de foco/offset externo e interno aplicados; 902 testes
> unitários passaram (atividade: `docs/activities/20261004-design-consolidation-focus.md`).
> ✅ **V3 concluído** — política central de movimento reduzido em foundations
> (exceção: spinner pendente); 11 blocos periféricos removidos; contrato de estilo
> reforçado (903 testes passaram; atividade:
> `docs/activities/20261004-design-consolidation-reduced-motion.md`).
> ✅ **V4/V5/V7 concluídos** — tokens `--sui-dur-*`/`--sui-ease-*` decompostos,
> `--sui-radius-control` e `--sui-nav-item-h*`; nav na escala de espaçamento; contrato
> proíbe durações literais fora das exceções documentadas (904 testes; atividade:
> `docs/activities/20261004-design-consolidation-motion-nav-radius.md`).
> ✅ **V6/L2/P5 concluídos** — teste de paridade `ThemeParityTests` (achou e corrigiu
> divergência real no `--sui-color-secondary` dark), 12 estilos inline estáticos migrados
> para classes com guarda CSP, aliases `--sm/--md/--lg` marcados legados e marcação interna
> migrada para nomes canônicos (910 testes; atividade:
> `docs/activities/20261004-design-consolidation-parity-inline-aliases.md`).
> ✅ **P1/P2 iniciado e validado** — `SUIComponentBase` fino, 8 componentes pilotos
> (incluindo `SUISelect<T>` e `SUITextField<T>`), aliases `UserAttributes` obsoletos
> nos 3 migrados, guarda contra novos aliases e gerador de catálogo atualizado;
> baseline público revisado (924 testes; atividade:
> `docs/activities/20261004-design-consolidation-component-base.md`).
> ✅ **P4 concluído** — sobrecarga tipada `Add(message, SUITone, …)` nos dois
> serviços, `string severity` vira ponte DIM `[Obsolete]`, normalização
> `"error"→"danger"`/`Neutral→info` unificada em `SUIToneNormalizer` interno;
> baseline revisado, CHANGELOG atualizado (926 testes; atividade:
> `docs/activities/20261004-design-consolidation-feedback-tone.md`).
> ✅ **F1/F4 + L1/L4 concluídos** — densidade vira token em cascata
> (`--sui-density-scale`; alturas/paddings de controle derivam via `calc()`,
> áreas de toque de navegação ficam literais como piso de acessibilidade),
> `SUILayout.Z*` (7 tokens) e `SUITypography.FsField` entram no modelo e no
> writer; `.editorconfig` na raiz e `scripts/README.md` documentam Node×Python.
> Orçamentos de asset reajustados pela regra ×1,03 (928 testes; atividade:
> `docs/activities/20261004-design-consolidation-density-zindex-tooling.md`).
> Restam fora desta onda: F5 (convergência dos hosts + pausa hover, exige
> verificação de timer) e P3 (generics). A migração dos demais
> componentes para a base permanece progressiva.

**Avaliador:** Sufficit AI Genius (sessão autônoma)
**Commit de referência:** `2101904` (main)
**Método:** leitura direta do código-fonte (Razor, C#, CSS autoral, JS, testes, scripts e CI). Documentação do próprio projeto foi usada apenas como referência de intenção, nunca como prova — todos os achados abaixo citam o arquivo onde o comportamento foi observado.
**Foco pedido:** melhorias de **visual**, **padronização**, **funcionalidade**, **arquitetura**, **legibilidade/organização** e **suavização** (refinamento de estados, tints, motion).

---

## 1. Sumário executivo

A biblioteca está em um patamar raro para uma RCL interna: tokens publicados por provider com validação estrutural anti-injeção (`SUIThemeCssWriter`), validação de SVG inline (`SUIIconMarkup`), integração com `EditContext` sem herança (`SUIFieldBinding`), gates de API pública, budgets de bytes e testes de contrato de nomenclatura. A base de engenharia é forte.

O débito está concentrado exatamente nos eixos pedidos:

1. **Padronização** — 74 componentes sem classe base: cada um redeclara `Class`/`Style`/atributos splat, e há **duas convenções divergentes** (`UserAttributes` em 17 arquivos × `AdditionalAttributes` em 29). Generics usam `T` e `TValue` sem critério. Severity de snackbar/toast é string solta numa biblioteca que prega enums tipados.
2. **Visual/suavização** — os tints de interação (hover/active/pressed/soft) são números mágicos espalhados (7%, 8%, 11%, 14%, 18%, 38%, 62%…), a política de foco tem 4 offsets diferentes sem token, e `prefers-reduced-motion` deixou de cobrir 7 módulos CSS com transições. A escala de estados existe no discurso, não no código.
3. **Arquitetura** — a fronteira global × isolado está em 70%/30% e os módulos globais carregam nomes descritivos gigantes com ordem de cascata implícita em sufixo numérico (`…flyouts-1/2/3.css`). Tokens de z-index e `--sui-fs-field` existem só no CSS: o modelo C# de tema não os alcança. A paleta dark está duplicada entre CSS e C# sem fonte única.
4. **Legibilidade** — comentários de "porquê" são excelentes, mas há estilos inline hardcoded em componentes (que também contradizem o suporte a CSP estrita), enum público com membros `SUIEdge.False/True` e sobreposição de responsabilidade entre `SUIButton` (loading interno) e `SUILoadingButton` (loading por parâmetro).
5. **Funcionalidade** — densidade existe em 3 componentes e não é sistêmica; `SUITable` não oferece virtualização; a rampa tipográfica semântica do enum (`display/headline/title/body/label/mono`) não é usada por nenhum componente.

Nada disso bloqueia produção — os consumidores já rodam. É o tipo de débito que, se endereçado em uma única "temporada de consolidação", transforma a biblioteca de "boa para uso interno" em "referência que um time novo adota sem hesitar".

---

## 2. Pontos fortes (manter e não regredir)

| Força | Evidência |
|---|---|
| Publicação de tokens com validação anti-injeção (allowlist de funções CSS, fallback por token) | `src/Themes/SUIThemeCssWriter.cs` |
| Validação de fragmentos SVG antes de renderizar via `MarkupString` | `src/Components/DataDisplay/SUIIconMarkup.cs` |
| `EditContext` opcional sem forçar herança (`SUIFieldBinding<T>`) | `src/Utilities/SUIFieldBinding.cs` |
| Acessibilidade real: roving tabindex, `aria-errormessage`, forced-colors, alvos de toque 44px, foco visível | `SUITable.razor`, `SUITextField.razor`, `sui-foundations.css` |
| JS colocalizado, importado sob demanda, com dispose e guarda contra init duplo | `SUITable.razor` (`_interopInitialized`), `*.razor.js` |
| Gates de engenharia: API baseline, validação de pacote contra baseline publicado, budgets de bytes (raw/gzip/Brotli), testes de nomenclatura | `eng/PublicApiBaseline.txt`, `src/Sufficit.Blazor.UI.csproj`, `scripts/build-css.mjs`, `tests/…/NamingConventionTests.cs` |
| Comentários que explicam o **porquê**, não o quê (raros e valiosos) | remarks de `SUIThemeCssWriter`, `SUIIconMarkup`, `SUIDialogService` |

---

## 3. Achados e propostas por eixo

Cada achado traz: **evidência** (onde o código prova), **impacto** e **proposta** (mudança de design, não remendo).

### 3.1 Visual e suavização

#### V1 — Tints de interação são números mágicos espalhados

**Evidência:** `color-mix(in srgb, var(--sui-color-primary) N%)` aparece com **6 porcentagens distintas** em arquivos diferentes: 7% (hover de nav, `sui-shared-navigation-…-1.css:41`), 11% (nav ativa, `:55`), 14% (`--sui-color-primary-soft`, `sui-foundations.css`), 12% e 38%/62% (ícone de expand, `…-1.css`), além da família 8%/14%/18%/22%/26% documentada para botões (`sui-buttons.css`).

**Impacto:** cada superfície "suaviza" de um jeito. Hover de nav (7%) e hover de botão text (8%) são indistinguíveis na prática, mas divergem no código — e qualquer ajuste global de "tato" exige caçar literais. É o oposto do que o sistema de tokens promete.

**Proposta:** criar uma **escala de estados** em `sui-foundations.css` e consumi-la em todos os módulos:

```css
:root {
  --sui-state-hover:    color-mix(in srgb, var(--sui-color-primary) 8%, transparent);
  --sui-state-selected: color-mix(in srgb, var(--sui-color-primary) 12%, transparent);
  --sui-state-pressed:  color-mix(in srgb, var(--sui-color-primary) 16%, transparent);
  --sui-state-soft:     color-mix(in srgb, var(--sui-color-primary) 20%, transparent);
}
```

Três decisões de design embutidas: (a) colapsar 7%→8% e 11%→12% — a diferença é imperceptível; (b) expor os mesmos tokens no modelo C# (`SUILayout` + `SUIThemeCssWriter`), tornando-os tematizáveis por consumidor; (c) `--sui-color-primary-soft` passa a ser alias de `--sui-state-selected` para compatibilidade. Benefício colateral: o modo escuro herda automaticamente, pois os tints derivam do primário.

#### V2 — Política de foco com 4 offsets diferentes e sem token

**Evidência:** `outline-offset` aparece como `-2px` (4×), `1px` (2×), `2px` (7×) e `3px` (2×) nos módulos CSS. O `1px` isolado e a variação sem documentação tornam o foco visivelmente inconsistente entre componentes vizinhos (ex.: tab × botão × select no mesmo formulário).

**Proposta:** tokens `--sui-focus-ring: 2px solid var(--sui-focus-color)` e `--sui-focus-offset: 2px`, com o modificador `--sui-focus-offset-inset: -2px` reservado a superfícies full-bleed (nav, tabs). Padronizar `1px`→`2px`. Isso também prepara o terreno para o já existente `--sui-focus-shadow` do preset Linear — hoje os dois mecanismos (outline e shadow) coexistem sem uma camada que os relacione.

#### V3 — `prefers-reduced-motion` cobre só metade das transições

**Evidência:** 7 módulos com `transition:` **não** têm bloco `prefers-reduced-motion`: `sui-shared-app-shell-…`, `sui-shared-navigation-…-1.css`, `…-2.css`, `sui-shared-progress-linear.css`, `sui-shared-select-listbox.css`, `sui-shared-switch.css`, `sui-sort-label.css`. Outros 8+ módulos cobrem. Ou seja: o switch, o menu do select e o drawer animam mesmo quando o usuário pediu movimento reduzido.

**Impacto:** acessibilidade (WCAG 2.3.3) e suavização — exatamente os componentes mais "móveis" da biblioteca ficaram de fora.

**Proposta:** em vez de repetir blocos por arquivo, centralizar em `sui-foundations.css`:

```css
@media (prefers-reduced-motion: reduce) {
  .sui-root *, .sui-root *::before, .sui-root *::after {
    transition-duration: .01ms !important;
    animation-duration: .01ms !important;
  }
}
```

Um único ponto de controle, à prova de esquecimento quando nascer o próximo componente animado. (Portais anexados ao `body` precisam do mesmo tratamento no seletor de portais.)

#### V4 — Motion tokens incompletos: duração e easing não são separáveis

**Evidência:** `SUILayout.Transition` é a string combinada `"160ms cubic-bezier(.4, 0, .2, 1)"` (`src/Themes/SUILayout.cs:48`). Só existem duas durações (160/280ms) e o easing está embutido na string.

**Proposta:** decompor em `--sui-dur-fast` (120ms), `--sui-dur` (160ms), `--sui-dur-slow` (280ms), `--sui-ease-standard` / `--sui-ease-emphasized`, mantendo `--sui-transition` como atalho composto. Componentes que hoje animam "só um pouco mais rápido" (sort label, switch) passam a ter vocabulário sem inventar literais.

#### V5 — Navegação fora do sistema de tamanhos

**Evidência:** `min-height: 50px; padding: 11px 14px` hardcoded em `sui-shared-navigation-…-1.css:32-34`. 50px e 11px não pertencem à escala `--sui-space-*` (4/8/12/16) nem às alturas de controle (28/36/44), e a nav não tem parâmetro de densidade.

**Proposta:** `--sui-nav-item-h` (padrão 48px, que já é o alvo do rail) e padding derivado de `--sui-space-*`, com a nav respondendo ao mecanismo de densidade proposto em F1.

#### V6 — Paleta dark duplicada em CSS e C#

**Evidência:** os valores dark existem em `sui-foundations.css` (`[data-sui-theme="dark"]`) **e** em `SUITheme.Dark` (`src/Themes/SUITheme.cs`). Hoje conferem (conferido valor a valor), mas são duas fontes para o mesmo fato.

**Proposta:** aceitar a duplicação como **fallback intencional**, mas protegê-la: um teste de contrato que renderiza `SUITheme.Dark` via `SUIThemeCssWriter.Write` e compara, token a token, com o bloco dark do CSS autoral. Custo baixo, elimina a classe de erro "dark do C# ficou para trás".

#### V7 — Raio de botão derivado fora do token

**Evidência:** botões usam raio próprio (1,25× base) enquanto `--sui-radius` existe como token — o fator 1,25× vive no CSS de botões.

**Proposta:** `--sui-radius-control: calc(var(--sui-radius) * 1.25)` em foundations. Consumidores que mudam o raio base veem o botão acompanhar, em vez de divergir.

### 3.2 Padronização

#### P1 — Dois nomes para o mesmo parâmetro de splatting

**Evidência:** `UserAttributes` em 17 componentes (Actions, Forms, Overlays, Nav…) × `AdditionalAttributes` em 29 (DataDisplay, Feedback, Layout…). `grep -rl` nas duas convenções lista componentes vizinhos de mesma família em convenções diferentes.

**Impacto:** é o primeiro parâmetro que todo consumidor toca. Ter de lembrar "qual dos dois este componente usa" é atrito diário e quebra a previsibilidade da API; também impede refactors mecânicos.

**Proposta:** padronizar em `AdditionalAttributes` (convenção do próprio Blazor) e manter `UserAttributes` como alias `[Obsolete]` que encaminha — a ponte sai na próxima janela de quebra. O `NamingConventionTests` ganha uma regra que proíbe novos `UserAttributes`, seguindo o padrão já usado em `LegacyParameterNames`.

#### P2 — Nenhuma classe base: cada componente redeclara a infraestrutura

**Evidência:** `Class` redeclarado 54×, `Style` 24×, splatting 46× no total. `SUIButton.razor` chega a declarar `public void Dispose() { }` **vazio** com o comentário revelador "ComponentBase is the only base class". Cada componente também repete a dança `SUIClassBuilder.Default("sui-x").AddClass(Class).Build()`.

**Proposta:** introduzir `SUIComponentBase : ComponentBase` (interno à biblioteca, ou público e deliberado) com `Class`, `Style`, `AdditionalAttributes`, `EffectiveClass` resolvido e helpers de splat. Ganho triplo: (a) elimina as ~120 declarações repetidas; (b) resolve P1 por construção — o nome canônico vive em um lugar só; (c) cria o ponto único onde futuras políticas transversais (densidade, telemetria de render, ID estável) se aplicam a todos. Trade-off conhecido: herança em Blazor precisa de cuidado com `BuildRenderTree`; por isso a base deve ser fina (parâmetros e utilitários, zero markup). O `SUIFieldBinding` já provou o caminho de "infraestrutura sem herança" para o caso de formulários — a base cobre o resto.

#### P3 — Generics com nomes de type parameter inconsistentes

**Evidência:** `SUISelect T` e `SUITable T` × `SUIChoiceCard TValue`. A convenção Blazor (`TValue` para componentes de valor) é seguida só às vezes.

**Proposta:** `TValue` para componentes de seleção/entrada de valor (`SUISelect`), `TItem`/`T` para coleções (`SUITable`). Mudança de nome de type parameter é quebra de fonte para quem especifica explicitamente — incluir na janela de quebra e no `PublicApiBaseline` com nota no changelog. Adicionar regra no `NamingConventionTests`.

#### P4 — Severity de snackbar/toast é string solta

**Evidência:** `ISUISnackbar.Add(string message, string severity = "info", …)` e `ISUIToast.Add(…)`, com normalização `"error"→"danger"` **duplicada** em `SUISnackbarService.cs` e `SUIToastService.cs`. A biblioteca inteira prega enums tipados (`SUITone` existe para badges e alerts) — menos os dois serviços de feedback, que aceitam qualquer string e ainda mantêm um sinônimo.

**Proposta:** sobrecarga `Add(string message, SUITone tone, …)` como caminho preferido; a versão string vira ponte `[Obsolete]` delegando para um único `SUIToneNormalizer` interno. Remove a duplicação e alinha feedback ao restante do sistema.

#### P5 — Aliases de classe CSS mantidos "porque sim"

**Evidência:** `.sui-icon--sm, .sui-icon--small` e equivalentes md/lg em `sui-foundations.css:219-221`. Duas convenções de nome para o mesmo modificador, ambas públicas.

**Proposta:** eleger `--small/--medium/--large` (casa com `SUISize`), marcar `--sm/--md/--lg` como legado em um comentário de remoção e eliminar na janela de quebra. É pouco CSS, mas é o tipo de alias que se multiplica.

### 3.3 Funcionalidade

#### F1 — Densidade não é sistêmica

**Evidência:** parâmetro `Dense` existe apenas em `SUIList`, `SUITable` e `SUIAppBar`. A vitrine oferece densidade confortável/compacta, mas a biblioteca não expõe mecanismo — cada componente que quiser participar precisa nascer com o parâmetro.

**Proposta:** densidade como **token em cascata**, não como parâmetro por componente: `--sui-density-scale` (1 confortável / ~0.8 compacta) publicado pelo provider, com alturas e paddings derivando via `calc()`. Componentes mantêm `Dense` como atalho local, mas apps inteiros ganham densidade coerente de graça. É o padrão que o sistema já usa para cor; estender para métrica é o passo natural.

#### F2 — `SUITable` sem virtualização

**Evidência:** `SUITable.razor` materializa tudo (`Items?.ToArray()`, `@foreach` em todas as linhas). Não há `Virtualize` nem `ItemsProvider`.

**Proposta:** parâmetro `Virtualize` (opt-in) que troca o `@foreach` por `<Virtualize Items="…">` preservando `RowTemplate`, `RowKey` (via `ItemKey`), roving tabindex e o contrato de grid. Tabelas operacionais da Sufficit (listagens de milhares de registros) são o caso de uso exato. Trade-off: a paginação por composição (`SUIPagination`) continua sendo o caminho recomendado; a virtualização serve cenários de scroll contínuo.

#### F3 — Rampa tipográfica semântica é um enum órfão

**Evidência:** `SUITypo` tem duas famílias — `h1…h6/subtitle1/body1/body2` (Material-like) **e** `display/headline/title/body/label/mono` (semântica). Grep em todos os `.razor`/`.razor.cs`: **nenhum componente usa os 6 membros semânticos**; só `sui-shared-text.css` e `sui-shared-status-banner.css` consomem as variáveis. `SUIText` default é `body1`.

**Impacto:** API pública com 20 membros onde uma fatia não tem consumidor nem orientação — o consumidor não sabe qual família é "a certa".

**Proposta:** decisão de produto explícita: (a) se a família semântica é o futuro, migrar os defaults internos (`SUIStatusBanner` já usa `--sui-fs-headline`), documentar o mapeamento h1→display etc. e deprecar a família Material na janela de quebra; (b) se não é, remover os 6 membros e as variáveis. O que não funciona é manter as duas "só por garantia".

#### F4 — Tokens de z-index e `--sui-fs-field` fora do modelo de tema

**Evidência:** `--sui-z-dropdown…tooltip` (7 tokens) e `--sui-fs-field` existem apenas em `sui-foundations.css`. `SUILayout.cs` (22 propriedades) e `SUITypography.cs` (29) não os contêm — ou seja, o consumidor não pode tematizá-los, e o `SUIThemeCssWriter` não os publica.

**Proposta:** `SUILayout.ZIndex*` (ou um `SUIZIndex` aninhado) e `SUITypography.FsField` no modelo C#, com o writer publicando. O campo de formulário é justamente o token que um consumidor com tipografia própria mais precisa ajustar — hoje ele não alcança.

#### F5 — Snackbar e Toast: dois sistemas paralelos de feedback transitório

**Evidência:** `ISUISnackbar`/`SUISnackbarHost` e `ISUIToast`/`SUIToastHost` têm a mesma estrutura (entry record com `Id/Message/Tone/ExpiresAt`, serviço com `OnEnqueue`, host que auto-dispensa). O Toast adiciona ação. Nenhum dos dois pausa o timer no hover/focus (verificar antes de implementar).

**Proposta:** convergir internamente: um `SUINotificationService` único com `NotificationKind.Banner|Toast` (ou manter as duas interfaces públicas como fachadas sobre ele). Público não muda; a lógica de fila, expiração e acessibilidade (pausa em hover, `aria-live`) vive uma vez. A pausa em hover é requisito WCAG 2.2.1 para conteúdo que expira — incorporar na convergência.

### 3.4 Arquitetura

#### A1 — Ordem de cascata carregada em nome de arquivo

**Evidência:** `sui-shared-navigation-links-groups-collapse-and-desktop-rail-flyouts-1.css`, `-2.css`, `-3.css`; `sui-shared-app-shell-layout-appbar-drawer-container-main-content.css`; `sui-shared-table-empty-no-records.css`; `sui-shared-stack-flex-column-row.css`. O sufixo `-1/-2/-3` existe porque a ordem de import em `sui-components.css` é semanticamente relevante — ou seja, **a cascata é mantida por convenção de nomenclatura**, e o nome virou um parágrafo.

**Impacto:** legibilidade de organização (o pior eixo do projeto hoje). Ninguém consegue nomear o arquivo sem ler o conteúdo; o `-2` não diz nada; renomear exige tocar imports e scripts de verificação.

**Proposta:** renomear por **assunto**, não por resumo do conteúdo: `sui-nav.css`, `sui-nav-rail.css`, `sui-nav-collapse.css` (a ordem continua explícita — mas no `sui-components.css`, com comentário de cascata, onde pertence). Mesma receita: `sui-app-shell.css`, `sui-table-empty.css`, `sui-stack.css`. Mudança mecânica com valor desproporcional: a pasta `src/styles` passa a se ler como um sumário.

#### A2 — Fronteira global × isolado em 70/30 precisa de política de destino

**Evidência:** CSS global em `src/styles` soma ~82KB contra ~34,7KB em 18 `*.razor.css`. A direção declarada é "híbrido, em migração".

**Proposta:** não se trata de inverter a proporção (o global compartilhado é uma escolha defensável e barata — o bundle inteiro tem ~10,5KB gzip). Trata-se de **fechar a regra**: qualquer estilo novo nasce em `*.razor.css`, a menos que seja (a) token/fundação, (b) portal anexado ao `body`, ou (c) primitiva compartilhada por 3+ componentes. Codificar a regra no `StyleContractTests` (lista de módulos globais permitidos que só encolhe) — o mesmo padrão de "débito que só decresce" já usado em `FileSizeBudgetTests.Debt`.

#### A3 — Estilos inline hardcoded contradizem o suporte a CSP

**Evidência:** `SUITextField.razor` embute `style="position:absolute;inset-block-start:50%;…"` (botão clear) e `style="height:auto;min-height:108px;…"` (textarea) diretamente no markup; o padrão se repete em outros componentes. Ao mesmo tempo, `SUIThemeProvider` expõe `Nonce` justamente para hosts com `Content-Security-Policy` estrita.

**Impacto:** atributos `style=` exigem `style-src 'unsafe-inline'` — o host que segue a recomendação de CSP da própria biblioteca quebra nos componentes. E legibilidade: 200+ caracteres de CSS dentro de atributo.

**Proposta:** mover todo `style=` hardcoded para classes nos respectivos `.razor.css`/módulos compartilhados (`sui-text-field__clear`, `sui-field__input--multiline` já existem — basta completá-las). Manter `style=` apenas onde o valor é **computado em runtime** (posicionamento de popover, tamanhos dinâmicos). Um teste de contrato que falha em `style=` com valor literal estático nos `.razor` impede regressão.

#### A4 — `SUIButton` e `SUILoadingButton`: dois mecanismos de loading

**Evidência:** `SUIButton` gerencia `IsLoading` **internamente** durante o click (seta true, invoca, false em `finally`). `SUILoadingButton` recebe `IsLoading` como **parâmetro** do pai e ainda adiciona `InstantFeedback` via JS. Um consumidor que migra de um para outro inverte quem controla o estado — e um `SUIButton` com handler longo já mostra spinner sozinho, tornando o LoadingButton redundante em parte dos casos.

**Proposta:** declarar `SUILoadingButton` o componente de loading **explícito** (async controlado pelo pai, feedback instantâneo) e extrair o auto-loading do `SUIButton` para um parâmetro opt-in (`AutoLoading`), default preservando o comportamento atual. Documentar a matriz de decisão: click rápido → `SUIButton`; operação async longa → `SUILoadingButton`. A longo prazo, o LoadingButton herda da mesma base (P2) e a diferença vira um único comportamento composto.

#### A5 — Resolução de tema via service locator, sem reatividade

**Evidência:** `SUIThemeProvider.OnParametersSet` chama `Services.GetService(typeof(ISUITheme))` a cada ciclo de parâmetros; `AddSufficitUI` captura a instância de tema **uma vez** no registro. Trocar o tema em runtime exige passar `Theme=` no provider (como a vitrine faz) — o caminho via DI não reage.

**Proposta:** aceitar como desenho (o parâmetro cobre o caso real) mas documentar de forma explícita no contrato do provider, e considerar um `ISUIThemeAccessor` scoped mutável apenas se surgir consumidor com troca de tema por DI. Não inflar a API antes da demanda.

#### A6 — `SUIEdge.False/True/Center` na API pública

**Evidência:** `SUIEnums.cs` — enum público cujos membros são `False`, `True`, `Center`. `SUIEdge.True` não se lê como forma de borda; é herança da ponte com o enum legado.

**Proposta:** introduzir `SUIEdgeShape { Square, Circle, Center }` (ou renomear membros na janela de quebra com aliases `[Obsolete]`), mantendo a ponte. A validação de pacote contra baseline vai acusar a remoção — por isso entra na lista da janela, não como patch casual.

### 3.5 Legibilidade e organização

#### L1 — Ausência de `.editorconfig`

**Evidência:** nenhum `.editorconfig` na raiz (confirmado). Com múltiplos autores (humanos e agentes), formatação e convenções de `using` ficam ao gosto de cada sessão.

**Proposta:** `.editorconfig` mínimo (indentação, `using` ordenados com `System` primeiro, preferências de `var`, file-scoped namespaces — que o código já segue) + `dotnet format --verify-no-changes` no CI. Custo de uma tarde; elimina diffs de estilo para sempre.

#### L2 — `SUIClassBuilder`: overload ambíguo

**Evidência:** `AddClass(string? value, bool when)` e `AddClass(string? value, Func<bool>? when = null)` — `AddClass("x")` resolve para o segundo (adiciona sempre). Funciona, mas a leitura exige saber qual overload foi chamado.

**Proposta:** renomear o incondicional para `Add` (já existe!) e remover o default do `Func<bool>`, tornando a condição sempre explícita: `AddClass("x", when: IsActive)`. Micro-mudança, leitura imediata.

#### L3 — Duplicação `src/styles` ↔ `src/wwwroot/styles`

**Evidência:** 38 arquivos idênticos nos dois caminhos (diff conferido: zero divergências hoje), sincronizados pelo `build:css` e protegidos pelo `check:css`.

**Avaliação:** duplicação **gerenciada** — os gates impedem drift. Não é dívida urgente; registrar no README dos estilos que `wwwroot/styles` é artefato gerado (comentário de cabeçalho nos arquivos copiados bastaria) para ninguém editar o espelho.

#### L4 — `scripts/` mistura Python e Node sem convenção visível

**Evidência:** `build-css.mjs`, `check-lighthouse.mjs` ao lado de `generate-catalog.py`, `release_version.py`, `__pycache__` versionado no .gitignore mas presente na árvore.

**Proposta:** `scripts/node/` e `scripts/python/`, ou um `scripts/README.md` de 10 linhas dizendo qual linguagem cobre o quê e por quê. Organização, não funcionalidade.

---

## 4. Pontuação por dimensão (0–10)

| Dimensão | Nota | Justificativa objetiva |
|---|---|---|
| **Visual / design tokens** | 7,5 | Fundações sólidas (cores, espaço, elevação, motion) com provider e validação; perde pontos pelos tints mágicos (V1), foco sem token (V2), motion incompleto (V3/V4) e nav fora da escala (V5). |
| **Padronização de API** | 6,5 | Enums próprios e convenções fortes em 90% da superfície; derrubam a nota o splatting duplo (P1), ausência de base (P2), generics inconsistentes (P3), severity string (P4) e `SUIEdge.False/True` (A6). |
| **Funcionalidade** | 7,0 | Cobertura de 73 componentes com contratos de forms e overlays completos; faltam densidade sistêmica (F1), virtualização de tabela (F2), tokens de z-index/fs-field tematizáveis (F4) e a decisão da rampa tipográfica (F3). |
| **Arquitetura** | 7,5 | Limites claros (temas, serviços, JS colocalizado, DI), gates de API e de bytes; débitos: cascata por nome de arquivo (A1), política global×isolado não codificada (A2), inline styles vs CSP (A3), loading duplicado (A4). |
| **Legibilidade / organização** | 7,0 | Comentários de "porquê" exemplares e arquivos com tamanho disciplinado; penalizada pelos nomes de CSS-parágrafo (A1), falta de `.editorconfig` (L1), aliases CSS (P5) e estilos inline (A3). |
| **Acessibilidade** | 8,5 | axe em 3 browsers, roving tabindex, forced-colors, 44px, focus trap; gap concreto: reduced-motion em 7 módulos (V3) e pausa de toast/snackbar (F5). |
| **Prontidão de produção** | 8,5 | CI com warnings-as-errors, baseline de pacote, release com OIDC, vitrine publicada, consumidores reais. |
| **Geral** | **7,5** | Engenharia de biblioteca madura com uma camada de consolidação de sistema de design pela frente. |

---

## 5. Roadmap priorizado

**P0 — suavização imediata (1–2 dias cada, sem quebra):**
1. Escala de estados `--sui-state-*` e varredura dos tints mágicos (V1)
2. Reduced-motion centralizado (V3)
3. Tokens de foco `--sui-focus-ring/-offset` (V2)
4. Mover `style=` hardcoded para classes (A3)

**P1 — consolidação de API (janela de quebra única, 1 sprint):**
5. `SUIComponentBase` com `Class/Style/AdditionalAttributes` unificados + alias obsoleto de `UserAttributes` (P2+P1)
6. `SUITone` em snackbar/toast + normalizador único (P4) e convergência interna dos dois hosts (F5)
7. Type parameters `TValue`/`TItem` (P3), remoção de aliases `--sm/--md/--lg` (P5), `SUIEdge` renomeado com ponte (A6)
8. Renomear módulos CSS por assunto + regra de fronteira no `StyleContractTests` (A1+A2)
9. Decisão da rampa tipográfica semântica (F3)

**P2 — expansão funcional (sprints seguintes):**
10. Densidade sistêmica via token (F1)
11. `SUITable` com `Virtualize` opt-in (F2)
12. Tokens de z-index e `FsField` no modelo C# (F4) + teste de paridade dark CSS×C# (V6)
13. `.editorconfig` + `dotnet format` no CI (L1); organização de `scripts/` (L4)
14. `AutoLoading` opt-in no `SUIButton` e matriz de decisão documentada (A4)

---

## 6. Veredito

**Recomendo a padronização em Sufficit.Blazor.UI para aplicações Sufficit novas, sem ressalvas de fundação.** Os alicerces que costumam falhar em bibliotecas internas — segurança de tema, contrato de API, acessibilidade, empacotamento — aqui são os pontos mais fortes.

Os riscos que restam são de **coerência de sistema de design**, não de competência de engenharia: tints e foco sem tokens, duas convenções de splatting, cascata em nome de arquivo, uma tipografia dupla indecisa e dois botões de loading. Todos têm solução de design clara (itens 1–9 do roadmap) e nenhum exige reescrita — é uma temporada de consolidação com janela de quebra única e honesta.

Critério de sucesso sugerido: quando um componente novo puder ser escrito **sem decidir** qual nome de splatting usar, qual porcentagem de tint aplicar no hover e em qual arquivo global colocar o CSS — porque o sistema já decidiu —, a consolidação estará completa.
