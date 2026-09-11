# Exibição de dados

Inclui `SUIText`, `SUIIcon`, `SUIChip`, `SUIStatusBadge`, `SUIList`, `SUITable`,
`SUIPagination`, `SUIStat` e `SUITimeline`.

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
