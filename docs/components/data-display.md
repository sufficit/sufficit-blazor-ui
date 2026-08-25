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
