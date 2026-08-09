# Sufficit.Blazor.UI

Biblioteca de componentes Blazor da Sufficit User Interface (SUI). Autônoma:
**não depende de MudBlazor** (nem pacote, nem código-fonte vendorizado). Traz o
próprio stylesheet (`sufficit-ui.css`) com design tokens `--sui-*`.

**Status: primeiro consumidor migrado.** O repositório consolidou os
componentes compartilhados que viviam no `sufficit-blazor`; a biblioteca já é
consumida por referência de projeto e permanece agnóstica de domínio.

## Alvo

`net10.0`. O `sufficit-blazor` já foi migrado para `net10.0`; o
`sufficit-ai` continua em `net9.0` e será avaliado separadamente.

## Componentes

Prefixo `SUI`, namespace único `Sufficit.Blazor.UI.Components`. São componentes
nossos, baseados em Blazor puro (HTML + CSS), sem herança de bibliotecas de
terceiros.

| Componente | Substitui (no `sufficit-blazor`) | Uso hoje |
| --- | --- | --- |
| `SUIButton` | `MudButtonEnchanted` | 16 |
| `SUINavGroup` | `MudNavGroupEnhanted` | 16 |
| `SUITableEmpty` | `TableNoRecords` | 40 |
| `SUILoadingButton` | `LoadingButton` | 5 |
| `SUINavLink` | `MudNavLinkEnchanted` | 3 |
| `SUIIconButton` | `MudIconButtonEnchanted` | 2 |
| `SUISkeletonLoader` | `SkeletonLoader` | 2 |
| `SUIEmptyState` | `EmptyState` | 1 |
| `SUISwitchButton` | `MudSwitchButton` | 1 |

`SUISkeletonType` acompanha o `SUISkeletonLoader`.

`GenericTable` foi descartado: zero usos em qualquer projeto.

## Estilo e tokens

Os componentes não usam Material Design. Toda a estilização vem de
`wwwroot/sufficit-ui.css`, com tokens próprios em `:root` (modo claro) e
`[data-sui-theme="dark"]` (modo escuro). Para adotar o tema escuro, basta
colocar o atributo no elemento raiz (ex.: `<body data-sui-theme="dark">`).

Inclua a folha de estilo na aplicação consumidora. O caminho exato depende de
como a biblioteca é consumida:

- **Como referência de projeto** (uso atual no `sufficit-blazor`): adicione o
  `ProjectReference` para `Sufficit.Blazor.UI.csproj` e referencie
  `<link href="_content/Sufficit.Blazor.UI/sufficit-ui.css" rel="stylesheet" />`.
- **Copiando os `.razor`**: ainda é possível para protótipos isolados; copie
  também `wwwroot/sufficit-ui.css` e referencie-o diretamente. Não há pacote
  NuGet publicado neste momento (`IsPackable=false`).

## API

Os parâmetros que controlam aparência usam enums próprios (não do MudBlazor):
`SUIColor`, `SUIVariant`, `SUISize`, `SUIButtonType`, `SUIEdge`, `SUITypo`,
`SUIAlign`, `SUIOrigin`. Quem está migrando de componentes MudBlazor precisa
preferir esses enums no código novo. Os controles de ação mantêm uma ponte
temporária para valores visuais legados, permitindo migrar telas sem uma troca
big-bang.

## Namespaces

Namespace único: `Sufficit.Blazor.UI.Components`.

## O que ainda não está aqui, e por quê

- **`Layout` e `UI/FilterControl`** do `sufficit-blazor` referenciam domínios de
  negócio (telefonia, financeiro, gateway de mensagens, logging). Não são
  genéricos como estão: precisam ser desacoplados antes de entrar numa
  biblioteca compartilhada — ainda mais numa pública.
- **Contrato de temas.** O `sufficit-blazor` já tem `ThemeService` e
  `MudThemeContainer`; eles evoluem para um contrato explícito (paleta,
  tipografia, densidade) quando houver mais de um consumidor real. Desenhar
  temas antes disso é adivinhação.
- **Testes e CI.** A serem adicionados junto com o primeiro consumidor.

## Como usar

No consumidor principal, use uma referência de projeto e inclua o stylesheet
estático `_content/Sufficit.Blazor.UI/sufficit-ui.css`. O projeto continua
existindo como biblioteca Razor independente, sem pacote MudBlazor e sem
componentes vendorizados.

## Roadmap

1. [x] Migrar `sufficit-blazor` para `net10.0`.
2. [x] Adotar os componentes no `sufficit-blazor`, trocando os nomes antigos
   pelos `SUI*` e removendo as duplicatas locais.
3. Contrato de temas, quando houver mais de um consumidor real.

Observação: o `sufficit-ai` hoje **não usa nenhum** destes componentes. A
consolidação lá é adoção, não migração — vale confirmar se compensa.

## SUINavGroup

O `SUINavGroup` reimplementa o padrão de navegação em árvore (grupo expansível,
modo rail com flyout flutuante, accordion exclusivo entre irmãos) usando apenas
CSS — `grid-template-rows: 0fr↔1fr` para o collapse animado, posicionamento
absoluto para o flyout. Não há interop JavaScript nem portal.

## Licença

MIT-0 para o código da Sufficit — ver [LICENSE](LICENSE). Compartilhamento
máximo, sem exigência de atribuição.
