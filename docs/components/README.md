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
[NavGroup](nav-group.md), [Dialog](dialog.md) e
[ThemeProvider](theme-provider.md).

Componentes com raiz semântica ou interativa encaminham atributos HTML não
reconhecidos para essa raiz. APIs antigas chamadas `UserAttributes` ou
`Attributes` continuam válidas por compatibilidade; componentes novos e os
componentes endurecidos nesta versão usam `AdditionalAttributes`.
