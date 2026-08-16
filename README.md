# Sufficit.Blazor.UI

Razor Class Library dos componentes **Sufficit User Interface (SUI)**. A
biblioteca usa Blazor puro, HTML, CSS e módulos JavaScript ES; não depende de
nenhuma biblioteca visual de terceiros nem contém código-fonte vendorizado.

## Compatibilidade e distribuição

- Target frameworks: `net9.0` e `net10.0`.
- Package ID: `Sufficit.Blazor.UI`.
- Distribuição: pacote NuGet e `ProjectReference` local.
- Namespace dos componentes: `Sufficit.Blazor.UI.Components`.
- Namespace de temas: `Sufficit.Blazor.UI.Themes`.

A série v1 mantém os dois TFMs. A v2 está planejada como net10-only, não antes
do fim de suporte do .NET 9 em 2026-11-10 e condicionada à validação de todos os
consumers conhecidos. Veja a
[política de versionamento e TFMs](docs/ARCHITECTURE-VERSIONING-AND-TFM.md) e o
[plano da v2](docs/PLAN-SUI-V2.md).

O CI compila ambos os frameworks com warnings tratados como erros, gera o
`.nupkg`, inspeciona seus assets e instala o pacote em RCLs e Blazor Web Apps
temporárias `net9.0` e `net10.0`. As apps são iniciadas na raiz e sob
`PathBase`, validando markup SSR, CSS global/isolation e módulos. As dependências ASP.NET Core usam versões de
servicing exatas; o Dependabot mantém a atualização semanal, evitando que dois
restores do mesmo commit escolham versões diferentes.

Builds locais usam a versão não publicável `0.0.0-local`. Uma release nasce
somente de tag `vMAJOR.MINOR.PATCH[-prerelease]`; o pacote só é enviado ao
NuGet.org depois dos gates multialvo, bUnit, Playwright/axe e validação do
artefato exato. Veja o [runbook de release](docs/RUNBOOK-RELEASE.md) e o
[changelog](CHANGELOG.md).

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

Substitua `MinhaAplicacao` pelo assembly do projeto host. O primeiro arquivo é
gerado e minificado a partir de fontes modulares; sem `@import` em runtime, ele
carrega tokens, primitives compartilhadas, portais e as regras globais ainda
em migração; o segundo reúne os `.razor.css` da aplicação e das RCLs
referenciadas. Carregar apenas um deles deixa parte dos componentes sem estilo.
O entrypoint `sufficit-ui.css` permanece compatível durante a janela de
migração dos consumidores.

Não inclua scripts SUI manualmente. Cada componente com interop importa seu
módulo JavaScript colocalizado de forma assíncrona e remove listeners no
descarte.

### Desenvolvimento do CSS

Os fontes autorais ficam em `src/styles`: foundations, portals, regras globais
de compatibilidade e o entrypoint de build. Depois de alterá-los, execute:

```bash
npm ci
npm run build:css
npm run check:css
```

O primeiro comando instala a versão travada do Lightning CSS; o segundo gera o
único asset público `src/wwwroot/sufficit-ui.css`; o terceiro confirma que o
artefato está atualizado e dentro dos budgets bruto, gzip e Brotli. O output é
minificado, portanto diagnósticos devem ser rastreados aos arquivos autorais,
não editados diretamente no arquivo gerado.

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
  `--sui-color-primary-action`, `--sui-color-primary-action-contrast`,
  `--sui-surface`, `--sui-text-primary`, `--sui-border`;
- tipografia: `--sui-font`, `--sui-fs-body`, `--sui-lh-body`;
- layout: `--sui-space-*`, `--sui-radius-*`, `--sui-shadow-*` e
  `--sui-control-h-*`.

Na `SUIPalette`, `Primary` continua sendo o acento de marca. Os campos
opcionais `PrimaryAction`/`PrimaryActionContrast` permitem uma superfície
própria para botões primários preenchidos; quando omitidos, o provider recua
para `Primary`/`PrimaryContrast` e preserva temas existentes.

## Catálogo de componentes

A referência por família, contratos de forms e páginas de Select, Tooltip,
NavGroup, Dialog e ThemeProvider ficam em
[`docs/components`](docs/components/README.md).

Todos os componentes públicos usam o prefixo `SUI`.

| Família | Componentes |
| --- | --- |
| Ações | `SUIButton`, `SUIIconButton`, `SUILoadingButton`, `SUILink` |
| Formulários | `SUIAutocomplete`, `SUIChoiceCard<TValue>`, `SUIFormGrid`, `SUINumericField`, `SUISelect<T>`, `SUISelectItem`, `SUISwitch`, `SUISwitchButton`, `SUITextField` |
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

## Testes

A suíte é dividida em três camadas. Todas rodam no CI e falham o build.

| Camada | Projeto/gate | Cobre |
| --- | --- | --- |
| Contrato de código | `tests/Sufficit.Blazor.UI.Tests` | bUnit (render, lifecycle, forward de atributos, acessibilidade de formulário), compatibilidade de API pública, convenções de nome e namespace, tamanho de arquivo, contrato de CSS, budgets de bytes e guarda anti-biblioteca-de-terceiros |
| Navegador | `tests/Sufficit.Blazor.UI.BrowserTests` | Playwright + axe em chromium/firefox/webkit: WCAG 2.2 AA em desktop/mobile e light/dark, teclado, foco visível, focus trap, forced-colors, RTL, zoom 200%, alvos de 44px, baselines visuais, budgets de runtime (requests, DOM, bytes, LCP, CLS) |
| Página completa | job `lighthouse` | `eng/lighthouse-budget.json` + `scripts/check-lighthouse.mjs`: categorias performance/accessibility/best-practices/SEO e métricas FCP, LCP, TBT, CLS, Speed Index |

```bash
dotnet test tests/Sufficit.Blazor.UI.Tests/Sufficit.Blazor.UI.Tests.csproj

# o catálogo precisa estar no ar para as camadas 2 e 3
dotnet run --project samples/Sufficit.Blazor.UI.Catalog/Sufficit.Blazor.UI.Catalog.csproj \
  -c Release --urls http://127.0.0.1:5180 &
BROWSER=chromium dotnet test tests/Sufficit.Blazor.UI.BrowserTests/Sufficit.Blazor.UI.BrowserTests.csproj
npx --yes lighthouse@12 http://127.0.0.1:5180 --preset=desktop --output=json \
  --output-path=artifacts/lighthouse/report.json --budget-path=eng/lighthouse-budget.json
node scripts/check-lighthouse.mjs artifacts/lighthouse/report.json
```

Budgets são teto, não meta móvel: quando um arquivo ou payload estoura, a
correção é dividir ou reduzir. Os poucos casos herdados ficam congelados em
listas de débito explícitas (`FileSizeBudgetTests.Debt`,
`NamingConventionTests.LegacyParameterNames`) que só podem encolher.

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
