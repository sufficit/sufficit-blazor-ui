# Sufficit.Blazor.UI

Razor Class Library dos componentes **Sufficit User Interface (SUI)**. A
biblioteca usa Blazor puro, HTML, CSS e módulos JavaScript ES; não depende de
MudBlazor nem contém código-fonte vendorizado de outra biblioteca visual.

## Compatibilidade e distribuição

- Target frameworks: `net9.0` e `net10.0`.
- Package ID: `Sufficit.Blazor.UI`.
- Distribuição: pacote NuGet e `ProjectReference` local.
- Namespace dos componentes: `Sufficit.Blazor.UI.Components`.
- Namespace de temas: `Sufficit.Blazor.UI.Themes`.

O CI compila ambos os frameworks com warnings tratados como erros, gera o
`.nupkg`, inspeciona seus assets e instala o pacote em consumidores Razor
mínimos `net9.0` e `net10.0`. As dependências ASP.NET Core usam versões de
servicing exatas; o Dependabot mantém a atualização semanal, evitando que dois
restores do mesmo commit escolham versões diferentes.

Uma varredura local encontra referências à biblioteca em 11 projetos de
aplicação/biblioteca e em um projeto de testes. Dez caminhos de produção já
resolvem a estrutura atual; o `sufficit-checkout` ainda usa o caminho legado e
permanece como débito explícito do rollout de consumidores.

## Instalação

Por pacote:

```xml
<PackageReference Include="Sufficit.Blazor.UI" Version="1.*" />
```

Durante desenvolvimento conjunto, um consumidor também pode apontar para
`src/Sufficit.Blazor.UI.csproj` com `ProjectReference`.

Inclua no `<head>` tanto o entrypoint global da RCL quanto o bundle de CSS
isolation gerado para a aplicação consumidora:

```html
<link href="_content/Sufficit.Blazor.UI/sufficit-ui.css" rel="stylesheet" />
<link href="MinhaAplicacao.styles.css" rel="stylesheet" />
```

Substitua `MinhaAplicacao` pelo assembly do projeto host. O primeiro arquivo
carrega tokens, primitives compartilhadas, portais e as regras globais ainda
em migração; o segundo reúne os `.razor.css` da aplicação e das RCLs
referenciadas. Carregar apenas um deles deixa parte dos componentes sem estilo.
O entrypoint `sufficit-ui.css` permanece compatível durante a janela de
migração dos consumidores.

Não inclua scripts SUI manualmente. Cada componente com interop importa seu
módulo JavaScript colocalizado de forma assíncrona e remove listeners no
descarte.

Registre os serviços no `Program.cs`:

```csharp
using Sufficit.Blazor.UI;

builder.Services.AddSufficitUI();
```

E instale o provider uma única vez na raiz interativa da aplicação:

```razor
@using Sufficit.Blazor.UI.Themes

<SUIThemeProvider>
    <Routes />
</SUIThemeProvider>
```

## Tema

O contrato público usa o casing real do código: `ISUITheme`, `SUIPalette`,
`SUITypography`, `SUILayout`, `DefaultSUITheme` e `SUIThemeProvider`.

Uma aplicação pode fornecer seu tema pelo DI:

```csharp
builder.Services.AddSufficitUI(options =>
    options.Theme = new IdentitySUITheme());
```

Ou diretamente no provider:

```razor
<SUIThemeProvider Theme="Theme">
    <Routes />
</SUIThemeProvider>
```

O provider publica os tokens `--sui-*` globalmente, inclusive `color-scheme`,
para que menus, dialogs, tooltips e toasts anexados ao `body` recebam a mesma
paleta. `ISUITheme.IsDark` seleciona o esquema; a paleta continua pertencendo
ao tema consumidor. Sem configuração, `DefaultSUITheme` fornece o fallback
claro azul.

Exemplos de tokens:

- cores: `--sui-color-primary`, `--sui-color-primary-contrast`,
  `--sui-surface`, `--sui-text-primary`, `--sui-border`;
- tipografia: `--sui-font`, `--sui-fs-body`, `--sui-lh-body`;
- layout: `--sui-space-*`, `--sui-radius-*`, `--sui-shadow-*` e
  `--sui-control-h-*`.

## Catálogo de componentes

A referência por família, contratos de forms e páginas de Select, Tooltip,
NavGroup, Dialog e ThemeProvider ficam em
[`docs/components`](docs/components/README.md).

Todos os componentes públicos usam o prefixo `SUI`.

| Família | Componentes |
| --- | --- |
| Ações | `SUIButton`, `SUIIconButton`, `SUILoadingButton`, `SUILink` |
| Formulários | `SUIAutocomplete`, `SUIChoiceCard<TValue>`, `SUINumericField`, `SUISelect<T>`, `SUISelectItem`, `SUISwitch`, `SUISwitchButton`, `SUITextField` |
| Layout | `SUIAppBar`, `SUICard`, `SUIContainer`, `SUIDivider`, `SUIDrawer`, `SUIGrid`, `SUILayout`, `SUIPageHeader`, `SUISpacer`, `SUIStack` |
| Navegação | `SUIItem`, `SUIList`, `SUIListItem`, `SUINavGroup`, `SUINavLink`, `SUITabPanel`, `SUITabs` |
| Exibição de dados | `SUIChip`, `SUIIcon`, `SUIStatusBadge`, `SUITable`, `SUITableEmpty`, `SUITd`, `SUIText`, `SUITh`, `SUITimeline`, `SUITimelineItem` |
| Feedback | `SUIAlert`, `SUIEmptyState`, `SUIProgressLinear`, `SUISkeletonLoader`, `SUIStatusBanner`, `SUIToast` |
| Overlays | `SUIConfirmDialog`, `SUIDialogHost`, `SUISnackbarHost`, `SUITooltip` |

Enums como `SUIColor`, `SUIVariant`, `SUISize`, `SUIButtonType`, `SUIEdge`,
`SUITypo`, `SUIAlign`, `SUIOrigin` e `SUITone` evitam dependência de tipos
visuais externos. Algumas APIs ainda aceitam valores legados por uma ponte de
compatibilidade; código novo deve usar os enums SUI.

## Exemplos

### Ação e feedback

```razor
<SUIButton ColorValue="SUIColor.Primary" OnClick="SaveAsync">
    Salvar alterações
</SUIButton>

<SUIStatusBanner Tone="SUITone.Success"
                 Title="Runtime estável"
                 Description="Todos os workers responderam." />
```

### Choice card

```razor
<SUIChoiceCard TValue="PaymentMethod"
               Value="PaymentMethod.Pix"
               SelectedValue="SelectedMethod"
               SelectedValueChanged="SelectMethod"
               Name="payment-method"
               Title="PIX"
               Description="Confirmação rápida, a qualquer hora"
               LeadingTone="SUITone.Success" />
```

### Select

```razor
<SUISelect T="string"
           Label="Região"
           Value="SelectedRegion"
           ValueChanged="OnRegionChanged"
           MenuMaxWidth="36rem">
    <SUISelectItem T="string" Value="sudeste">Sudeste</SUISelectItem>
    <SUISelectItem T="string" Value="sul">Sul</SUISelectItem>
</SUISelect>
```

O menu do `SUISelect` usa a top layer quando disponível, reposiciona-se nas
bordas da viewport e suporta `ArrowUp`, `ArrowDown`, `Home`, `End`, `Enter`,
`Space` e `Escape`. Use `<select>` nativo quando postagem HTML ou o seletor do
sistema operacional forem requisitos.

### Tooltip

```razor
<SUITooltip Text="Armazenamento"
            Placement="SUITooltipPlacement.Right"
            MaxWidth="280">
    <SUIIconButton Icon="@Icons.Storage" AriaLabel="Armazenamento" />
</SUITooltip>
```

## Engenharia e documentação

- [Atividade concluída de arquitetura e hardening](docs/activities/202608141316-completed-sui-architecture-hardening.md)
- [Rollout e rollback dos consumidores](docs/CONSUMER-ROLLOUT.md)
- [Índice de documentação](docs/README.md)
- [Skill e convenções SUI](skills/sui-design/SKILL.md)

A atividade registra a migração para organização por famílias, CSS híbrido,
módulos JavaScript colocalizados, catálogo executável, testes de componentes,
gates de acessibilidade, pacote final e validação nos consumidores.

## Licença

MIT-0 — veja [LICENSE](LICENSE).
