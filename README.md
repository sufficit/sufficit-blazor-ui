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

Prefixo `Suf`, namespace único `Sufficit.Blazor.UI.Components`. São componentes
nossos: usam MudBlazor por baixo, mas a API e o nome são da Sufficit.

| Componente | Substitui | Uso hoje |
| --- | --- | --- |
| `SufButton` | `MudButtonEnchanted` | 16 |
| `SufNavGroup` | `MudNavGroupEnhanted` | 16 |
| `SufTableEmpty` | `TableNoRecords` | 40 |
| `SufLoadingButton` | `LoadingButton` | 5 |
| `SufNavLink` | `MudNavLinkEnchanted` | 3 |
| `SufIconButton` | `MudIconButtonEnchanted` | 2 |
| `SufSkeletonLoader` | `SkeletonLoader` | 2 |
| `SufEmptyState` | `EmptyState` | 1 |
| `SufSwitchButton` | `MudSwitchButton` | 1 |

`SufSkeletonType` acompanha o `SufSkeletonLoader`.

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

- A versão do MudBlazor está como `9.*` no `csproj`. **Deve ser fixada** numa
  versão exata assim que a compatibilidade com `net10.0` for confirmada: um
  curinga numa biblioteca compartilhada propaga quebra para todos os
  consumidores de uma vez.
- O código foi copiado de um repositório privado sem arquivo de licença. Este
  repositório adota MIT-0, alinhado ao `sufficit-identity`. **Confirmar se é a
  licença pretendida** antes de divulgar o pacote.
- Nada aqui foi compilado ainda — não havia SDK .NET no ambiente onde a
  extração foi feita. O primeiro `dotnet build` é o gate real.

## Licença

MIT-0. Ver [LICENSE](LICENSE).
