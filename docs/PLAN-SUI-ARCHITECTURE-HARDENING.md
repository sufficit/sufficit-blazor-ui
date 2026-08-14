# Plano — arquitetura e hardening da biblioteca SUI

**Status:** Planejado; nenhuma fase iniciada
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

- [ ] Corrigir os itens de pacote para `../icon.png` e `../README.md`.
- [ ] Corrigir restore/build/pack do workflow para
      `src/Sufficit.Blazor.UI.csproj`.
- [ ] Corrigir o diretório NuGet do Dependabot para `/src`.
- [ ] Adicionar job de `dotnet pack` em pull requests sem publicar o pacote.
- [ ] Inspecionar o `.nupkg`: DLLs dos dois TFMs, README, ícone, CSS e JS.
- [ ] Tornar explícito no CI que `net9.0` e `net10.0` são suportados.
- [ ] Revisar os `PackageReference` flutuantes `9.*`/`10.*` e documentar a
      política de atualização/reprodutibilidade.
- [ ] Corrigir no README: TFMs, número/estado dos consumidores, pacote,
      contrato de tema, uso de JavaScript e componentes existentes.

**Gate da fase:** build com warnings como erro e pack passam em ambiente limpo;
o pacote pode ser instalado em um consumidor mínimo para os dois TFMs.

### Fase 1 — criar catálogo e rede de segurança

- [ ] Criar `samples/Sufficit.Blazor.UI.Catalog` em `net10.0`.
- [ ] Renderizar todos os componentes e suas variantes relevantes.
- [ ] Cobrir estados: default, hover/focus, disabled, loading, empty, error,
      conteúdo longo, RTL, 200% de zoom, light e dark.
- [ ] Criar projeto de testes de componentes com bUnit ou equivalente.
- [ ] Adicionar testes de `SUIClassBuilder`, tema e serialização de tokens.
- [ ] Adicionar testes de renderização semântica e `AdditionalAttributes`.
- [ ] Adicionar Playwright para teclado, focus, popover, tooltip e viewport.
- [ ] Integrar axe ou verificação WCAG equivalente no catálogo.
- [ ] Capturar baseline visual desktop e mobile dos consumidores críticos.
- [ ] Registrar os defeitos P1 atuais como testes que falham pelo motivo certo,
      sem congelar comportamento incorreto como snapshot aprovado.

**Gate da fase:** catálogo executa localmente e no CI; testes detectam pelo
menos dark theme inválido, tooltip isolado, Tab no select e dialog sem foco.

### Fase 2 — corrigir os defeitos P1 antes da refatoração estrutural

#### Tema e contraste

- [ ] Formalizar `SUIThemeProvider` como provider único e global da aplicação,
      necessário porque portais são anexados ao `<body>`.
- [ ] Emitir uma regra `:root { ... }` sintaticamente válida para light e dark,
      incluindo `color-scheme` dentro da regra.
- [ ] Garantir que paletas dark customizadas não sejam substituídas pelo azul
      fallback de `[data-sui-theme="dark"]`.
- [ ] Cobrir alteração/estabilidade do tema conforme o contrato escolhido;
      remover `IsFixed` se troca em runtime passar a ser suportada.
- [ ] Auditar `PrimaryContrast` e contrastes de info/success/warning/error.
- [ ] Preservar o âmbar como marca/acento, usando foreground ou tonalidade de
      superfície acessível quando branco não atingir 4,5:1.

#### Formulários e teclado

- [ ] Gerar `id` estável para cada controle e associar `<label for>`.
- [ ] Associar helper/error via `aria-describedby` e estado via
      `aria-invalid`/`aria-errormessage` quando aplicável.
- [ ] Alterar `SUISelect` para cancelar apenas teclas tratadas, nunca Tab.
- [ ] Adicionar ids das opções, `aria-activedescendant` e anúncio do item ativo.
- [ ] Preservar foco no trigger ou adotar roving focus de forma consistente.
- [ ] Implementar `SUIAutocomplete` com semântica combobox/listbox, setas,
      Enter, Escape, loading, vazio e anúncio de resultados.
- [ ] Cancelar/dispor corretamente pesquisas e `CancellationTokenSource`.

#### Overlays e tooltip

- [ ] Fazer `SUITooltip` inicializar seu próprio comportamento.
- [ ] Corrigir os imports atuais para caminhos relativos ao `<base href>`.
- [ ] No dialog, mover foco para a superfície/primeiro controle ao abrir.
- [ ] Conter Tab/Shift+Tab dentro do dialog.
- [ ] Fechar com Escape quando permitido e restaurar o foco do acionador.
- [ ] Impedir que backdrop, substituição de request ou dispose deixem awaiters
      pendentes.

**Gate da fase:** todos os testes P1 ficam verdes; navegação completa funciona
somente por teclado; axe não encontra violação crítica/séria nas superfícies
afetadas; contrastes de texto normal atingem WCAG AA.

### Fase 3 — organizar componentes por família sem mudar comportamento

- [ ] Definir e registrar o mapa completo de arquivos para Actions,
      DataDisplay, Feedback, Forms, Layout, Navigation e Overlays.
- [ ] Mover arquivos mantendo o namespace público atual.
- [ ] Extrair `@code` para `.razor.cs` apenas quando houver lifecycle,
      interop, estado complexo ou tamanho que prejudique a leitura.
- [ ] Manter componentes pequenos em um único `.razor`.
- [ ] Atualizar paths em skills, docs, workflows e consumidores que apontem
      diretamente para arquivos-fonte.
- [ ] Confirmar que a movimentação não altera nomes de tipos ou parâmetros.
- [ ] Gerar/validar baseline de API pública antes e depois da movimentação.

**Gate da fase:** diff funcional zero no catálogo, API pública inalterada,
build/test/pack verdes e nenhum path antigo restante no repositório.

### Fase 4 — decompor e colocalizar o JavaScript

- [ ] Extrair Select, Tooltip e Rail para módulos independentes.
- [ ] Importar cada módulo somente pelo componente que o usa.
- [ ] Substituir listeners globais por listeners registrados no anchor/alvo
      sempre que possível.
- [ ] Remover o `MutationObserver` global do `body` para rail flyouts.
- [ ] Adicionar cleanup explícito para `ResizeObserver`, window/document
      listeners, timers e elementos flutuantes.
- [ ] Tratar `JSDisconnectedException` no dispose de Blazor Server.
- [ ] Testar múltiplas instâncias e remoção/recriação do componente.
- [ ] Testar aplicação publicada em subdiretório com `<base href="/app/">`.
- [ ] Remover `sufficit-ui.js` somente após não haver import restante.

**Gate da fase:** tooltip funciona sozinho; Select não instala código de rail ou
tooltip; nenhum observer/listener órfão permanece após dispose; base path não
quebra imports.

### Fase 5 — migrar para CSS híbrido e isolamento gradual

- [ ] Criar `sui-foundations.css` com tokens, helpers e primitives
      compartilhados.
- [ ] Criar `sui-portals.css` para superfícies geradas fora do scope Razor.
- [ ] Manter `sufficit-ui.css` como entrypoint de compatibilidade durante a
      migração dos consumidores.
- [ ] Documentar que o consumidor precisa carregar o stylesheet global e o
      `{Consumer}.styles.css` gerado pelo Blazor.
- [ ] Migrar primeiro componentes autocontidos: ChoiceCard, StatusBadge,
      Timeline e EmptyState.
- [ ] Migrar depois componentes com slots/filhos: PageHeader, Table e Tabs,
      verificando a necessidade de `::deep`.
- [ ] Manter Button/Icon/Field/Card globais enquanto forem primitives
      compartilhados por múltiplos componentes.
- [ ] Eliminar seletores duplicados do monólito a cada migração.
- [ ] Verificar ordem de cascade e especificidade light/dark em cada lote.
- [ ] Medir CSS original, gzip e Brotli antes/depois; não adicionar minificador
      sem ganho mensurável além da compressão já fornecida pelo .NET.

**Gate da fase:** nenhum componente muda visual ou comportamento no catálogo e
nos consumidores; o bundle isolado é carregado; não há regra órfã ou duplicada;
o entrypoint legado continua compatível durante a janela definida.

### Fase 6 — hardening responsivo, motion e semântica sistêmica

- [ ] Garantir alvo mínimo de 44x44 px para ações em viewport touch, preservando
      densidade desktop onde apropriado.
- [ ] Cobrir spinner, snackbar, dialog, progress, fields, tabs, drawer e switch
      em `prefers-reduced-motion`.
- [ ] Trocar progress de animação de `width` para `transform: scaleX()` ou
      remover a transição se ela não comunicar mudança útil.
- [ ] Remover a faixa lateral de 4 px do alert e preservar diferenciação por
      ícone, tom, borda uniforme e texto.
- [ ] Completar tabs com ids, `aria-controls`, painel, roving tabindex e setas.
- [ ] Tornar rows clicáveis semanticamente operáveis por teclado ou exigir que
      a ação viva em button/link dentro da célula.
- [ ] Adicionar `scope`/headers e `colspan` correto aos estados vazios de tabela.
- [ ] Corrigir Space em `SUIListItem` com `keydown` e `preventDefault`, ou
      renderizar button/link nativo.
- [ ] Testar overflow horizontal, textos longos, RTL, 320 px e zoom de 200%.
- [ ] Consolidar escala semântica de z-index para dropdown, sticky, backdrop,
      modal, toast e tooltip.

**Gate da fase:** WCAG AA no catálogo, todos os alvos touch críticos conformes,
nenhuma animação essencial sem alternativa reduced-motion e nenhum overflow em
320 px/200% zoom.

### Fase 7 — endurecer API pública e documentação

- [ ] Inventariar os 25 parâmetros públicos `object` e classificar quais ainda
      precisam da ponte de migração.
- [ ] Introduzir parâmetros tipados com enums SUI e marcar pontes antigas como
      obsoletas com prazo de remoção em major version.
- [ ] Avaliar adapters legados separados em vez de contaminar componentes-base.
- [ ] Integrar fields com `EditContext`/`InputBase<T>` ou documentar claramente
      o contrato de validação suportado.
- [ ] Tornar `AdditionalAttributes` consistente nos componentes de raiz
      interativa/semântica.
- [ ] Resolver a contradição de `SUIText`: não expor aparência h1-h6 em um
      `<div>` sem oferecer tag semântica equivalente.
- [ ] Adicionar validação de compatibilidade da API e package validation no CI.
- [ ] Criar documentação por família e páginas próprias para Select, Tooltip,
      NavGroup, Dialog e ThemeProvider.
- [ ] Atualizar a skill `sui-design` para refletir JavaScript, dark mode,
      catálogo e estrutura novos.

**Gate da fase:** API pública documentada, deprecações compilam com mensagem
acionável, package validation passa e não há divergência conhecida entre
README, skill, catálogo e código.

### Fase 8 — rollout controlado nos consumidores

- [ ] Publicar pacote prerelease ou referência de commit reproduzível.
- [ ] Escolher primeiro um consumidor SUI puro como canário.
- [ ] Confirmar em cada consumidor o carregamento de `sufficit-ui.css` e do
      `{Consumer}.styles.css` gerado pelo CSS isolation.
- [ ] Validar light/dark e a identidade de cada consumer: âmbar ou vermelho.
- [ ] Validar navegação, Select, Tooltip, Dialog, tabelas, forms e shell.
- [ ] Reexecutar screenshots desktop/mobile e comparar com baseline aprovado.
- [ ] Construir/testar `sufficit-cloud-mobile`, Identity Management/Vault,
      `sufficit-blazor` e `sufficit-ai-genius` conforme aplicável.
- [ ] Definir janela de compatibilidade para o entrypoint CSS antigo.
- [ ] Documentar rollback para a versão anterior do pacote/referência.

**Gate da fase:** todos os consumidores em escopo compilam e passam os cenários
de browser definidos; nenhum consumer perde tema, CSS, JS ou interação; rollback
foi exercitado pelo menos no canário.

### Fase 9 — gates finais e encerramento

- [ ] Executar build Release com warnings como erro para `net9.0` e `net10.0`.
- [ ] Executar todos os testes unitários, bUnit, browser e acessibilidade.
- [ ] Executar `dotnet pack` e instalar o `.nupkg` em consumidores mínimos.
- [ ] Inspecionar fingerprint, gzip/Brotli e paths dos static web assets.
- [ ] Executar o detector Impeccable uma vez sobre os alvos alterados.
- [ ] Reexecutar a auditoria técnica e atingir no mínimo 17/20, sem P0/P1.
- [ ] Confirmar WCAG AA nos estados catalogados e cenários críticos.
- [ ] Atualizar README, docs index, uso, arquitetura e notas de migração.
- [ ] Arquivar este plano em `docs/activities/YYYYMMDDHHmm-completed-*.md` com
      evidências de build, testes, package e consumidores.

**Gate da fase:** release candidate instalável, consumidores validados, zero
P0/P1 e documentação alinhada ao código publicado.

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

- [projeto e configuração do pacote](../src/Sufficit.Blazor.UI.csproj);
- [workflow de publicação](../.github/workflows/publish.yml);
- [configuração do Dependabot](../.github/dependabot.yml);
- [stylesheet global atual](../src/wwwroot/sufficit-ui.css);
- [módulo JavaScript global atual](../src/wwwroot/sufficit-ui.js);
- [provider de tema](../src/Themes/SUIThemeProvider.razor);
- [Select](../src/Components/SUISelect.razor);
- [Autocomplete](../src/Components/SUIAutocomplete.razor);
- [Dialog host](../src/Components/SUIDialogHost.razor);
- [Tooltip](../src/Components/SUITooltip.razor);
- [convenções SUI](../skills/sui-design/SKILL.md).

Referências do framework:

- [CSS isolation do Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation?view=aspnetcore-10.0);
- [static web assets, compressão e fingerprint](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0);
- [JavaScript colocalizado e módulos em RCLs](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-10.0).

## Encerramento

Este documento permanece um `PLAN-` enquanto qualquer checkbox estiver
pendente. Ao concluir, mover o conteúdo e as evidências para uma atividade
datada, registrar decisões duráveis em `ARCHITECTURE-*` e remover este plano da
lista de planos ativos.
