# Layout

Inclui Layout, AppBar, Drawer, Container, Grid/Item, Stack, Spacer, Divider,
Card e PageHeader. PageHeader sempre renderiza `header` com `h1`; `LeadingContent`
é preferível a eyebrow para navegação interativa.

`SUISection` combina CardHeader e CardContent para seções operacionais tituladas,
com slots opcionais de ícone e ações. Use-o quando a mesma seção precisa aparecer
em mais de uma superfície; conteúdo pontual pode continuar compondo `SUICard`
diretamente.

Drawer permanente participa do fluxo em desktop. Drawer temporário usa eixo
lógico, funciona em RTL e ocupa o nível `--sui-z-drawer`. Tokens de spacing,
raio e elevação vêm do tema; não replique números de z-index no consumer.
