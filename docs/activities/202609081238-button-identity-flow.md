# Identidade e organização das ações

O usuário rejeitou o tom amarronzado da entrega anterior e apontou grupos de botões encostados na demonstração. Objetivo: recuperar o laranja, refinar o preenchido com gradiente discreto e organizar os grupos automaticamente, preservando as proporções e os contratos de alinhamento já corrigidos.

## Diagnóstico e implementação

Base `d71900b`. A imagem mostrava três raízes independentes no exemplo SUIButton: linha de variantes, pilha de alinhamento e botão de conteúdo longo. Havia gap dentro das pilhas, mas nenhum contêiner espaçando as raízes.

- Uma pilha externa `SUIStack Spacing="4"` organiza os grupos. A linha existente `SUIStack Row Wrap` continua distribuindo ações e quebrando linhas automaticamente. Não foram introduzidas margens individuais nem regras globais que mudariam grupos intencionalmente contíguos.
- A ação da paleta Sufficit na vitrine voltou a `#c2410c`, substituindo `#a34f2b`. Os temas consumidores e as outras paletas conservam suas cores.
- Filled tem gradiente vertical da cor de ação até uma sobreposição preta de 6,67%. Hover e active alteram o fundo mantendo o gradiente. Proporções de texto/ícone, alinhamento, foco e APIs permanecem.
- Testes de aparência verificam o contraste composto nos dois extremos do gradiente, nas três paletas e estados normal/hover/active. O teste de conteúdo mede distância real entre todas as ações consecutivas, incluindo quebra de linha e separação de grupos.
- Exemplo copiável, catálogo e documentação atualizados. A documentação de alinhamento foi corrigida para descrever o centro comum sem compensação artificial.
- Deduplicação de seletores e remoção de `height:auto` redundante mantiveram o CSS dentro dos limites existentes: 54.675 bytes bruto, 10.049 gzip, 8.793 Brotli. Variação +5/+5/+19 bytes sobre a base; nenhum orçamento ampliado.

## Verificação

- `dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror`: sem avisos/erros.
- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release --no-build`: 582 aprovados.
- `python3 scripts/generate-catalog.py --check` e `python3 scripts/check-catalog-examples.py`: catálogo consistente, 68 exemplos copiados compilados.
- `npm run check:css`: artefatos gerados e limites aprovados.
- Showcase local: 31 casos aprovados. Catálogo Server: oito casos de alinhamento, estados e contraste de ações aprovados.
- Capturas em 1280 e 390 px, temas claro/escuro: 12 px entre ações e 16 px entre grupos, sem overflow. Gradiente e resultado visual inspecionados.
- CI completo: 582 testes de componentes; Chromium 51, Firefox 46 e WebKit 46 aprovados. Os demais casos dependem do host/engine específico. Pacote e Lighthouse aprovados; primeira rodada com nota 1,0 nas quatro categorias.
- Referências visuais recapturadas no runner e comparadas novamente em execução normal, sem atualização: todos os jobs aprovados.
- Pages público: CSS idêntico byte a byte ao artefato local; medidas, cores e gradientes idênticos nos quatro cenários. Seis casos públicos de conteúdo/espaçamento e aparência/contraste/teclado aprovados com `SUI_SHOWCASE_URL=https://sufficit.github.io/sufficit-blazor-ui/ dotnet test tests/Sufficit.Blazor.UI.BrowserTests -c Release --no-build --filter 'FullyQualifiedName~ButtonVariantsKeepContrast|FullyQualifiedName~ButtonContentHonorsAlignment'`.

## Entrega

- [Código 2cf041a](https://github.com/sufficit/sufficit-blazor-ui/commit/2cf041a).
- [Baselines c155a88](https://github.com/sufficit/sufficit-blazor-ui/commit/c155a88).
- [CI e recaptura](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34244965304).
- [CI com comparação normal](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34245422285).
- [Deploy Pages](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34244966042).
- [Demonstração publicada](https://sufficit.github.io/sufficit-blazor-ui/?component=SUIButton).

Trabalho feito com as skills sufficit-frontend 1.0.1 e software-development, em `.worktrees/button-identity-flow`. A alteração preexistente no csproj do checkout principal foi preservada. Commit/push main e Pages seguem a autorização vigente. A execução automática inicial foi substituída pela recaptura conforme a concorrência do workflow, sem cancelamento manual do usuário. Nenhuma nova versão NuGet nem deploy Cloud Mobile nesta entrega.

### Estado local ao encerrar

A tentativa de fast-forward do checkout principal foi abortada pelo Git porque surgiram edições paralelas em `src/styles/sui-shared-switch.css`, sua cópia pública e `src/wwwroot/sufficit-ui.css`. Nenhum desses arquivos foi revertido, sobrescrito ou incluído nesta entrega. O csproj manteve o hash inicial. O checkout principal permanece em `d71900b` com essas edições locais; a worktree `button-identity-flow` contém a entrega e acompanha `origin/main`. A sincronização do checkout principal deve ocorrer quando a edição paralela do switch for integrada. Isso não bloqueia a publicação validada dos botões.
