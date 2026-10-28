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

Com `Actions`, o cabeçalho vira uma linha de decisão: título e descrição à
esquerda ocupam a linha, as ações ficam no fim dela e quebram para baixo do
texto quando ele ficaria com menos de 16rem (mobile), em vez de espremê-lo.

`ToneValue` marca uma seção de consequência — a "zona de perigo":
`SUITone.Danger` para perda (excluir, revogar) e `SUITone.Warning` para
interrupção. Só a borda e o título recebem a cor do tom; a descrição continua
neutra e a seção mantém tamanho e posição (cor distingue, não cria hierarquia;
ver `docs/DESIGN-VISUAL-HIERARCHY.md`). Sem tom, ou com Neutral/Info/Success, a
marcação é a mesma de antes. Sai por classes (`sui-section--tone`,
`sui-section--danger|warning`), seguro sob CSP estrita.

```razor
<SUISection Title="Excluir tema" Description="Não pode ser desfeito." ToneValue="SUITone.Danger">
    <Actions><SUIButton ColorValue="SUIColor.Error" OnClick="AskDelete">Excluir</SUIButton></Actions>
    <ChildContent>@* formulário de confirmação, quando aberto *@</ChildContent>
</SUISection>
```

Seções seguidas nunca se tocam: fora de um container, `.sui-section +
.sui-section` aplica o `Gap` (16px por padrão); dentro de `SUIStack`/`SUIGrid`
o gap do container já separa.

`SUIStack` aplica `Spacing` (0–6, padrão 3) por classes CSS da biblioteca. Isso
mantém o intervalo entre filhos em aplicações com CSP sem `unsafe-inline`;
`Style` fica disponível somente para personalizações explícitas do consumidor.
Para separar um cabeçalho de sua explicação, componha ambos como filhos diretos
do stack e remova margens próprias que possam duplicar o intervalo.

Drawer permanente participa do fluxo em desktop. Drawer temporário usa eixo
lógico, funciona em RTL e ocupa o nível `--sui-z-drawer`. Tokens de spacing,
raio e elevação vêm do tema; não replique números de z-index no consumer.

`SUICardActions` é o rodapé de ações de `SUICard`, depois de `SUICardContent`:
um `div` com a classe `sui-card__actions`, sem semântica própria, que recebe
os botões. `AlignEnd` alinha as ações ao fim da linha em vez do início;
atributos não mapeados vão para a raiz.
