# Ações

`SUIButton`, `SUIIconButton`, `SUILoadingButton` e `SUILink` preservam HTML
nativo: button quando não há `Href`, anchor quando há navegação. Estado
desabilitado não dispara callback; links externos recebem `noopener noreferrer`
por padrão.

Use `ColorValue`, `VariantValue`, `SizeValue`, `ButtonTypeValue` e, no botão de
ícone, `EdgeValue`. Os parâmetros antigos sem sufixo aceitam `object` apenas
como ponte obsoleta até v2. Targets críticos passam a 44 px em viewport touch,
sem alterar a densidade desktop.

Para ações com ícone e texto, use `StartIcon` ou `EndIcon` em vez de posicionar
um SVG manualmente ao lado do conteúdo. O botão mantém o texto no centro
geométrico e aplica somente ao ícone a compensação óptica necessária; isso vale
também durante o estado de carregamento.

## Alinhamento e conteúdo longo

`SUIButton` mantém o conteúdo centralizado por padrão. Use a propriedade pública
`Style="justify-content:flex-start;text-align:start"` para ações de menu, ou
`justify-content:space-between` quando o conteúdo tem texto e um indicador lateral.
O wrapper do botão herda esse alinhamento, cresce com `FullWidth` e permite quebra
de palavras longas. Não é necessário selecionar `.sui-btn__label` no consumidor.
