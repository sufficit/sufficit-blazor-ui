# Espaçamento do aviso de versão na vitrine

Na base `bbf5c0b`, o bloco “Documentação de desenvolvimento” não tinha estilo próprio e ficava a **0px** do botão “Experimentar SUIButton” na página Instalação. A captura enviada foi confirmada no site público.

A correção adiciona `margin-block-start: var(--sui-space-6)` a `.catalog-version` em `samples/Sufficit.Blazor.UI.Showcase/wwwroot/showcase.css`. O token define 32px e atende o bloco compartilhado das páginas, preservando conteúdo, navegação e demais estilos. Trabalho orientado por sufficit-frontend, em worktree isolada; a alteração preexistente de versionamento no csproj principal foi preservada.

## Verificação

Playwright conferiu cinco rotas (Instalação, SUIButton, Temas, Composições e Visão geral), em 1440/390px e temas claro/escuro: 20 combinações. Medição inicial com CSS local interceptado sobre o app público, seguida da mesma matriz sem interceptação após o deploy. Resultado: 32px de separação, exceto Composições que mantém os 48px preexistentes do exemplo anterior; sem overflow da página. O botão Experimentar continua navegando para SUIButton. Captura desktop revisada visualmente; CSS público comparado byte a byte com o fonte. Dados temporários: `/tmp/catalog-version-published.json` e `/tmp/catalog-version-published-*.png`.

Não foram adicionados testes permanentes para esta regra pequena de CSS. Os gates existentes de build e publicação passaram.

- [Código 2cab36f](https://github.com/sufficit/sufficit-blazor-ui/commit/2cab36fad135c9a37fc298d55e4cfd83e514a56e).
- [Build aprovado](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34236193986).
- [Testes estáticos e Pages aprovados](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34236193923).
- [Página pública conferida](https://sufficit.github.io/sufficit-blazor-ui/?view=setup).

Correção restrita à vitrine; sem deploy do Cloud ou publicação NuGet.
