# Ajuda de SUITextField menor e mais próxima

O usuário pediu uma redução discreta em “Nome exibido aos usuários” e maior proximidade do textbox. O helper tinha fonte de 12 px e distância de 12 px: 4 px do gap do campo somados a 8 px de margem.

## Alteração e decisão

Base `143da31`, trabalho isolado em `.worktrees/field-helper-spacing`, com sufficit-frontend 1.0.1 e software-development. A correção pertence a `SUITextField.razor.css`: `margin-top:0` e fonte de 92% do token caption. No tema padrão, resulta em 11,04 px e distância de 4 px. Label continua com 12 px; cor, itálico, conteúdo, mensagens de erro e associação acessível permanecem.

Foi usado CSS isolation, já parte da arquitetura da biblioteca, para manter o ajuste no componente solicitado. Tentativas de alterar o helper compartilhado ultrapassaram os limites comprimidos e foram descartadas integralmente. Nenhum orçamento foi ampliado e nenhuma regra alheia foi removida para acomodar a alteração. O CSS global permanece idêntico: 54.675 bytes bruto, 10.049 gzip e 8.793 Brotli. O arquivo isolado fonte tem 172 bytes e integra o bundle de isolation existente, sem novo carregamento individual.

## Validação

- `npm run check:css`: artefato global e limites aprovados.
- `dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror`: sem avisos/erros.
- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release --no-build`: 587 aprovados.
- Publicação local da vitrine com `dotnet publish` e preparador Pages.
- Navegador em 1280 e 390 px, temas claro/escuro: fonte 12→11,04 px e distância 12→4 px. Label e cores mantidos, sem overflow. Associação `aria-describedby`, edição e limpeza do campo verificadas.
- Capturas locais e públicas inspecionadas; medidas públicas idênticas às locais. CSS isolation publicado comparado byte a byte com o artefato local.
- CI completo: componentes, pacote, três navegadores e Lighthouse aprovados. Primeira rodada: 587 componentes, Chromium 51, Firefox 46 e WebKit 46; os demais casos são específicos de host/engine. Lighthouse 1,0 nas quatro categorias.
- Referências visuais recapturadas no runner e comparadas em nova execução normal, sem atualização: todos os jobs aprovados.
- Pages concluído, incluindo os testes da vitrine.

## Entrega e estado local

- [Código 1551ea9](https://github.com/sufficit/sufficit-blazor-ui/commit/1551ea9).
- [Referências visuais 11c6ff0](https://github.com/sufficit/sufficit-blazor-ui/commit/11c6ff0).
- [CI/recaptura](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34248008254).
- [CI com comparação normal](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34248654205).
- [Deploy Pages](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34248008554).
- [SUITextField publicado](https://sufficit.github.io/sufficit-blazor-ui/?component=SUITextField).

Commit/push main e Pages seguem autorização vigente. A execução automática inicial foi substituída pela recaptura, conforme concorrência configurada, sem cancelamento manual do usuário. Não houve release NuGet nem deploy Cloud Mobile.

O checkout principal contém o commit local paralelo `7df3929` do switch, ainda ausente de origin/main, e modificação preexistente no csproj. Ambos foram preservados, sem rebase/merge dessa árvore nem publicação de trabalho alheio. O csproj mantém o hash inicial. A worktree desta tarefa contém a entrega e acompanha origin/main.
