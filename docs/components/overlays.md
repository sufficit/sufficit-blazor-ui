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
