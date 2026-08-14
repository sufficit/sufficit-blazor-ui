# Componentes SUI — catálogo e convenções

Carregue ao construir ou migrar UI. Namespace único `Sufficit.Blazor.UI.Components`.

## Índice
- Princípios
- Enums próprios
- Class builder
- Catálogo
- Especiais: `SUIChoiceCard`, `SUISelect`
- Layout e alinhamento de formulários
- Headings semânticos com `SUITextTag`
- Coexistência com MudBlazor
- Mapeamento de migração

## Princípios

- Prefixo **`SUI`**; classes **`.sui-*`**; tokens **`--sui-*`** em `:root`.
- **Não é Material Design:** flat, sombras suaves, raios generosos. Componentes nossos em Blazor puro (HTML+CSS), sem herança de libs de terceiros.
- Dark mode via `[data-sui-theme="dark"]` num elemento raiz.
- A biblioteca **não depende de MudBlazor** (nem pacote, nem vendorizado).

## Enums próprios

Use **estes**, não os do MudBlazor:

`SUIColor`, `SUIVariant` (Text/Outlined/Filled), `SUISize` (Small/Medium/Large), `SUIButtonType`, `SUIEdge`, `SUITypo` (h1–h6, subtitle1/2, body1/2, button, caption, overline), `SUITextTag`, `SUIAlign`, `SUIOrigin`, `SUITone`. Definidos em `src/Components/SUIEnums.cs`.

Parâmetros visuais novos usam o sufixo tipado `Value`: `ColorValue`,
`VariantValue`, `SizeValue`, `ButtonTypeValue`, `EdgeValue` e `ToneValue`.
Os nomes antigos sem sufixo são pontes obsoletas para valores externos e saem
na v2; não os use em código novo.

## Class builder

`src/Utilities/SUIClassBuilder.cs`. `.Slug(valor)` vira enum em kebab-class.

```csharp
SUIClassBuilder.Default("sui-btn")
    .AddClass("sui-btn--sm", () => size == SUISize.Small)
    .Slug(color)              // SUIColor.Primary -> "sui-btn--primary"
    .Build();
```

## Catálogo

Ações: `SUIButton`, `SUILoadingButton`, `SUIIconButton`, `SUILink`.
Forms: `SUITextField`, `SUISelect`/`SUISelectItem`, `SUINumericField`,
`SUIAutocomplete`, `SUIChoiceCard<T>`, `SUISwitch`, `SUISwitchButton`.
Tabelas: `SUITable`, `SUITh`, `SUITd`, `SUITableEmpty`.
Layout: `SUICard`, `SUIStack`, `SUIGrid` (+ `SUIItem`, 12 col responsivo `sui-item--{bp}-{n}`), `SUIContainer` (sm/md/lg/xl/xxl), `SUILayout`, `SUIDivider`, `SUISpacer`.
Shell: `SUIAppBar`, `SUIDrawer`, `SUINavGroup`, `SUINavLink` (collapse em CSS;
rail com módulo colocalizado para posicionar/descartar flyout), `SUIPageHeader`.
Navegação por abas: `SUITabs`, `SUITabPanel`.
Coleções: `SUITimeline`/`SUITimelineItem`, `SUIList`/`SUIListItem`/`SUIItem`, `SUIChip`.
Feedback/estado: `SUIStatusBadge`, `SUIEmptyState`, `SUISkeletonLoader` (+ `SUISkeletonType`), `SUIAlert`, `SUIToast`, `SUIProgressLinear`.
Texto/mídia: `SUIText`, `SUIIcon` (+ `SUIIcons.cs` paths SVG), `SUILink`.
Overlay/serviços: `SUIDialogHost` (+ `SUIConfirmDialog` + `ISUIDialogService`), `SUISnackbarHost` (+ `ISUISnackbar`).

## Especiais

### `SUIChoiceCard<TValue>`

Transforma uma opção inteira num alvo de toque acessível: rádio nativo + título + descrição + ícone contextual + estado selecionado, sem acoplar a domínio. Checkout usa o mesmo para PIX/cartão/boleto.

```razor
<SUIChoiceCard TValue="PaymentMethod" Value="PaymentMethod.Pix"
               SelectedValue="SelectedMethod" SelectedValueChanged="SelectMethod"
               Name="payment-method" Title="PIX"
               Description="Confirmação rápida, a qualquer hora" LeadingTone="SUITone.Success">
    <IconContent><SUIIcon Name="pix" Size="20" /></IconContent>
</SUIChoiceCard>
```

Slots opcionais: `IconContent`, `LeadingTone`, `LeadingClass`, `TrailingContent`, `Class`.

### `SUISelect` (listbox customizado)

Renderiza listbox próprio (mesmo tema/espaçamento em todo consumer). Acompanha o conteúdo, nunca menor que o campo, respeita a viewport. Teclado completo (↑↓ Home End Enter Space Esc). Use `<select>` nativo só quando postagem HTML ou menu do SO for deliberada.

```razor
<SUISelect T="string">...</SUISelect>                  <!-- largura pelo conteúdo -->
<SUISelect T="string" MenuWidth="22rem">...</SUISelect> <!-- fixa, reduz em telas pequenas -->
<SUISelect T="string" MenuMaxWidth="36rem">...</SUISelect> <!-- fluido até 36rem -->
```

## Headings semânticos

`SUIText` com `Tag=Auto` mapeia `SUITypo.h1`–`h6` para o heading nativo
correspondente. Título de página:

```razor
<SUIText Typo="SUITypo.h1">Visão geral</SUIText>
```

Quando o visual não corresponde ao nível do outline, declare ambos:
`<SUIText Typo="SUITypo.h5" Tag="SUITextTag.H1">`. `Div`, `Span` e `P`
também são explícitos. Se usar heading custom, resete a margem UA no shell.

## Assets, isolamento e interop

Carregue o entrypoint global da RCL e o `{Consumer}.styles.css`; o segundo traz
os `.razor.css` isolados. Tooltip usa portal global em `sui-portals.css`.
Select, Tooltip, NavGroup, Tabs e Dialog importam e descartam seus próprios
módulos ES; não existe `sufficit-ui.js` global e o host não inclui scripts.

Fields são controlados (`Value`/`ValueChanged`), não `InputBase<T>`: o caller
passa `Invalid`/`ErrorText` a partir do seu EditContext/validador.

## Layout e alinhamento de formulários

Componentes SUI controlam o ritmo interno de label, controle e helper; o
consumer controla a relação entre colunas. Em Grid/Flex horizontal:

- aplique `min-width: 0` e `align-self: start` aos wrappers `.sui-field`;
- neutralize margens de fluxo como `.field-group + .field-group` dentro da
  linha; essas regras servem ao empilhamento vertical, não às colunas;
- reserve altura igual para labels que podem ocupar duas linhas e remova essa
  reserva depois do breakpoint de empilhamento;
- compare também a altura dos controles — topo alinhado com input de 40px e
  select de 44px ainda é defeito;
- marque a linha com `data-sui-align-row` e execute
  `references/alignment-audit.md`. Um relatório sem comparações falha fechado.

Não corrija desalinhamento com `transform`, offsets negativos ou valores
específicos por campo; isso quebra com idioma, zoom e mensagens de validação.

## Coexistência com MudBlazor

Projetos podem ter MudBlazor residual durante migração. Regras:
- No **código novo**, use SUI + enums SUI. Os componentes de ação têm ponte temporária para valores visuais legados (migrar sem big-bang).
- **Não misture** classes `mud-*` em componentes SUI esperando styling SUI.
- O stylesheet SUI tem fallbacks defensivos `var(--mud-palette-primary, var(--sui-color-primary))` e um bloco legado `.mud-drawer.sufficit-rail` — são peso morto quando MudBlazor não está carregado, inofensivos.

## Mapeamento de migração (MudBlazor → SUI)

| SUI | Substitui (no `sufficit-blazor`) |
|---|---|
| `SUIButton` | `MudButtonEnchanted` |
| `SUINavGroup` | `MudNavGroupEnhanted` |
| `SUITableEmpty` | `TableNoRecords` |
| `SUILoadingButton` | `LoadingButton` |
| `SUINavLink` | `MudNavLinkEnchanted` |
| `SUIIconButton` | `MudIconButtonEnchanted` |
| `SUISkeletonLoader` | `SkeletonLoader` |
| `SUIEmptyState` | `EmptyState` |

`GenericTable` foi descartado (zero usos). `Layout`/`UI/FilterControl` do `sufficit-blazor` ainda não entraram na lib por estarem acoplados a domínio (telefonia, financeiro, mensageria, logs).
