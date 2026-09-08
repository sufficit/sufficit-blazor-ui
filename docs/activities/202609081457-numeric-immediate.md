# Campo numérico: atualização imediata e roda opcional

## Objetivo e base

Atender ao comportamento solicitado no controle Unidade de espaçamento do editor de temas. Base866d8b8; alteração preexistente em src/Sufficit.Blazor.UI.csproj preservada por worktree isolada. Aplicados os contratos da skill sufficit-frontend e o fluxo de software-development.

## Implementação

Commit `ebba2d7d84bac77c9b7a04c5bf7bc56e25ce666f` enviado à main.

- SUINumericField<T>: novo Immediate publica valores convertíveis por input, como SUITextField. Padrão false mantém change. O change não duplica a publicação no modo imediato; disabled e descarte bloqueiam eventos. Parsing invariant e integração com EditContext preservados.
- ChangeOnWheel opt-in: módulo colocalizado importado somente quando necessário; incremento nativo stepUp/stepDown para preservar Min/Max/Step e precisão decimal. Somente campo focado/habilitado/editável; Ctrl/Meta e Step=any não são capturados. Input/change seguem o contrato do componente. AbortController remove listener ao desativar/descartar, WeakMap evita duplicação.
- Editor de temas: ativa ambas as opções para tamanho, espaçamento e raio. Publica somente valores da faixa configurada para permitir digitar números de dois dígitos sem substituir o primeiro dígito.
- Exemplo do catálogo: opções habilitadas e valor atual visível. catalog.json regenerado, incluindo parâmetros públicos e código copiável. Documentação de formulários explica defaults e responsabilidades de ValueChanged.
- O módulo elevou o agregado Brotli de JS para8896B; teto documentado ajustado de8704B para9216B. CSS e limites de CSS não foram alterados. Sem dependência externa nova.

## Validação

- 595 testes de componentes aprovados, incluindo modo padrão/imediato, ausência de publicação duplicada, inválidos, disabled, decimal anulável e limpeza.
- Publish WASM Release e68 exemplos copiados compilados sem warnings/erros. generate-catalog.py --check aprovado.
- 3 testes novos de navegador por Chromium/Firefox/WebKit: digitação, ArrowUp, roda real, limites, edição de18, aplicação dos tokens antes de blur e persistência após reload. Teste de módulo cobre foco, passo decimal, máximo/mínimo, readonly, disabled, modificadores, any, configuração repetida e desconexão.
- Build CI34259581058 aprovado: componentes, pacote, CSS, browsers, acessibilidade, comparação visual e Lighthouse.
- Pages34259581061: build e deploy aprovados.
- URL pública: mesmos3 testes Chromium aprovados após deploy.

CI: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34259581058
Deploy: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34259581061
Teste: https://sufficit.github.io/sufficit-blazor-ui/?view=themes

## Limites e compatibilidade

Immediate não adiciona debounce nem salvamento remoto: o consumidor controla ValueChanged. ChangeOnWheel=false preserva o comportamento nativo do navegador sem instalar listener próprio. Entrada não numérica conserva o último valor e participa da validação; tipos anuláveis aceitam vazio. Pacote NuGet e aplicações consumidoras não foram atualizados nesta entrega; código e vitrine estão publicados.
