# Controles coerentes do playground de IconButton

O usuário apontou “Mostrar ícone” no exemplo de um botão de ícone. A opção não ocultava nada: alternava entre Add e Edit enquanto o título acessível continuava “Criar item”.

## Correção

No playground, “Mostrar ícone” aparece apenas para SUIButton, onde controla o ícone opcional junto ao texto. SUIIconButton mantém Add e o título “Criar item”, tanto na prévia quanto no código copiável. A explicação de tamanho agora fala em conteúdo proporcional e serve aos dois componentes. Nenhuma API da biblioteca ou CSS foi alterada.

Base `9850473`, que já inclui a integração do switch. Trabalho em `.worktrees/icon-button-playground`, preservando o csproj modificado no checkout principal. Skills sufficit-frontend e software-development.

## Verificação

- `dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror`: sem avisos/erros.
- Artefato estático publicado localmente; inspeção visual em 1280 e 390 px.
- Verificação funcional local e pública: ausência da opção no IconButton; ícone Add e título no código; disabled, ativação por teclado e contador; toggle do botão comum oculta/mostra o ícone e atualiza o código; navegação entre os exemplos pela busca; ausência de overflow da página.
- O script inicial tentou usar a família de navegação oculta no mobile. Após o timeout, foi ajustado para usar a busca existente; nenhuma alteração adicional do produto foi necessária.
- CI completo aprovado: compilação, CSS, pacote, componentes, Chromium, Firefox, WebKit e Lighthouse.
- Pages concluído e comportamento confirmado no endereço público.
- Sem recaptura de baselines: a alteração é exclusiva do playground WASM; o catálogo Server permanece igual e suas comparações passaram.

## Entrega

- [Código 6c436f8](https://github.com/sufficit/sufficit-blazor-ui/commit/6c436f8).
- [CI](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34253110330).
- [Pages](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34253110430).
- [IconButton publicado](https://sufficit.github.io/sufficit-blazor-ui/?component=SUIIconButton).

Commit/push main e Pages conforme autorização vigente. Nenhuma release NuGet nem deploy Cloud Mobile nesta tarefa.
