# Análise do Sufficit.Blazor.UI — 07/09/2026

Escopo: estrutura do repositório, implementação dos componentes e serviços principais, composição visual, temas, testes e viabilidade de uma vitrine publicada no GitHub Pages. A referência a publicação pelo GitHub foi interpretada como GitHub Pages com GitHub Actions. Este trabalho é uma análise: não implementa nem publica a nova vitrine.

## Parecer

A base permite evoluir sem reescrever a biblioteca. Manteria a Razor Class Library independente, a organização por famílias e o pacote único. A principal mudança estrutural seria separar exemplos reutilizáveis dos hosts de execução, permitindo uma vitrine WebAssembly estática e preservando testes específicos de Blazor Server.

A implementação ainda não está pronta para ser apresentada como catálogo completo: há componentes sem demonstração, uma composição de layout inadequada e lacunas reais em temas e formulários. Os testes atuais passam, mas isso não significa que todos os estados dos componentes estejam corretos.

Avaliação orientativa, limitada aos caminhos inspecionados; não é certificação de acessibilidade nem benchmark de produção:

| Dimensão | Nota / 4 | Evidência principal |
| --- | --- | --- |
| Acessibilidade | 2 | Cenários axe existentes passam; snackbar de sucesso escuro tem contraste 1,74:1 |
| Desempenho | 3 | Budgets passam; buscas não cancelam trabalho remoto e tabelas não têm caminho para grandes volumes |
| Responsividade/composição | 2 | Testes mobile passam; demonstração de shell comprime elementos no desktop |
| Temas | 2 | Contrato e tokens existem; faltam pares de contraste e isolamento entre instâncias |
| Consistência de implementação | 2 | Famílias e API controladas, mas parâmetros ignorados e fontes CSS divergentes |
| Total | 11/20 | Base utilizável com trabalho significativo antes da vitrine pública |

O detector mecânico da skill impeccable retornou `[]`. Sua ausência de achados não comprova a correção de componentes Razor; os problemas abaixo foram confirmados por leitura de código ou inspeção no navegador.

## O que preservar

- Biblioteca em `src`, catálogo em `samples` e testes separados: a divisão principal já é boa.
- Famílias Actions, Forms, Layout, Navigation, DataDisplay, Feedback e Overlays: não há justificativa para reorganizar tudo.
- Ausência de dependência de framework visual externo.
- Contrato `ISUITheme`, com palette, typography e layout, configurável por DI e por parâmetro.
- CSS global minificado e estilos isolados, com módulos JavaScript colocalizados e carregados sob demanda.
- Testes de API pública, lifecycle, teclado, acessibilidade, subpath, tamanhos de assets e regressão visual.
- Política documentada de versionamento e migração dos consumidores.

## Achados prioritários

### 1. [P1 — requisito da vitrine] O catálogo atual depende de servidor

Local: `samples/Sufficit.Blazor.UI.Catalog/Program.cs:14`, `Components/App.razor:20` e `.github/workflows/build.yml`.

O host registra `AddInteractiveServerComponents` e renderiza as rotas em `InteractiveServer`. O workflow compila, testa e publica o pacote, mas não contém implantação de site no Pages. Copiar seu `wwwroot` não produz uma vitrine interativa funcional.

Recomendação: criar host **Blazor WebAssembly standalone**, com dados demonstrativos locais, reutilizando os componentes reais. O resultado publicado é estático, embora os componentes executem interativamente no navegador. Configurar base path, assets `_content`, CSS isolado e comportamento ao atualizar URLs internas. Para um projeto hospedado na URL padrão do Pages, o prefixo será `/sufficit-blazor-ui/`; domínio próprio pode exigir `/`.

GitHub Actions deve compilar o CSS, publicar o host, validar a saída estática sob esse prefixo e entregar o artefato ao Pages. Não basta adicionar `InteractiveWebAssembly` ao host atual e copiar arquivos, porque a dependência do servidor precisa ser removida da aplicação publicada.

Referências: [Blazor WebAssembly no GitHub Pages](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages?view=aspnetcore-10.0), [workflows de publicação do Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages).

### 2. [P1 — temas/acessibilidade] Snackbar escuro com contraste insuficiente

Local: `src/styles/sui-components.css:1316` e `samples/Sufficit.Blazor.UI.Catalog/Components/Pages/Home.razor:324`.

Reprodução: abrir o catálogo, alternar para tema escuro e clicar em “Mostrar snackbar”. O estilo computado é texto `rgb(255,255,255)` sobre fundo `rgb(74,222,128)`. Contraste calculado: **1,74:1**. O branco é fixado em `--_sn-fg`; o fundo vem do token Success, que fica claro no tema escuro.

Impacto: a mensagem fica difícil de ler. Recomendação: separar cores de texto/acento de superfícies preenchidas e adicionar pares como Success/OnSuccess, Error/OnError e Warning/OnWarning, ou pares específicos de superfície/contraste para feedback. Aplicar também aos demais componentes que combinam fundo semântico com texto branco fixo. Adicionar testes com o snackbar aberto em cada tema.

O mínimo para texto normal é 4,5:1: [WCAG 2.2, critério 1.4.3](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html).

### 3. [P1 — formulários] Integração incompleta com EditContext

Local: `src/Components/Forms/SUITextField.razor:83` e `:182`; `SUIDateField.razor.cs:19`; demais campos de `Forms`.

`ValueExpression` é exposto em alguns campos, mas não utilizado. A família não contém integração com `EditContext`, `FieldIdentifier` ou `NotifyFieldChanged`. Atualizar `ValueChanged` altera o modelo, mas não fornece por si só o acompanhamento de campo modificado e a validação durante a edição. A validação do modelo no envio pode continuar funcionando; o problema é a integração por campo.

Além disso, o `catch` de `SUITextField.SetValue` engloba tanto a conversão quanto o callback do consumidor. Uma exceção do callback também é silenciosamente descartada.

Recomendação: contrato compartilhado de campos, com suporte opcional a EditContext, mensagens de validação e parsing apropriado ao tipo/cultura. Preservar uso fora de EditForm. Restringir a captura de exceções a falhas de conversão esperadas e não ocultar erros de callbacks. Testar alteração de campo, envio, limpeza, erro de parsing e callbacks com falha.

Referência: [modelo EditForm/EditContext](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/binding?view=aspnetcore-10.0).

### 4. [P1 — composição] A demonstração de layout ensina uma composição inadequada

Local: `samples/Sufficit.Blazor.UI.Catalog/Components/Pages/Home.razor:241`; `src/styles/sui-components.css:890` e `:998`.

O exemplo coloca AppBar, Drawer, Grid, Divider e Stack como filhos diretos do SUILayout. No desktop, `.sui-layout` muda para `flex-direction: row`, distribuindo todos lado a lado. Em viewport de 1440px, a inspeção mediu barra com 105px, grid com 205px e stack com 49px; o bloco ocupou 1032px de altura. O resultado visual comprime os textos e deixa uma grande área vazia.

Recomendação: representar explicitamente barra superior, linha de navegação/conteúdo e área principal flexível. A classe `.sui-layout__main` existe, mas o exemplo não a utiliza. Documentar a composição correta e avaliar slots `Header`, `Navigation` e `MainContent`, ou um componente de conteúdo principal, com compatibilidade preservada. A miniatura de demonstração precisa de dimensões próprias, em vez de herdar automaticamente altura de tela inteira.

### 5. [P2 — disposição] SUIStack ignora Style e não define corretamente Spacing=0

Local: `src/Components/Layout/SUIStack.razor:3`, `:31`, `:45`.

O parâmetro `Style` existe, mas o HTML recebe apenas `GapStyle`. `Spacing=0` gera `var(--sui-space-0)`, token ausente das fundações e do provider. Em condições padrão, o gap inválido pode parecer zero; o contrato ainda está incorreto e pode deixar outra declaração de gap prevalecer.

Recomendação: combinar os estilos com precedência documentada e gerar explicitamente `gap:0` para zero. Isso também beneficia `SUISection`, que oferece `ContentSpacing=0`.

### 6. [P2 — temas] Defaults e tokens derivados não têm fonte única

Local: `src/Themes/SUIPalette.cs:11`, `:31`; `src/styles/sui-foundations.css:16`; `src/Themes/SUIThemeProvider.razor`.

O default C# é azul (`#2563eb`), enquanto o CSS sem provider usa âmbar/laranja (`#ee6321`). `PrimarySoft` contém azul literal: trocar apenas `Primary` não atualiza a tonalidade suave. Há também tokens presentes apenas no CSS, como `--sui-fs-field` e níveis de z-index, sem propriedades correspondentes no contrato tipado.

Recomendação: definir uma origem canônica de tokens, alinhar os defaults e derivar PrimarySoft de `var(--sui-color-primary)`. Decidir quais tokens são customizáveis por contrato e quais permanecem internos. Fornecer presets claros/escuros completos; `IsDark` não constrói uma palette escura automaticamente.

### 7. [P2 — requisito futuro] Temas simultâneos não são isolados

Local: `src/Themes/SUIThemeProvider.razor:128` e `docs/components/theme-provider.md`.

Cada instância publica CSS em `:root,.sui-root[data-sui-theme]`. Portanto, duas instâncias afetam as mesmas raízes; a última regra vence. A documentação exige uma instância por aplicação, então isso é uma limitação documentada, não uma regressão.

Impacto: impede comparação confiável lado a lado entre marcas ou light/dark usando múltiplos providers no mesmo documento.

Recomendação: para a primeira vitrine, um seletor global de tema; para comparação simultânea, iframe por exemplo ou provider com identificador de escopo e hosts de portal associados. Não implementar comparação simplesmente aninhando providers atuais. Preferência claro/escuro/sistema e persistência podem viver inicialmente no host da vitrine.

### 8. [P2 — agilidade de uso] O catálogo não cobre toda a biblioteca nem explica a API

Local: `samples/Sufficit.Blazor.UI.Catalog/Components/Pages/Home.razor` e `docs/components`.

Foram encontrados 68 arquivos de componentes Razor em `src`, excluindo `_Imports` e incluindo o provider. Sete componentes não possuem demonstração identificada nos samples: SUICheckbox, SUIDecisionDialog, SUIPagination, SUIPendingChangesBar, SUIProgressSteps, SUISection e SUIStat. SUIConfirmDialog é demonstrado indiretamente via serviço, por isso não entrou nessa lista.

A Home concentra 340 linhas e mistura showcase, estados de teste e dados específicos. A navegação atual é por âncoras de famílias, sem documentação individual junto do exemplo.

Recomendação: registro de exemplos e uma página por componente, com nome real, finalidade, parâmetros, valores padrão, eventos, código copiável e estados. Usar esse registro também para menu, busca e verificação de cobertura. Separar as fixtures de estresse das páginas de apresentação. Documentar composições recorrentes: formulário com ações, lista com filtros, estado vazio e diálogo de confirmação.

### 9. [P2 — manutenção] CSS concentrado e cópias antigas divergentes

Local: `src/styles/sui-components.css`, `src/wwwroot/styles` e `scripts/build-css.mjs`.

O arquivo principal tem 1420 linhas. Quatro cópias em `wwwroot/styles` diferem das fontes de mesmo nome: card, components, entry e foundations. O script gera somente `wwwroot/sufficit-ui.css`; não sincroniza essas cópias. O entrypoint público atual está consistente e passou na validação.

Recomendação: manter uma única árvore de autoria, dividir regras por responsabilidade e gerar o bundle público. Verificar consumidores antes de remover os assets antigos; se ainda forem necessários para compatibilidade, gerá-los automaticamente e validar a equivalência no CI.

O bundle global tem 53.625 bytes, 9.922 gzip e 8.659 Brotli. O limite gzip é 10.000 bytes: restam apenas 78 bytes. Isso exige cuidado com novas regras, mas não comprova lentidão. Tamanho do bundle global não é o total transferido pelo aplicativo: o CSS isolado e o runtime também contam.

### 10. [P2 — desempenho/lifecycle] Buscas e notificações precisam de cancelamento mais completo

Local: `src/Components/Forms/SUIAutocomplete.razor.cs:16`, `:191`; `src/Components/Feedback/SUISnackbarHost.razor:25`.

Autocomplete já tem debounce e descarta resultados antigos, porém SearchFunc não recebe CancellationToken: uma requisição já iniciada continua consumindo recursos. Snackbar usa `async void`, Task.Run por entrada e atrasos sem cancelamento no Dispose; altera a coleção fora da ação enviada ao dispatcher.

Recomendação: adicionar overload de busca cancelável preservando a API existente. No host de notificações, executar mutações no dispatcher, cancelar tarefas ao descartar o componente e limitar/organizar mensagens concorrentes. Não atribuir ganhos numéricos antes de medir com uso real.

### 11. [P2 — desempenho] Tabela precisa de um caminho documentado para volumes maiores

Local: `src/Components/DataDisplay/SUITable.razor:17` e `:28`.

A tabela faz `Any()` e depois enumera os itens, não usa chave de linha e renderiza todos os registros recebidos. Isso é adequado para páginas pequenas, mas não oferece estabilidade de identidade nem limitação própria de volume para listas grandes.

Recomendação: parâmetro de chave estável para linhas; orientar paginação externa usando SUIPagination; considerar provedor paginado ou virtualização opcional quando houver demanda medida. Preservar a tabela simples e não transformar todos os usos num grid complexo.

### 12. [P2 — validação] Cobertura forte, porém incompleta em estados e contratos

Local: `tests/Sufficit.Blazor.UI.BrowserTests/AccessibilityBrowserTests.cs:196`, `CatalogBrowserTests.Baselines.cs` e testes de componentes.

Axe bloqueia apenas violações serious/critical nos estados exercitados. As baselines comparam a aparência atual, inclusive a composição problemática de layout. Testes de nomes, linhas e strings não substituem testes de comportamento.

Recomendação: matriz componente × estado × tema, incluindo snackbar visível, menus e diálogos abertos, teclado e erros. Verificar isolamento de tema quando implementado, Style/Spacing, integração com EditForm e saída estática real em subpath. Na vitrine WASM, reavaliar budgets de runtime: os limites do catálogo Server não representam seu custo inicial de download.

## Arquitetura sugerida para a vitrine

Manter `src` como biblioteca. Extrair os exemplos para uma RCL pequena de demonstração, consumida por dois hosts: catálogo Server para contratos desse modo e vitrine WebAssembly para publicação estática. Não duplicar exemplos entre hosts. Essa separação é útil aqui porque a biblioteca atende diferentes modos de execução; não é recomendação para dividir cada família em um pacote.

A vitrine teria:

1. Introdução com instalação, configuração de DI e carregamento dos dois tipos de CSS.
2. Navegação por famílias e busca por componente.
3. Página individual com preview real, variantes, estados, API e código copiável.
4. Área de temas com presets, claro/escuro/sistema, densidade e edição dos tokens permitidos; exportação da configuração C#.
5. Composições prontas para usos recorrentes dos consumidores.
6. Versão do pacote documentado e guia de migração dos parâmetros legados.

Uma página HTML pré-gerada seria alternativa para documentação puramente visual e menor carga inicial; a recomendação de WASM decorre da necessidade de experimentar eventos e comportamento dos componentes reais. Se indexação e primeira carga se tornarem requisitos centrais, avaliar documentação pré-gerada com demos interativas carregadas sob demanda.

## Ordem de execução recomendada

1. Corrigir snackbar/contraste, integração de campos e composição do shell; incluir testes dos casos encontrados.
2. Corrigir Style/Spacing, alinhar tokens e definir presets e contrato de tema.
3. Extrair exemplos e completar cobertura; construir navegação e páginas por componente.
4. Criar host WASM, medir carga inicial e validar o artefato estático na raiz e em subpath.
5. Adicionar publicação pelo GitHub Actions/Pages e executar revisão final da vitrine.
6. Evoluir cancelamento, tabelas e escopos simultâneos conforme uso real.

Para passes específicos de interface: `$impeccable harden` nos estados e formulários, `$impeccable colorize` nos pares de contraste, `$impeccable layout` na composição do catálogo, `$impeccable adapt` na navegação responsiva e `$impeccable polish` ao concluir a implementação.

## Validação realizada

- `dotnet build Sufficit.Blazor.UI.slnx --configuration Release -warnaserror`: passou, sem erros nem avisos.
- Testes de componentes em Release: **379 passaram**.
- Testes Chromium em Release: **45 passaram**, incluindo as baselines, axe, performance e subpath.
- `npm run check:css`: passou, bundle atualizado e budgets respeitados.
- Inspeção de screenshots desktop/mobile, leitura do CSS computado do shell e reprodução do snackbar escuro.
- Detector impeccable: nenhuma ocorrência reportada.

Firefox, WebKit, Lighthouse e validação de pacote não foram executados nesta análise. Não houve medição de rede móvel, load test, teste de consumidores externos ou publicação. Os testes existentes exercitam o host Server; compatibilidade do futuro catálogo WASM ainda deve ser validada.

Artefatos temporários e logs: `/tmp/sufficit-blazor-ui-review`. O código da biblioteca, testes, workflow e catálogo permaneceu sem alteração durante a análise.
