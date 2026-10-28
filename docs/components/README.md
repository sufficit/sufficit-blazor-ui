# Componentes SUI

Este diretório documenta a API pública da `Sufficit.Blazor.UI` por família e os
componentes cujo lifecycle ou integração exigem um contrato próprio.

## Instalação comum

Todo host carrega `_content/Sufficit.Blazor.UI/sufficit-ui.css` e o seu bundle
`{Assembly}.styles.css`, registra `AddSufficitUI()` e mantém um único
`SUIThemeProvider` na raiz interativa. Módulos `.razor.js` são importados pelos
componentes e nunca devem ser adicionados manualmente.

## Famílias

- [Ações](actions.md)
- [Formulários](forms.md)
- [Exibição de dados](data-display.md)
- [Feedback](feedback.md)
- [Layout](layout.md)
- [Navegação](navigation.md)
- [Overlays](overlays.md)
- [Tema](theming.md)
- [Migração da API tipada](api-migration.md)

Páginas detalhadas: [Select](select.md), [Tooltip](tooltip.md),
[NavGroup](nav-group.md), [Dialog](dialog.md), [Stat](stat.md) e
[ThemeProvider](theme-provider.md).

## CSP estrita

Hosts com `style-src-attr 'none'` (identity, por exemplo) fazem o navegador
ignorar todo atributo `style`, pré-renderizado ou definido depois pelo Blazor.
A SUI não escreve valores próprios nesse atributo:

- valores limitados viram classes: `SUISection.Gap` (`sui-section--gap-0…6`) e `ToneValue` (`sui-section--danger|warning`),
  `SUIText.Color` (`sui-color-*`), colunas/gap de `SUIFormGrid`, label de
  `SUILoadingButton`, grupo oculto de `SUIFilterTree` (atributo `hidden`);
- valores livres vão em `data-sui-style="--sui-…:valor"` e o inicializador JS
  do pacote (`Sufficit.Blazor.UI.lib.module.js`, carregado automaticamente
  pelo Blazor) os aplica com `element.style.setProperty`, que a política não
  bloqueia: `SUIFormGrid.ColumnTemplate`/`MinColumnWidth`, `SUIDrawer.Width`,
  `SUIProgressLinear.Value`, `SUIStat.BackgroundImage`,
  `SUISkeletonLoader.Size`, `SUIPopover.MaxWidth`, `SUINavGroup.MaxHeight`;
  `SUISelect.MenuWidth`/`MenuMaxWidth` usam o módulo do próprio componente.
  Antes do JS rodar, o CSS usa o padrão (a grade com `ColumnTemplate` divide
  suas trilhas em colunas iguais; a barra de progresso começa vazia).

`data-sui-style` só aceita custom properties `--sui-*`; os valores passam por
uma allow-list de caracteres e funções (`SUIStyleBuilder.IsSafeValue`) no
servidor e são revalidados no navegador, então o atributo não serve para
injetar outras propriedades. Consumidores podem usá-lo para tokens
(`data-sui-style="--sui-color-info:#0b6"`), montando o valor com
`SUIStyleBuilder`.

O parâmetro `Style` (e `SUITable.RowStyleFunc`) é repasse do consumidor: sob
CSP estrita ele **é ignorado**. Use `Class` com uma regra no CSS da aplicação.
O teste `StrictCspBrowserTests` carrega o catálogo com essa política e falha
se algum componente voltar a depender de `style`.

Componentes com raiz semântica ou interativa encaminham atributos HTML não
reconhecidos para essa raiz. APIs antigas chamadas `UserAttributes` ou
`Attributes` continuam válidas por compatibilidade; componentes novos e os
componentes endurecidos nesta versão usam `AdditionalAttributes`.
