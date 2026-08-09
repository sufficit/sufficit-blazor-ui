# Sufficit.Blazor.UI

Biblioteca de componentes Blazor da Sufficit, construída sobre
[MudBlazor](https://mudblazor.com). Base compartilhada entre as aplicações da
Sufficit, com suporte a temas por aplicação.

**Status: fase inicial.** O repositório existe para consolidar componentes que
hoje vivem no `sufficit-blazor` e são reimplementados em cada aplicação. A
superfície abaixo é o núcleo agnóstico de domínio; o restante entra
gradualmente.

## Alvo

`net10.0`. As aplicações de origem (`sufficit-blazor`, `sufficit-ai`) estão em
`net9.0` e serão migradas — um RCL Razor não atravessa essa diferença, então a
migração é pré-requisito para o consumo.

## Componentes

Prefixo `Suff`, namespace único `Sufficit.Blazor.UI.Components`. São componentes
nossos: usam MudBlazor por baixo, mas a API e o nome são da Sufficit.

| Componente | Substitui | Uso hoje |
| --- | --- | --- |
| `SuffButton` | `MudButtonEnchanted` | 16 |
| `SuffNavGroup` | `MudNavGroupEnhanted` | 16 |
| `SuffTableEmpty` | `TableNoRecords` | 40 |
| `SuffLoadingButton` | `LoadingButton` | 5 |
| `SuffNavLink` | `MudNavLinkEnchanted` | 3 |
| `SuffIconButton` | `MudIconButtonEnchanted` | 2 |
| `SuffSkeletonLoader` | `SkeletonLoader` | 2 |
| `SuffEmptyState` | `EmptyState` | 1 |
| `SuffSwitchButton` | `MudSwitchButton` | 1 |

`SuffSkeletonType` acompanha o `SuffSkeletonLoader`.

`GenericTable` foi descartado: zero usos em qualquer projeto.

## Namespaces

Namespace único: `Sufficit.Blazor.UI.Components`.

Cinco dos componentes originais declaravam `@namespace MudBlazor` — ou seja, se
injetavam dentro do namespace da biblioteca de terceiros. Funcionava (dispensava
`@using`), mas colide com qualquer tipo futuro de mesmo nome no MudBlazor e
esconde a origem do componente na leitura do código. Agora ficam no namespace
próprio.

## O que ainda não está aqui, e por quê

- **`Layout` e `UI/FilterControl`** do `sufficit-blazor` referenciam domínios de
  negócio (telefonia, financeiro, gateway de mensagens, logging). Não são
  genéricos como estão: precisam ser desacoplados antes de entrar numa
  biblioteca compartilhada — ainda mais numa pública.
- **`MudThemeManagerButtonAdmin`** depende do pacote `MudBlazor.ThemeManager`.
  Incluí-lo obrigaria a carregar essa dependência por causa de um componente.
- **Contrato de temas.** O `sufficit-blazor` já tem `ThemeService` e
  `MudThemeContainer`; eles evoluem para um contrato explícito (paleta,
  tipografia, densidade) quando houver mais de um consumidor real. Desenhar
  temas antes disso é adivinhação.
- **Testes e CI.** A serem adicionados junto com o primeiro consumidor.

## Como usar

Copie o `.razor` (e o `.razor.cs`/`.cs` quando houver) para o seu projeto e
ajuste o `@namespace`. Não há pacote a referenciar: são poucos componentes e a
cópia evita acoplar as aplicações a um ciclo de release desta biblioteca.

O projeto aqui existe para o CI provar que os componentes compilam.

## Roadmap

1. Migrar `sufficit-blazor` e `sufficit-ai` para `net10.0`.
2. Adotar os componentes no `sufficit-blazor`, trocando os nomes antigos pelos
   `Suf*` — é onde estão todos os usos atuais.
3. Contrato de temas, quando houver mais de um consumidor real.

Observação: o `sufficit-ai` hoje **não usa nenhum** destes componentes. A
consolidação lá é adoção, não migração — vale confirmar se compensa.

## Pendências conhecidas

- A versão do MudBlazor segue como `9.*`. Agora que a build passou, vale fixar
  a versão exata que passou.
- O CI compila em `net10.0` com MudBlazor 9 e `-warnaserror`, sem warnings e
  sem alertas do CodeQL. A compatibilidade MudBlazor 9 + .NET 10 está
  confirmada — era a principal incógnita da migração.

## Licença

MIT-0 para o código da Sufficit — ver [LICENSE](LICENSE). Compartilhamento
máximo, sem exigência de atribuição.

**Exceção:** `SuffNavGroup.razor.cs` é derivado do MudBlazor e mantém o
cabeçalho de copyright original. O MudBlazor é MIT, que **exige** preservar o
aviso de licença — por isso o cabeçalho fica no arquivo e não pode ser
removido. Isso não afeta o restante do repositório.
