# Ações

`SUIButton`, `SUIIconButton`, `SUILoadingButton` e `SUILink` preservam HTML
nativo: button quando não há `Href`, anchor quando há navegação. Estado
desabilitado não dispara callback; links externos recebem `noopener noreferrer`
por padrão.

Use `ColorValue`, `VariantValue`, `SizeValue`, `ButtonTypeValue` e, no botão de
ícone, `EdgeValue`. Os parâmetros antigos sem sufixo aceitam `object` apenas
como ponte obsoleta até v2. Targets críticos passam a 44 px em viewport touch,
sem alterar a densidade desktop.
