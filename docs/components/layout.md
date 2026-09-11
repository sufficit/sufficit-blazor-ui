# Layout

Inclui Layout, AppBar, Drawer, Container, Grid/Item, Stack, Spacer, Divider,
Card e PageHeader. PageHeader sempre renderiza `header` com `h1`; `LeadingContent`
é preferível a eyebrow para navegação interativa.

`SUISection` combina CardHeader e CardContent para seções operacionais tituladas,
com slots opcionais de ícone e ações. Use-o quando a mesma seção precisa aparecer
em mais de uma superfície; conteúdo pontual pode continuar compondo `SUICard`
diretamente. Seus blocos de conteúdo seguem, por padrão, um fluxo vertical com
`ContentSpacing="4"` (16px), evitando que alertas, campos e grupos fiquem colados.
Use outro valor da escala de 0 a 6 quando a densidade da seção exigir; reserve
`ContentSpacing="0"` para conteúdo que controla integralmente o próprio layout.

Drawer permanente participa do fluxo em desktop. Drawer temporário usa eixo
lógico, funciona em RTL e ocupa o nível `--sui-z-drawer`. Tokens de spacing,
raio e elevação vêm do tema; não replique números de z-index no consumer.

`SUICardActions` é o rodapé de ações de `SUICard`, depois de `SUICardContent`:
um `div` com a classe `sui-card__actions`, sem semântica própria, que recebe
os botões. `AlignEnd` alinha as ações ao fim da linha em vez do início;
atributos não mapeados vão para a raiz.
