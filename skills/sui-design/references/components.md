# Componentes SUI — catálogo e convenções

Carregue ao construir ou migrar UI. Namespace único `Sufficit.Blazor.UI.Components`.

## Índice
- Princípios
- Enums próprios
- Class builder
- Catálogo
- Especiais: `SUIChoiceCard`, `SUISelect`
- Headings (não use `SUIText` como heading)
- Coexistência com MudBlazor
- Mapeamento de migração

## Princípios

- Prefixo **`SUI`**; classes **`.sui-*`**; tokens **`--sui-*`** em `:root`.
- **Não é Material Design:** flat, sombras suaves, raios generosos. Componentes nossos em Blazor puro (HTML+CSS), sem herança de libs de terceiros.
- Dark mode via `[data-sui-theme="dark"]` num elemento raiz.
- A biblioteca **não depende de MudBlazor** (nem pacote, nem vendorizado).

## Enums próprios

Use **estes**, não os do MudBlazor:

`SUIColor`, `SUIVariant` (Text/Outlined/Filled), `SUISize` (Small/Medium/Large), `SUIButtonType`, `SUIEdge`, `SUITypo` (h1–h6, subtitle1/2, body1/2, button, caption, overline), `SUIAlign`, `SUIOrigin`, `SUITone`. Definidos em `Components/SUIEnums.cs`.

## Class builder

`Utilities/SUIClassBuilder.cs`. `.Slug(valor)` vira enum em kebab-class.

```csharp
SUIClassBuilder.Default("sui-btn")
    .AddClass("sui-btn--sm", () => size == SUISize.Small)
    .Slug(color)              // SUIColor.Primary -> "sui-btn--primary"
    .Build();
```

## Catálogo

Ações: `SUIButton`, `SUILoadingButton`, `SUIIconButton`, `SUIChoiceCard<T>`.
Dados: `SUITextField`, `SUISelect`/`SUISelectItem`, `SUINumericField`, `SUIAutocomplete`, `SUICheckbox`, `SUISwitch`, `SUISwitchButton`.
Tabelas: `SUITable`, `SUITh`, `SUITd`, `SUITableEmpty`.
Layout: `SUICard`, `SUIStack`, `SUIGrid` (+ `SUIItem`, 12 col responsivo `sui-item--{bp}-{n}`), `SUIContainer` (sm/md/lg/xl/xxl), `SUILayout`, `SUIDivider`, `SUISpacer`.
Shell: `SUIAppBar`, `SUIDrawer`, `SUINavGroup`, `SUINavLink` (nav em árvore só em CSS — `grid-template-rows: 0fr↔1fr` para o collapse; rail com flyout; sem JS/portal), `SUIPageHeader` (eyebrow + título + descrição + actions).
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

## Headings: NÃO use `SUIText` como heading

`SUIText` renderiza `<div class="sui-text sui-text--{Typo}">` — **não é semântico**. Título de página deve ser `<h1>` real, senão o `FocusOnNavigate Selector="h1"` não tem alvo e o leitor de tela não navega por cabeçalho. Padrão:

```razor
<h1 class="sui-text sui-text--h5 mb-4">Visão geral</h1>
```

(Visual idêntico ao `SUIText Typo="SUITypo.h5"`, mas semântico.) Subseções: `<h2 class="sui-text sui-text--h6">`. Se usar heading custom, resete margem UA: `.cloud-mobile-shell h1, h2, h3 { margin: 0; }`.

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
