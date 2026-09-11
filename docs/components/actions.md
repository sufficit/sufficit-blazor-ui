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
um SVG manualmente ao lado do conteúdo. O botão alinha texto e quadro do ícone pelo centro, sem deslocamentos artificiais;
isso vale também durante o estado de carregamento.

## Alinhamento e conteúdo longo

`SUIButton` mantém o conteúdo centralizado por padrão. Use a propriedade pública
`Style="justify-content:flex-start;text-align:start"` para ações de menu, ou
`justify-content:space-between` quando o conteúdo tem texto e um indicador lateral.
O wrapper do botão herda esse alinhamento, cresce com `FullWidth` e permite quebra
de palavras longas. Não é necessário selecionar `.sui-btn__label` no consumidor.

## Proporção e preenchimento

`SizeValue="SUISize.Large"` aumenta altura/padding e conserva `FsButton`, como Medium. O ícone automático de SUIButton usa 1,25em; `IconSizeValue` continua permitindo tamanho explícito independente do botão. SUIIconButton limita o desenho automático a 1,5em, sem mudar a escala do componente SUIIcon isolado. SUILoadingButton segue a mesma tipografia de botão.

Filled mantém preenchimento opaco com gradiente discreto: peso 500, raio 1,25×`--sui-radius`, sombra 1 e borda sutil derivada da ação e de seu contraste. Soft mantém o fundo tonal alternativo. O tom de ação da vitrine é configurado por `PrimaryAction`; a biblioteca respeita as cores de cada consumidor.

### Organização automática

Agrupe ações em `<SUIStack Row Wrap>`: o contêiner distribui o espaço entre botões e quebra linhas conforme a largura disponível. Para vários grupos, use uma pilha externa `<SUIStack Spacing="4">`; assim o espaço entre grupos permanece independente do número de botões ou linhas. O exemplo copiável de SUIButton demonstra essa composição, inclusive com rótulo longo. Evite margens individuais nos botões.

Filled usa gradiente vertical discreto derivado da cor de ação do tema; hover e active mantêm esse acabamento. A paleta Sufficit da vitrine recupera o laranja `#c2410c`.

## Copiar para a área de transferência

`SUICopyToClipboard` coloca `Text` (obrigatório) na área de transferência e
avisa pelo `ISUISnackbar`: sucesso com `SuccessMessage` ou "`Title` copiado
para a área de transferência."; falha com `ErrorMessage` (padrão "Erro ao
copiar") seguido da primeira linha do motivo. Sem `ChildContent`, renderiza um
`SUIIconButton` pequeno cujo nome acessível é `AriaLabel` ou "Copiar `Title`".
Com `ChildContent`, o wrapper é um `span` semanticamente transparente, sem
role nem tabindex; o clique sobe do filho, e o acesso por teclado é
responsabilidade dele — envolva um botão real. `FullWidth` estica o wrapper.
O componente importa seu próprio módulo ES sob demanda: usa
`navigator.clipboard` em contexto seguro e cai para `textarea` +
`execCommand("copy")` fora dele. O aviso só aparece se houver um
`SUISnackbarHost` na raiz.
