# Catálogo estático, temas e contratos de componentes

Status: concluído localmente. Publicação remota não executada.

## Entrega

Vitrine standalone Blazor WebAssembly com 68 componentes executáveis, índice
por família, busca, documentação de parâmetros e exemplos Razor copiáveis.
Host Server preservado; ambos usam a RCL compartilhada de demonstrações.
Composições mostram EditForm, paginação, alterações pendentes e diálogos.
A navegação por query string funciona em hospedagem estática sem rewrite.

Tema claro/escuro/sistema acompanha o dispositivo, persiste no navegador,
oferece três paletas e duas densidades e exporta C#. Presets públicos
SUITheme.Light/Dark e contrastes semânticos completam a API de temas.
O provider permanece global e único, inclusive para portais no body.

A biblioteca recebeu integração de campos com EditContext por composição,
notificações de alteração e erros de parsing; cancelamento remoto do autocomplete;
RowKey e materialização da tabela; descarte/dispatcher do snackbar; correção de
Style e espaçamento zero em Stack e de limites numéricos não fornecidos.
CSS compartilhado foi dividido e suas cópias de compatibilidade agora são
produzidas pelo build. Nenhum limite de tamanho do bundle foi aumentado.

Workflow Pages entregue com gates e artefato; deploy limitado a main, sem
publicação por PR. O runbook explica configuração Pages, execução, manutenção
e recuperação. README, changelog e contratos de formulário/tema atualizados.

## Validação executada

- Build Release dos 7 projetos, warnings como erros: zero erros/avisos.
- Testes de componentes/contratos: 548 passaram. O número inclui teorias por
  arquivo; o crescimento também reflete a divisão do CSS, além das regressões novas.
- Chromium Server: 45 passaram, incluindo axe, comportamento, budgets e
  referências visuais. Smoke de subpath executado sob /app.
- Chromium do WASM publicado: 7 passaram sob /sufficit-blazor-ui/ e os mesmos
  7 passaram sob /. Cobrem 68 páginas, reload, busca, restauração de campos,
  validação, paginação, temas e acessibilidade desktop claro/mobile escuro.
- Todos os 68 textos Razor copiáveis compilaram em RCL temporária independente.
- Metadados gerados atualizados; parâmetros comparados por reflection com a
  API de componentes. Baseline público regenerado: nenhuma assinatura removida.
- CSS verificado: 53.993 bytes bruto, 9.890 gzip e 8.664 Brotli, dentro dos
  tetos existentes de 54.000 / 10.000 / 8.800 bytes.
- dotnet publish Release produziu artefato WASM com trimming; preparador
  validou assets e base. YAML do workflow validado por parser.
- Pacote local Sufficit.Blazor.UI.2.0.0-review.catalog.nupkg inspecionado e
  instalado em RCL e Blazor Web App temporárias net10.0; app executada em raiz
  e /app, verificando markup, CSS global/isolation e módulos JavaScript.
- Revisão visual independente: pass após corrigir Cancelar no formulário
  inicial e reduzir os snippets aos membros realmente usados. Capturas de
  desktop, mobile, componente e temas em .impeccable/review/ (artefatos locais).
- Quatro baselines Server em docs/baselines/catalog atualizados intencionalmente
  para a composição correta da área Layout e verificados novamente.

## Limites e decisões

GitHub Pages e NuGet não foram publicados remotamente. Ativar Pages com origem
GitHub Actions e enviar o código para main habilita o fluxo entregue. O pacote
local tem versão somente de revisão; release segue o processo de tags existente.

Esta rodada executou Chromium; Firefox/WebKit e Lighthouse não foram
reexecutados. O detector visual auxiliar operou em modo regex degradado,
portanto a conclusão visual se apoia nas capturas, revisão e testes de navegador.

Campos com ValueExpression dentro de EditForm agora notificam e leem mensagens;
consumidores com validação manual devem revisar duplicação de mensagens. As
assinaturas e herança foram preservadas. Temas simultâneos isolados, virtualização
de tabelas e migração dos consumidores externos não fazem parte desta entrega.
Os planos anteriores de rollout permanecem como trabalho separado.

## Artefatos e operação

- Site publicado local: artifacts/showcase/wwwroot (base /sufficit-blazor-ui/).
- Pacote: artifacts/catalog-implementation/packages/.
- Operação: ../RUNBOOK-SHOWCASE-PAGES.md.
- Sistema visual: ../../DESIGN.md; produto: ../../PRODUCT.md.

O plano temporário PLAN-catalogo-estatico.md foi encerrado após validação e
substituído por este registro. Nenhum commit ou push realizado nesta atividade.
