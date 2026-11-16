# Exibição de dados

Inclui `SUIAudioPlayer`, `SUIText`, `SUIIcon`, `SUIChip`, `SUIStatusBadge`, `SUIList`, `SUITable`,
`SUIDataTable`, `SUIPagination`, `SUIStat` e `SUITimeline`.

`SUIText` mapeia `SUITypo.h1`–`h6` para headings nativos quando `Tag=Auto`;
`Tag` permite escolher `Div`, `Span`, `P` ou heading explicitamente. Aparência
e semântica deixam de ser contratos contraditórios.

`SUITable.ColumnCount` controla o `colspan` do vazio. `SUITh` usa `scope="col"`
por padrão. Row só recebe `role="button"` e tabindex quando `OnRowClick` existe,
com Enter/Space; para ações distintas por linha, prefira buttons/links dentro
das células. `RowClassFunc` e `RowStyleFunc` permitem estados semânticos sem
acoplar a tabela ao domínio, e `Loading` expõe `aria-busy` com progresso.
`SUIPagination` mantém faixa, página e controles acessíveis; `SUIStat` atende
indicadores operacionais compactos. Chip e TimelineItem usam os parâmetros enum
`*Value`.

`SUIAvatar` renderiza `img` quando `Src` existe, com `alt` vindo de `Alt`; se a
imagem falhar ao carregar, cai para o `ChildContent` (iniciais) e volta a
tentar quando `Src` muda. Sem imagem, o `div` recebe `role="img"` e
`aria-label` quando `Alt` está preenchido; sem `Alt`, fica `aria-hidden`, em
vez de anunciar uma letra sem sentido. `SizeValue` aceita Small/Medium/Large,
`ColorValue` aplica a classe de cor e `Rounded` troca o círculo por quadrado
arredondado.

`SUITd` é a célula de `RowTemplate` em `SUITable`: um `td` cujo `DataLabel`
vira `data-label`, o rótulo exibido quando a tabela colapsa para cards.
`SUITableSortLabel<T>` vai dentro de `SUITh` e renderiza um `button`, porque
ordenar é ação e precisa de teclado e leitor de tela. É controlado por
`SortDirection`/`SortDirectionChanged` e cicla None → Ascending → Descending →
None; `AllowUnsorted="false"` volta a Ascending no terceiro clique. O estado é
anunciado em texto oculto, em inglês por padrão — localize com
`StateDescriptionFactory`. `aria-sort` pertence ao `th`, e o consumidor o deriva
de `SortDirection`. `SortBy` e `InitialDirection` são apenas metadados de
migração do MudTableSortLabel; a ordenação continua no callback.

`SUITableEmpty` preenche o `NoRecordsContent` de `SUITable`: ícone
(`Icon`, padrão `SUIIcons.SearchOff`), `Title` com `TitleTypo` (padrão h6) e
`Description`, quebrada em linhas por `\n` ou, na falta dele, na primeira
sentença. `ChildContent` recebe ações como "limpar filtros". O `colspan` vem de
`SUITable.ColumnCount`.

## Paginação localizável e por links

`SUIPagination` aceita `PreviousLabel`, `NextLabel`, `PageSizeLabel`, `PageLabel` (`(página, total) => …`),
`RangeFormatter` (`(primeiro, último, total) => …`), `EmptyText` e `Disabled` (ex.: durante o carregamento).
Com `PageHref` (`índice => $"?page={índice + 1}"`) anterior/próxima viram links, para páginas roteadas no
servidor; a ponta desabilitada não tem `href`.

`SUITableSortLabel` traduz o sufixo de estado por `AscendingLabel`, `DescendingLabel` e `UnsortedLabel`
(`StateDescriptionFactory` continua tendo precedência).

Em `StackOnMobile`, um `SUITd` com `DataLabel` envolve o conteúdo em `div.sui-td__value`, que é
`display: contents` no desktop e o segundo item da grade rótulo/valor no celular: vários filhos na célula
ficam juntos na coluna do valor.

## Reprodução de áudio

`SUIAudioPlayer` recebe `Source` (URL autorizada ou data URI), `Label` acessível,
`DownloadName`, `DownloadText` e `ShowDownload`. Usa controles nativos, carrega
somente metadados inicialmente e não inicia reprodução automática. O layout
adapta a mídia à largura disponível sem estilo inline na tela consumidora.
`ErrorText` informa falha do player; mudar `Source` limpa o erro e reinicia a
mídia. Sem fonte, mostra `EmptyText`. O componente não busca credenciais nem
confirma publicação/entrega: cabe ao consumidor resolver a mídia autorizada.


## Consulta assíncrona com SUIDataTable

`SUIDataTable<TItem>` compõe `SUITable` e `SUIPagination`. `LoadPage` recebe uma
`SUITableQuery` imutável (página zero-based, tamanho, chave e direção de ordenação)
e um cancellation token. Retorne `SUITablePage<TItem>` com os registros autorizados
e `TotalItems`. A aplicação continua responsável por autorização, filtros e ordenação;
no modo local, forneça `Items` já ordenados. Use `ShowPagination` para habilitar o
paginador e os callbacks controlados para persistir página/tamanho na URL.

`QueryKey` identifica o contexto e os filtros. Alterá-la cancela a consulta anterior,
limpa os registros do escopo anterior e inicia outra consulta. Renders sem alteração
da consulta não repetem o carregamento. Uma resposta atrasada não substitui resultados
mais novos. O carregamento inicial usa skeleton; vazio só aparece após conclusão.
`ColumnCount` informa o colspan real do placeholder e das mensagens.

`ReloadAsync()` atualiza a consulta atual. Falha nessa atualização preserva a lista
anterior com aviso persistente de dados possivelmente desatualizados e botão de nova
tentativa. Uma falha inicial não anuncia sucesso vazio. `OnLoadError` permite tratar
autenticação/logs no consumidor; detalhes de exceções não são exibidos pela biblioteca.
`OnRowClick`, `RowKey` e `RowAriaLabelFunc` preservam a interação de grid do SUITable.
Ao trocar de modo remoto para local ou descartar o componente, a operação pendente é
cancelada. Consulte `SUIDataTableExample.razor` na vitrine para o contrato executável.
