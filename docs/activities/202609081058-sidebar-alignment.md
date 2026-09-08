# Centralização vertical dos links do menu lateral

## Solicitação e causa

A captura do usuário mostrava o texto de “Temas e densidade” acima do centro do fundo. A base 418c305 usava `display:block` com altura mínima de 36px; a centralização flex existia apenas no media query móvel. Na página publicada, a caixa do texto do item ativo estava 2,109px acima do centro (8,391px de espaço superior e 12,609px inferior).

## Alteração

Em `samples/Sufficit.Blazor.UI.Showcase/wwwroot/showcase.css`, a regra geral dos links recebeu `display:flex; align-items:center`. A duplicação do media query foi removida; as alturas mínimas de 36px/44px, cores, conteúdo e URLs foram mantidas. Correção local da vitrine, sem mudança de API da biblioteca. Skill utilizada: sufficit-frontend 1.0.1. Worktree isolada; alteração preexistente do csproj principal preservada.

## Validação e entrega

Playwright existente usado para medir os 72 links em 1440px, 800px e 390px, nos temas claro e escuro. Conferidos estado ativo/inativo/hover, altura mínima, ausência de overflow da página e navegação por teclado. Desvio máximo da caixa de glifos após centralização: 0,734px, compatível com métricas da fonte. Capturas desktop claro e móvel escuro inspecionadas. Não foram adicionados testes permanentes para a pequena correção de CSS.

A primeira verificação usou CSS local interceptado sobre o app publicado. Após deploy, a mesma matriz foi repetida sem interceptação, com sucesso; CSS público comparado byte a byte com o fonte. Medidas e capturas temporárias em `/tmp/sidebar-alignment-published.json` e `/tmp/sidebar-alignment-published-*.png`.

- Código: [c9aa8fb](https://github.com/sufficit/sufficit-blazor-ui/commit/c9aa8fb122b4918fd9ab07b249182598c356662b).
- [Build aprovado](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34234788260).
- [Testes do site estático e Pages aprovados](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34234788424).
- [Página pública conferida](https://sufficit.github.io/sufficit-blazor-ui/?view=themes).

Sem deploy do Cloud ou publicação NuGet: o defeito era no menu da vitrine.
