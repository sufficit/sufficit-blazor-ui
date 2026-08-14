# Plano — arquitetura e hardening da biblioteca SUI

**Status:** Concluído e arquivado em 2026-08-14
**Criado:** 2026-08-14
**Escopo:** `sufficit-blazor-ui` e validação dos consumidores SUI existentes
**Baseline da auditoria:** 10/20; 0 P0, 8 P1, 8 P2 e 2 P3

## Objetivo

Evoluir a `Sufficit.Blazor.UI` de uma coleção funcional de componentes Blazor
com assets globais para uma Razor Class Library organizada, testável,
acessível e publicável, preservando o contrato público e a identidade visual
dos consumidores atuais.

Ao fim do plano, a biblioteca deve ter:

- componentes agrupados por família, com arquivos relacionados colocalizados;
- CSS híbrido: fundações compartilhadas globais e estilos exclusivos isolados
  por componente;
- módulos JavaScript colocalizados, autocontidos e descartáveis;
- catálogo executável com estados claros, escuros, responsivos e interativos;
- testes automatizados de renderização, teclado, tema, pacote e consumidores;
- pipeline de build e publicação reproduzível para `net9.0` e `net10.0`;
- zero achado P0/P1 na auditoria técnica final.

## Fora de escopo

- rebrand ou substituição da linguagem visual SUI;
- adoção de Material Design, MudBlazor, Sass ou outro framework de UI;
- componentes acoplados a telefonia, financeiro, mensageria ou outro domínio;
- tree-shaking de CSS por componente em runtime;
- pasta e README obrigatórios para cada componente simples;
- remoção imediata das pontes de compatibilidade sem janela de depreciação;
- publicação direta em produção sem validação nos consumidores.

## Evidências e situação atual

### Estrutura e assets

- O projeto usa `Microsoft.NET.Sdk.Razor` e compila como RCL Blazor pura.
- O projeto tem 49 componentes em `src/Components`, todos no mesmo diretório.
- Há um único code-behind colocalizado: `SUINavGroup.razor.cs`.
- Não há arquivos `.razor.css` nem `.razor.js`.
- `src/wwwroot/sufficit-ui.css` tem 2.192 linhas e 70.712 bytes.
- `src/wwwroot/sufficit-ui.js` tem 469 linhas e 17.102 bytes.
- No build Release do .NET 10, o CSS resulta em aproximadamente 10,7 KB
  Brotli e o JavaScript em aproximadamente 3,5 KB Brotli.
- Os static web assets recebem fingerprint, gzip/Brotli e metadados de cache;
  minificação manual não é o gargalo prioritário.

### Build, pacote e automação

- `dotnet build --configuration Release -warnaserror` passa para os dois TFMs
  com zero erro e zero warning.
- `dotnet pack` falha com `NU5019` porque o projeto ainda procura
  `../../icon.png` e `../../README.md` depois do achatamento do diretório.
- O workflow de publicação ainda procura
  `src/Sufficit.Blazor.UI/Sufficit.Blazor.UI.csproj`, caminho inexistente.
- O diretório NuGet do Dependabot também aponta para `/src/Sufficit.Blazor.UI`
  em vez de `/src`.
- O CI atual valida compilação, mas não executa testes de componentes, pacote,
  acessibilidade ou browser.

### Qualidade técnica

| Dimensão | Nota | Evidência principal |
| --- | ---: | --- |
| Acessibilidade | 1/4 | labels sem associação, teclado incompleto, dialog sem foco e contraste insuficiente |
| Performance | 3/4 | assets pequenos/comprimidos, porém módulo JS global e observers amplos |
| Responsividade | 2/4 | grid e breakpoints bons, mas controles de 28/36 px no mobile |
| Theming | 2/4 | contrato de tokens consistente, porém emissão dark inválida |
| Integridade | 2/4 | sistema coerente, mas sem testes e com pack/publicação quebrados |
| **Total** | **10/20** | **Aceitável; trabalho significativo necessário** |

### Achados priorizados

| ID | Prioridade | Área | Achado |
| --- | --- | --- | --- |
| REL-01 | P1 | Release | pacote falha por caminhos antigos de README/ícone |
| REL-02 | P1 | Release | workflow de publicação usa caminho de projeto inexistente |
| THEME-01 | P1 | Theming | provider dark gera regra CSS sem seletor e ignora a paleta customizada |
| A11Y-01 | P1 | Select | `preventDefault` incondicional pode bloquear Tab |
| A11Y-02 | P1 | Forms | labels e helpers não estão associados aos controles |
| A11Y-03 | P1 | Overlay | dialog não gerencia foco, Escape nem restauração de foco |
| A11Y-04 | P1 | Cor | branco sobre âmbar/info/success/warning falha em texto pequeno |
| JS-01 | P1 | Tooltip | tooltip não importa o próprio módulo e pode ficar inerte isoladamente |
| A11Y-05 | P2 | Autocomplete | falta padrão combobox, teclado e anúncio de resultados |
| A11Y-06 | P2 | Tabs/Table/List | padrões ARIA e teclado estão incompletos |
| RESP-01 | P2 | Mobile | alvos interativos médios/pequenos ficam abaixo de 44 px |
| MOTION-01 | P2 | Motion | reduced motion não cobre spinner, snackbar, dialog e progress |
| TEST-01 | P2 | Qualidade | não existe catálogo executável nem projeto de testes |
| JS-02 | P2 | Performance | um módulo instala listeners e observer globais para três recursos |
| API-01 | P2 | API | parâmetros visuais `object` reduzem segurança de tipos |
| DOC-01 | P2 | Docs | README e skill divergem do código e dos consumidores atuais |
| STYLE-01 | P3 | Visual | alerta usa faixa lateral de 4 px, proibida pela convenção SUI atual |
| PERF-01 | P3 | Motion | progress linear anima `width` em vez de `transform` |

## Decisões arquiteturais

### 1. Organização por família, não pasta obrigatória por componente

Estrutura-alvo:

```text
src/
├── Components/
│   ├── Actions/
│   ├── DataDisplay/
│   ├── Feedback/
│   ├── Forms/
│   ├── Layout/
│   ├── Navigation/
│   └── Overlays/
├── Services/
├── Themes/
├── Utilities/
└── wwwroot/
    └── styles/
        ├── sui-foundations.css
        └── sui-portals.css
```

Arquivos com o mesmo basename devem ficar adjacentes:

```text
Components/Forms/
├── SUISelect.razor
├── SUISelect.razor.cs
├── SUISelect.razor.css
├── SUISelect.razor.js
└── SUISelectItem.razor
```

Criar subdiretório exclusivo apenas quando um componente tiver quatro ou mais
arquivos/colaboradores próprios e a pasta de família deixar de ser legível.
`SUISelect` e `SUINavGroup` são candidatos; componentes simples não são.

Os componentes já declaram o namespace
`Sufficit.Blazor.UI.Components`, portanto a movimentação física não deve mudar
o namespace ou o nome público dos tipos.

### 2. CSS híbrido

Permanecem globais:

- tokens `--sui-*`, `color-scheme`, tipografia, spacing e elevação;
- helpers públicos e primitives intencionalmente compartilhados;
- `.sui-btn`, compartilhado por button/loading/icon button;
- `.sui-field`, compartilhado por text/numeric/select/autocomplete;
- `.sui-icon`, usada inclusive por markup SVG interno de outros componentes;
- escala de z-index e estilos de elementos criados no `<body>`;
- tooltip/portal quando o elemento flutuante não recebe o scope do componente.

Podem migrar para `.razor.css`:

- estilos exclusivos do markup emitido pelo próprio componente;
- responsive rules exclusivas desse componente;
- focus, hover, selected, disabled e reduced-motion exclusivos;
- custom properties privadas do componente (`--_...`).

Regras que atravessam `RenderFragment`, componentes filhos ou portais devem:

1. permanecer em um stylesheet compartilhado; ou
2. usar `::deep` de forma explícita e coberta por teste; ou
3. ser substituídas por uma API/classe pública no componente filho.

CSS isolation melhora propriedade e evita vazamento de seletores, mas não deve
ser vendido como code splitting: a RCL gera um bundle importado no stylesheet
do consumidor.

### 3. JavaScript colocalizado e autocontido

Separar o módulo atual em:

- `SUISelect.razor.js` — top layer, posicionamento e cleanup do menu;
- `SUITooltip.razor.js` — registro do anchor, superfície flutuante e cleanup;
- `SUINavGroup.razor.js` — posicionamento e estado de interação do rail.

Cada componente deve importar seu próprio módulo usando o caminho estável:

```text
./_content/Sufficit.Blazor.UI/{PATH}/{COMPONENT}.razor.js
```

O prefixo `./` é obrigatório para respeitar o `<base href>` de aplicações
hospedadas fora da raiz. Cada módulo deve expor inicialização/registro e
descarte; observers e listeners não podem permanecer ativos após a remoção do
componente.

### 4. Documentação viva

Não criar README por componente. A documentação deve ser dividida em:

- XML docs no contrato público;
- `docs/USAGE-COMPONENTS-*.md` por família ou componente complexo;
- catálogo executável com exemplos reais e estados extremos;
- matriz de compatibilidade/migração para consumidores;
- atividade datada ao concluir este plano.

## Ordem e dependências

```text
Fase 0 (release confiável)
  └─> Fase 1 (catálogo e testes baseline)
       └─> Fase 2 (P1 funcional/a11y/theming)
            └─> Fase 3 (organização física)
                 ├─> Fase 4 (JS colocalizado)
                 └─> Fase 5 (CSS híbrido)
                      └─> Fase 6 (hardening sistêmico)
                           └─> Fase 7 (API e docs)
                                └─> Fase 8 (consumidores)
                                     └─> Fase 9 (gates finais)
```

As fases 4 e 5 podem ocorrer em branches separadas depois da Fase 3, mas o
rollout nos consumidores só começa quando ambas estiverem integradas e verdes.

## Fases de implementação

Cada fase termina com um gate verificável e pode ser entregue sem deixar a
biblioteca em estado intermediário quebrado.

### Fase 0 — restaurar release e baseline reproduzível

- [x] Corrigir os itens de pacote para `../icon.png` e `../README.md`.
- [x] Corrigir restore/build/pack do workflow para
      `src/Sufficit.Blazor.UI.csproj`.
- [x] Corrigir o diretório NuGet do Dependabot para `/src`.
- [x] Adicionar job de `dotnet pack` em pull requests sem publicar o pacote.
- [x] Inspecionar o `.nupkg`: DLLs dos dois TFMs, README, ícone, CSS e JS.
- [x] Tornar explícito no CI que `net9.0` e `net10.0` são suportados.
- [x] Revisar os `PackageReference` flutuantes `9.*`/`10.*` e documentar a
      política de atualização/reprodutibilidade.
- [x] Corrigir no README: TFMs, número/estado dos consumidores, pacote,
      contrato de tema, uso de JavaScript e componentes existentes.

**Gate da fase:** build com warnings como erro e pack passam em ambiente limpo;
o pacote pode ser instalado em um consumidor mínimo para os dois TFMs.

**Evidência (2026-08-14):** builds Release `net9.0` e `net10.0` passaram com
zero warning; `Sufficit.Blazor.UI.1.26.814.1358.nupkg` foi gerado e validado por
`scripts/validate-package.sh`. O pacote contém as duas DLLs, README, ícone, CSS
e JavaScript e foi restaurado/compilado em RCLs mínimas dos dois TFMs.

### Fase 1 — criar catálogo e rede de segurança

- [x] Criar `samples/Sufficit.Blazor.UI.Catalog` em `net10.0`.
- [x] Renderizar todos os componentes e suas variantes relevantes.
- [x] Cobrir estados: default, hover/focus, disabled, loading, empty, error,
      conteúdo longo, RTL, 200% de zoom, light e dark.
- [x] Criar projeto de testes de componentes com bUnit ou equivalente.
- [x] Adicionar testes de `SUIClassBuilder`, tema e serialização de tokens.
- [x] Adicionar testes de renderização semântica e `AdditionalAttributes`.
- [x] Adicionar Playwright para teclado, focus, popover, tooltip e viewport.
- [x] Integrar axe ou verificação WCAG equivalente no catálogo.
- [x] Capturar baseline visual desktop e mobile do catálogo antes das correções;
      os consumidores críticos permanecem no gate próprio da Fase 8.
- [x] Registrar os defeitos P1 atuais como testes que falham pelo motivo certo,
      sem congelar comportamento incorreto como snapshot aprovado.

**Gate da fase:** catálogo executa localmente e no CI; testes detectam pelo
menos dark theme inválido, tooltip isolado, Tab no select e dialog sem foco.

**Baseline automatizado (2026-08-14):** a solution e o catálogo compilam com
zero warning. Dos 11 testes bUnit iniciais, 5 passaram e 6 reproduziram os P1
esperados: CSS dark sem seletor, troca de tema congelada e labels/helpers sem
relação nos campos de texto, número, Select e Autocomplete. Esses testes não
serão relaxados; a Fase 2 deve torná-los verdes. Os 5 cenários Playwright
também reproduziram os defeitos esperados: Tab preso no Select, tooltip isolado
inerte, dialog sem foco e tema sem atualização. O axe registrou contraste,
labels, `aria-controls` do NavGroup e semântica do ListItem como violações
críticas/sérias para correção nas Fases 2 e 6.

**Gate concluído (2026-08-14):** catálogo local executado com Chromium; matriz
bUnit, Playwright e axe integrada ao CI. As referências visuais anteriores às
correções estão em `docs/baselines/catalog/catalog-light-desktop.png` e
`catalog-light-mobile.png`; o teste de viewport cobre 320 px e zoom simulado de
200%. Os testes P1 falham exclusivamente nos contratos que a Fase 2 corrige.

### Fase 2 — corrigir os defeitos P1 antes da refatoração estrutural

#### Tema e contraste

- [x] Formalizar `SUIThemeProvider` como provider único e global da aplicação,
      necessário porque portais são anexados ao `<body>`.
- [x] Emitir uma regra `:root { ... }` sintaticamente válida para light e dark,
      incluindo `color-scheme` dentro da regra.
- [x] Garantir que paletas dark customizadas não sejam substituídas pelo azul
      fallback de `[data-sui-theme="dark"]`.
- [x] Cobrir alteração/estabilidade do tema conforme o contrato escolhido;
      remover `IsFixed` se troca em runtime passar a ser suportada.
- [x] Auditar `PrimaryContrast` e contrastes de info/success/warning/error.
- [x] Preservar o âmbar como marca/acento, usando foreground ou tonalidade de
      superfície acessível quando branco não atingir 4,5:1.

#### Formulários e teclado

- [x] Gerar `id` estável para cada controle e associar `<label for>`.
- [x] Associar helper/error via `aria-describedby` e estado via
      `aria-invalid`/`aria-errormessage` quando aplicável.
- [x] Alterar `SUISelect` para cancelar apenas teclas tratadas, nunca Tab.
- [x] Adicionar ids das opções, `aria-activedescendant` e anúncio do item ativo.
- [x] Preservar foco no trigger ou adotar roving focus de forma consistente.
- [x] Implementar `SUIAutocomplete` com semântica combobox/listbox, setas,
      Enter, Escape, loading, vazio e anúncio de resultados.
- [x] Cancelar/dispor corretamente pesquisas e `CancellationTokenSource`.

#### Overlays e tooltip

- [x] Fazer `SUITooltip` inicializar seu próprio comportamento.
- [x] Corrigir os imports atuais para caminhos relativos ao `<base href>`.
- [x] No dialog, mover foco para a superfície/primeiro controle ao abrir.
- [x] Conter Tab/Shift+Tab dentro do dialog.
- [x] Fechar com Escape quando permitido e restaurar o foco do acionador.
- [x] Impedir que backdrop, substituição de request ou dispose deixem awaiters
      pendentes.

**Gate da fase:** todos os testes P1 ficam verdes; navegação completa funciona
somente por teclado; axe não encontra violação crítica/séria nas superfícies
afetadas; contrastes de texto normal atingem WCAG AA.

**Gate concluído (2026-08-14):** 17 testes bUnit e 10 cenários P1 Playwright
verdes. O axe não encontrou violações críticas/sérias nos temas light e dark;
contraste semântico, estrutura de listas, tooltip isolado, teclado completo de
Select/Autocomplete, responsividade a 320 px/zoom de 200%, tema reativo, trap,
Escape/restauração de foco e descarte/substituição de dialogs estão cobertos.

### Fase 3 — organizar componentes por família sem mudar comportamento

Mapa de movimentação congelado antes dos `git mv`:

| Família | Arquivos |
| --- | --- |
| `Actions` | `SUIButton`, `SUIIconButton`, `SUILoadingButton`, `SUILink` |
| `DataDisplay` | `SUIChip`, `SUIIcon`, `SUIIcons`, `SUIList`, `SUIListItem`, `SUITable`, `SUITableEmpty`, `SUITd`, `SUITh`, `SUIText`, `SUITimeline`, `SUITimelineItem` |
| `Feedback` | `SUIAlert`, `SUIEmptyState`, `SUIProgressLinear`, `SUISkeletonLoader`, `SUISkeletonType`, `SUISnackbarHost`, `SUIStatusBadge`, `SUIStatusBanner`, `SUIToast` |
| `Forms` | `ISUISelectRegistry`, `SUIAutocomplete`, `SUIChoiceCard`, `SUINumericField`, `SUISelect`, `SUISelectItem`, `SUISwitch`, `SUISwitchButton`, `SUITextField` |
| `Layout` | `SUIAppBar`, `SUICard`, `SUIContainer`, `SUIDivider`, `SUIDrawer`, `SUIGrid`, `SUIItem`, `SUILayout`, `SUIPageHeader`, `SUISpacer`, `SUIStack` |
| `Navigation` | `SUINavGroup` (`.razor`/`.razor.cs`), `SUINavLink`, `SUINavigationContext`, `SUITabPanel`, `SUITabs` |
| `Overlays` | `SUIConfirmDialog`, `SUIDialogHost` (`.razor`/`.razor.js`), `SUITooltip`, `SUITooltipPlacement` |

`SUIEnums.cs` permanece diretamente em `Components/` por ser o único contrato
compartilhado por todas as famílias; colocá-lo numa família criaria ownership
falso. Namespaces continuam `Sufficit.Blazor.UI.Components`.

- [x] Definir e registrar o mapa completo de arquivos para Actions,
      DataDisplay, Feedback, Forms, Layout, Navigation e Overlays.
- [x] Mover arquivos mantendo o namespace público atual.
- [x] Extrair `@code` para `.razor.cs` apenas quando houver lifecycle,
      interop, estado complexo ou tamanho que prejudique a leitura.
- [x] Manter componentes pequenos em um único `.razor`.
- [x] Atualizar paths em skills, docs, workflows e consumidores que apontem
      diretamente para arquivos-fonte.
- [x] Confirmar que a movimentação não altera nomes de tipos ou parâmetros.
- [x] Gerar/validar baseline de API pública antes e depois da movimentação.

**Gate da fase:** diff funcional zero no catálogo, API pública inalterada,
build/test/pack verdes e nenhum path antigo restante no repositório.

**Gate concluído (2026-08-14):** sete famílias físicas criadas; Select,
Autocomplete e DialogHost tiveram estado/lifecycle extraídos para code-behind,
enquanto componentes simples continuaram em um arquivo. As 579 assinaturas
públicas normalizadas antes/depois produziram o mesmo SHA-256
`49546badd6204cee2dbaa29a0359ace2815b377dc7a546f78ca5d24c1cdbf7a7`.
Build Release com warnings como erro, 17 testes bUnit, 10 testes Playwright e
pack `1.26.814.1448` ficaram verdes; referências antigas só permanecem em
registros históricos ou no contrato compartilhado `SUIEnums.cs` que não mudou.

### Fase 4 — decompor e colocalizar o JavaScript

- [x] Extrair Select, Tooltip e Rail para módulos independentes.
- [x] Importar cada módulo somente pelo componente que o usa.
- [x] Substituir listeners globais por listeners registrados no anchor/alvo
      sempre que possível.
- [x] Remover o `MutationObserver` global do `body` para rail flyouts.
- [x] Adicionar cleanup explícito para `ResizeObserver`, window/document
      listeners, timers e elementos flutuantes.
- [x] Tratar `JSDisconnectedException` no dispose de Blazor Server.
- [x] Testar múltiplas instâncias e remoção/recriação do componente.
- [x] Testar aplicação publicada em subdiretório com `<base href="/app/">`.
- [x] Remover `sufficit-ui.js` somente após não haver import restante.

**Gate da fase:** tooltip funciona sozinho; Select não instala código de rail ou
tooltip; nenhum observer/listener órfão permanece após dispose; base path não
quebra imports.

**Gate concluído (2026-08-14):** módulos independentes e fingerprintados para
Select, Tooltip, NavGroup e DialogHost; nenhum import do arquivo global antigo
permanece. Tooltip registra listeners por anchor, Select e rail removem
listeners/frames/`ResizeObserver`, DialogHost controla seu listener de foco por
referência, e o portal some ao descartar a última instância. Os 17 testes bUnit
e 12 cenários Playwright passaram, incluindo múltiplas instâncias,
remoção/recriação e execução real sob `/app/`. O pacote `1.26.814.1459` instalou
em consumidores mínimos net9/net10 com os quatro módulos colocalizados.

### Fase 5 — migrar para CSS híbrido e isolamento gradual

- [x] Criar `sui-foundations.css` com tokens, helpers e primitives
      compartilhados.
- [x] Criar `sui-portals.css` para superfícies geradas fora do scope Razor.
- [x] Manter `sufficit-ui.css` como entrypoint de compatibilidade durante a
      migração dos consumidores.
- [x] Documentar que o consumidor precisa carregar o stylesheet global e o
      `{Consumer}.styles.css` gerado pelo Blazor.
- [x] Migrar primeiro componentes autocontidos: ChoiceCard, StatusBadge,
      Timeline e EmptyState.
- [x] Migrar depois componentes com slots/filhos: PageHeader, Table e Tabs,
      verificando a necessidade de `::deep`.
- [x] Manter Button/Icon/Field/Card globais enquanto forem primitives
      compartilhados por múltiplos componentes.
- [x] Eliminar seletores duplicados do monólito a cada migração.
- [x] Verificar ordem de cascade e especificidade light/dark em cada lote.
- [x] Medir CSS original, gzip e Brotli antes/depois; não adicionar minificador
      sem ganho mensurável além da compressão já fornecida pelo .NET.

**Medição inicial da fase (2026-08-14):** o entrypoint monolítico tem 72.202
bytes brutos, 12.928 bytes em gzip nível 9 e 10.891 bytes em Brotli qualidade
11. A comparação final contará o entrypoint, as duas folhas globais importadas
e o bundle isolado gerado, evitando uma falsa redução baseada apenas na
transferência das regras entre arquivos.

**Medição final:** entrypoint 45.690 bytes, fundações 11.648, portais 2.850 e
bundle isolado 14.684; total de 74.872 bytes brutos, 15.258 em gzip e 12.904 em
Brotli. O acréscimo de 2.670 bytes brutos e cerca de 2 KB comprimidos vem dos
atributos de scope e da perda de dicionário entre quatro respostas. Não foi
adicionado minificador: o ganho arquitetural é ownership e carregamento
correto, e a compactação de transporte do .NET continua ativa.

**Gate da fase:** nenhum componente muda visual ou comportamento no catálogo e
nos consumidores; o bundle isolado é carregado; não há regra órfã ou duplicada;
o entrypoint legado continua compatível durante a janela definida.

**Gate concluído (2026-08-14):** build Release dos dois TFMs sem warnings, 17
testes bUnit e 13 cenários Playwright verdes. O teste de CSS híbrido verifica
as três folhas globais, o bundle fingerprintado, scopes e estilos efetivos; os
assets também responderam 200 na raiz e sob `/app/`. Não há interseção entre os
seletores próprios dos sete `.razor.css` e o monólito. Screenshots desktop e
mobile foram inspecionados em `artifacts/phase5-css/`. O pacote
`1.26.814.1513` contém fundações, portais e bundle isolado e instalou com
sucesso em consumidores mínimos net9/net10.

### Fase 6 — hardening responsivo, motion e semântica sistêmica

- [x] Garantir alvo mínimo de 44x44 px para ações em viewport touch, preservando
      densidade desktop onde apropriado.
- [x] Cobrir spinner, snackbar, dialog, progress, fields, tabs, drawer e switch
      em `prefers-reduced-motion`.
- [x] Trocar progress de animação de `width` para `transform: scaleX()` ou
      remover a transição se ela não comunicar mudança útil.
- [x] Remover a faixa lateral de 4 px do alert e preservar diferenciação por
      ícone, tom, borda uniforme e texto.
- [x] Completar tabs com ids, `aria-controls`, painel, roving tabindex e setas.
- [x] Tornar rows clicáveis semanticamente operáveis por teclado ou exigir que
      a ação viva em button/link dentro da célula.
- [x] Adicionar `scope`/headers e `colspan` correto aos estados vazios de tabela.
- [x] Corrigir Space em `SUIListItem` com `keydown` e `preventDefault`, ou
      renderizar button/link nativo.
- [x] Testar overflow horizontal, textos longos, RTL, 320 px e zoom de 200%.
- [x] Consolidar escala semântica de z-index para dropdown, sticky, backdrop,
      modal, toast e tooltip.

**Gate da fase:** WCAG AA no catálogo, todos os alvos touch críticos conformes,
nenhuma animação essencial sem alternativa reduced-motion e nenhum overflow em
320 px/200% zoom.

**Gate concluído (2026-08-14):** Tabs seguem o padrão tab/tabpanel com roving
tabindex, setas, Home/End, foco e prevenção do comportamento nativo por módulo
colocalizado; Table expõe headers de coluna, vazio com `colspan` integral e row
interativa por Enter/Space. Alert usa borda uniforme e Progress anima
`transform`. A escala `--sui-z-*` cobre dropdown até tooltip. Build dos dois
TFMs passou sem warnings, os 20 testes bUnit e 18 cenários Playwright ficaram
verdes, inclusive axe claro/escuro, targets touch, reduced-motion, RTL, 320 px,
zoom 200% e Tabs sob `/app/`. Screenshots foram inspecionados em
`artifacts/phase6-hardening/`. O pacote `1.26.814.1524`, incluindo o novo módulo
de Tabs, instalou em consumidores mínimos net9/net10.

### Fase 7 — endurecer API pública e documentação

- [x] Inventariar os 24 parâmetros públicos `object` e classificar quais ainda
      precisam da ponte de migração.
- [x] Introduzir parâmetros tipados com enums SUI e marcar pontes antigas como
      obsoletas com prazo de remoção em major version.
- [x] Avaliar adapters legados separados em vez de contaminar componentes-base.
- [x] Integrar fields com `EditContext`/`InputBase<T>` ou documentar claramente
      o contrato de validação suportado.
- [x] Tornar `AdditionalAttributes` consistente nos componentes de raiz
      interativa/semântica.
- [x] Resolver a contradição de `SUIText`: não expor aparência h1-h6 em um
      `<div>` sem oferecer tag semântica equivalente.
- [x] Adicionar validação de compatibilidade da API e package validation no CI.
- [x] Criar documentação por família e páginas próprias para Select, Tooltip,
      NavGroup, Dialog e ThemeProvider.
- [x] Atualizar a skill `sui-design` para refletir JavaScript, dark mode,
      catálogo e estrutura novos.

**Gate da fase:** API pública documentada, deprecações compilam com mensagem
acionável, package validation passa e não há divergência conhecida entre
README, skill, catálogo e código.

**Gate concluído (2026-08-14):** o inventário corrigido contém 24 parâmetros
`object`: 23 pontes visuais agora possuem alternativa tipada e depreciação
acionável para remoção em `v2.0.0`; apenas `SUISelectItem.Value` permanece como
ponte intencional do filho não genérico para `SUISelect<T>`. Alert e StatusBadge
também oferecem `ToneValue` tipado, preservando as propriedades textuais
legadas. `SUIText` ganhou tag semântica, os roots relevantes encaminham
atributos adicionais e o contrato controlado de validação dos fields está
documentado, com adapters `InputBase<T>` reservados para tipos separados. O
baseline público em `eng/PublicApiBaseline.txt` e o package validation protegem
compatibilidade; a documentação por família, as cinco páginas dedicadas e a
skill local foram sincronizadas. Build Release dos dois TFMs ficou sem warnings,
23 testes bUnit e 18 cenários Playwright passaram, e o pacote
`1.26.814.1539` passou pela instalação/compilação nos consumidores mínimos
net9/net10.

### Fase 8 — rollout controlado nos consumidores

- [x] Publicar pacote prerelease ou referência de commit reproduzível.
- [x] Escolher primeiro um consumidor SUI puro como canário.
- [x] Confirmar em cada consumidor o carregamento de `sufficit-ui.css` e do
      `{Consumer}.styles.css` gerado pelo CSS isolation.
- [x] Validar light/dark e a identidade de cada consumer: âmbar ou vermelho.
- [x] Validar navegação, Select, Tooltip, Dialog, tabelas, forms e shell.
- [x] Reexecutar screenshots desktop/mobile e comparar com baseline aprovado.
- [x] Construir/testar `sufficit-cloud-mobile`, Identity Management/Vault,
      `sufficit-blazor` e `sufficit-ai-genius` conforme aplicável.
- [x] Definir janela de compatibilidade para o entrypoint CSS antigo.
- [x] Documentar rollback para a versão anterior do pacote/referência.

**Gate da fase:** todos os consumidores em escopo compilam e passam os cenários
de browser definidos; nenhum consumer perde tema, CSS, JS ou interação; rollback
foi exercitado pelo menos no canário.

**Gate concluído (2026-08-14):** `sufficit-cloud-mobile`, escolhido como canário
SUI puro, migrou 88 usos legados, passou build estrito, 100 testes, smoke HTTP
dos três CSS/módulos e inspeção desktop/mobile; seu rollback foi exercitado em
cópia temporária contra `1.26.814.1358`. Identity Management/Vault/Public
passou build estrito e 712 testes; `sufficit-blazor` passou build normal e 280
testes sem CS0618 da SUI; AI Genius passou a solução estrita, build Android
normal e 383 testes. Todos os bundles dos hosts importam o bundle isolado da
SUI, nenhum host inclui o JavaScript global removido e a classe histórica
`.theme-dark` passou a ativar os tokens escuros. Evidências, limitações
preexistentes, compatibilidade e rollback estão em
`docs/CONSUMER-ROLLOUT.md`; screenshots em `artifacts/phase8-consumers/`.

### Fase 9 — gates finais e encerramento

**Andamento (2026-08-14):** a primeira repetição dos testes de browser expôs
duas execuções concorrentes e, depois de isolá-las, um problema real no host de
catálogo: em `Release`/`Production`, o manifesto de static web assets não era
carregado e o runtime procurava CSS/JS gerados sob `wwwroot`. O catálogo passou
a chamar `UseStaticWebAssets()` explicitamente. Em hosts reconstruídos, a suíte
raiz passou 19/20 com o cenário exclusivo de subpath ignorado, e a execução
integral sob `/app` passou 20/20.

- [x] Executar build Release com warnings como erro para `net9.0` e `net10.0`.
- [x] Executar todos os testes unitários, bUnit, browser e acessibilidade.
- [x] Executar `dotnet pack` e instalar o `.nupkg` em consumidores mínimos.
- [x] Inspecionar fingerprint, gzip/Brotli e paths dos static web assets.
- [x] Executar o detector Impeccable uma vez sobre os alvos alterados.
- [x] Reexecutar a auditoria técnica e atingir no mínimo 17/20, sem P0/P1.
- [x] Confirmar WCAG AA nos estados catalogados e cenários críticos.
- [x] Atualizar README, docs index, uso, arquitetura e notas de migração.
- [x] Arquivar este plano em `docs/activities/YYYYMMDDHHmm-completed-*.md` com
      evidências de build, testes, package e consumidores.

**Gate da fase:** release candidate instalável, consumidores validados, zero
P0/P1 e documentação alinhada ao código publicado.

**Gate concluído (2026-08-14):** release candidate instalável validado nos
dois TFMs, catálogo e consumidores verdes dentro das ressalvas preexistentes
registradas, auditoria 20/20 sem P0/P1 e documentação sincronizada.

**Auditoria final (2026-08-14):**

| Dimensão | Nota | Evidência final |
| --- | ---: | --- |
| Acessibilidade | 4/4 | axe sem violações serious/critical em light/dark; teclado, foco, ARIA e touch cobertos no browser |
| Performance | 4/4 | módulos ES por componente com descarte; CSS/JS fingerprintados e servidos em gzip/Brotli |
| Responsividade | 4/4 | 320 px, zoom 200%, RTL, conteúdo extremo e viewport móvel sem overflow |
| Theming | 4/4 | provider, paleta customizada, dark e alias `.theme-dark` cobertos por testes |
| Integridade | 4/4 | build multialvo estrito, 23 bUnit, 20 browser, pack e consumidores validados |
| **Total** | **20/20** | **zero P0/P1 remanescente no escopo da biblioteca** |

**Evidência do release candidate:** `Sufficit.Blazor.UI.1.26.814.1618.nupkg`,
SHA-256 `b5bea65eb1d75ddf3f76b7da10b652f0dbc741ffc242a721e353674765ddbf3a`.
Consumidores mínimos `net9.0` e `net10.0` compilaram com zero warning/erro. Na
saída publicada, `sufficit-ui.css` mede 47.030 bytes raw, 8.764 gzip e 7.370
Brotli; foundations 12.190/3.166/2.661; portals 2.872/982/800; o bundle isolado
fingerprinted mede 15.271/2.919/2.492. Todos responderam HTTP 200 em raiz e
`/app`, com negociação `Content-Encoding`. O detector Impeccable terminou sem
achados; a correção final também alinhou o caminho do projeto no CodeQL.

## Estratégia de commits e entregas

Manter commits pequenos, reversíveis e com um único tipo de mudança:

1. release/CI e documentação factual;
2. catálogo e infraestrutura de testes;
3. uma correção P1 com seus testes;
4. movimentação física sem alteração funcional;
5. um módulo JavaScript por commit;
6. uma família/lote de CSS isolado por commit;
7. hardening e API por componente/família;
8. rollout por consumidor.

Não misturar movimentação de arquivos com mudança visual/funcional no mesmo
commit. Isso preserva revisão, blame, bisect e rollback.

## Riscos e mitigação

| Risco | Impacto | Mitigação |
| --- | --- | --- |
| CSS isolation altera especificidade/ordem | regressão visual silenciosa | migrar em lotes, catálogo e screenshot por lote |
| `RenderFragment`/filhos não recebem scope esperado | slots sem estilo | manter global, usar `::deep` deliberado ou API pública |
| tooltip/portal fica fora da raiz scoped | tema/estilo ausente | manter tokens e portal CSS globais |
| consumidor não carrega `{Consumer}.styles.css` | componentes parcialmente sem CSS | gate explícito por consumidor e entrypoint compatível |
| path de `.razor.js` muda ao mover arquivos | interop falha em runtime | mover antes, usar path `./_content/...`, teste base path |
| bridge `object` é removida cedo | quebra binária/source | deprecação e remoção apenas em major version |
| correções de contraste alteram identidade | inconsistência de marca | preservar cor committed como acento e validar com consumer |
| dual target diverge | pacote funciona só num TFM | matriz CI, catálogo net10 e consumidor mínimo net9 |
| refatoração conflita com CSS em evolução | merge complexo | congelar lotes, não misturar estrutura e visual |

## Critérios globais de aceite

- Build e pack reproduzíveis em ambiente limpo.
- Pacote contém assets, README e ícone corretos para os dois TFMs.
- Tooltip, Select e NavGroup não dependem da presença um do outro.
- Nenhum controle impede Tab ou cria keyboard trap.
- Labels, helpers, errors, tabs, listboxes e dialogs têm relações ARIA válidas.
- Dialog move, contém e restaura foco.
- Texto normal atinge contraste mínimo de 4,5:1.
- Ações touch críticas atingem 44x44 px em viewport móvel.
- Reduced motion cobre toda animação/transição relevante.
- Light/dark respeitam a paleta do consumidor sem fallback silencioso.
- CSS isolado não exige README ou pasta por componente simples.
- JavaScript não deixa listener, observer, timer ou elemento órfão.
- Catálogo, testes, README, skill e código descrevem o mesmo contrato.
- Consumidores em escopo passam build e validação visual/funcional.
- Auditoria final sem P0/P1 e com score mínimo de 17/20.

## Validação e comandos de referência

Os comandos exatos podem evoluir com a criação dos projetos, mas o pipeline
final deve oferecer equivalentes automatizados para:

```bash
dotnet restore src/Sufficit.Blazor.UI.csproj
dotnet build src/Sufficit.Blazor.UI.csproj --configuration Release -warnaserror
dotnet test --configuration Release
dotnet pack src/Sufficit.Blazor.UI.csproj --configuration Release --output artifacts/packages
```

Além deles:

- testes Playwright desktop/mobile em light/dark;
- axe/WCAG no catálogo;
- inspeção do `.nupkg` e static web asset manifests;
- builds dos consumidores;
- detector Impeccable após todas as alterações de UI;
- nova auditoria técnica ao final.

## Referências técnicas

Evidências locais:

- [projeto e configuração do pacote](../../src/Sufficit.Blazor.UI.csproj);
- [workflow de publicação](../../.github/workflows/publish.yml);
- [configuração do Dependabot](../../.github/dependabot.yml);
- [stylesheet global atual](../../src/wwwroot/sufficit-ui.css);
- [módulo do Select](../../src/Components/Forms/SUISelect.razor.js);
- [módulo do rail](../../src/Components/Navigation/SUINavGroup.razor.js);
- [módulo do Tooltip](../../src/Components/Overlays/SUITooltip.razor.js);
- [provider de tema](../../src/Themes/SUIThemeProvider.razor);
- [Select](../../src/Components/Forms/SUISelect.razor);
- [Autocomplete](../../src/Components/Forms/SUIAutocomplete.razor);
- [Dialog host](../../src/Components/Overlays/SUIDialogHost.razor);
- [Tooltip](../../src/Components/Overlays/SUITooltip.razor);
- [convenções SUI](../../skills/sui-design/SKILL.md).

Referências do framework:

- [CSS isolation do Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-10.0);
- [static web assets, compressão e fingerprint](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0);
- [JavaScript colocalizado e módulos em RCLs](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-10.0).

## Encerramento

Este documento permanece um `PLAN-` enquanto qualquer checkbox estiver
pendente. Ao concluir, mover o conteúdo e as evidências para uma atividade
datada, registrar decisões duráveis em `ARCHITECTURE-*` e remover este plano da
lista de planos ativos.
