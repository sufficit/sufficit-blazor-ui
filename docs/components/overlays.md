# Overlays

Tooltip e Dialog gerenciam seu próprio interop ES module. O host não inclui
scripts SUI. Tooltip é a única superfície visual criada no `body`; por isso sua
regra vive em `sui-portals.css`. Dialog permanece na árvore Razor e o host deve
existir uma única vez. Veja [Tooltip](tooltip.md) e [Dialog](dialog.md).

`SUIConfirmDialog` e `SUIDecisionDialog` são corpos de dialog: renderizam só
mensagem e ações e recebem a `SUIDialogReference` por cascata; título,
`role="dialog"` e foco vêm do `SUIDialogHost`. `ISUIDialogService.ConfirmAsync`
abre `SUIConfirmDialog` (Cancelar/Confirmar em `Error`) e devolve `bool`.
`SUIDecisionDialog` abre por `ShowAsync<SUIDecisionDialog>(título, parâmetros)`
com `Message` obrigatório, `Description`, `PrimaryText`/`SecondaryText`/
`CancelText` (padrões Sim/Não/Cancelar) e `PrimaryColor`/`SecondaryColor`;
`Result` completa com `true`, `false` ou `null` para cancelamento.


`SUIPopup` é uma superfície interativa não modal com estado `Open`/`OpenChanged`.
O consumidor fornece o trigger em `ChildContent` e o conteúdo em `Content`,
conservando os cascades de cliente/identidade. Diferentemente de uma dica de texto,
expõe `role=dialog` e um `Label` acessível. O trigger deve refletir o mesmo estado
em `aria-expanded`; `PanelId` permite ligar `aria-controls`. A top layer nativa evita
cortes pelo drawer, e o posicionamento acompanha resize/scroll e conteúdo variável.
Escape fecha o painel superior e devolve foco ao trigger; clique externo fecha sem
roubar foco do destino. A biblioteca não toma foco ao abrir, permitindo hover e
navegação por Tab sem transferências inesperadas. Não use esta superfície para substituir um
diálogo modal de confirmação. Consulte `SUIPopupExample.razor`.
