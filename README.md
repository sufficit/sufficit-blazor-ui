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

### `Components/MudBlazorExtended`

Extensões sobre componentes do MudBlazor, preservando a API original e
acrescentando comportamento:

- `MudButtonEnchanted`
- `MudIconButtonEnchanted`
- `MudNavLinkEnchanted`
- `MudNavGroupEnhanted`
- `MudSwitchButton`

### `Components/UX`

- `EmptyState` — estado vazio com título, descrição e ação opcional
- `SkeletonLoader` — carregamento por esqueleto (ver `SkeletonLoaderType`)
- `LoadingButton` — botão com estado de operação em andamento

### `Components/Tables`

- `GenericTable`
- `TableNoRecords`

## Namespaces

O namespace raiz é `Sufficit.Blazor.UI`, deliberadamente diferente do pacote
legado `Sufficit.Blazor`. Durante a migração, um projeto pode referenciar os
dois; namespaces idênticos tornariam cada uso ambíguo.

## O que ainda não está aqui, e por quê

- **`Layout` e `UI/FilterControl`** do `sufficit-blazor` referenciam domínios de
  negócio (telefonia, financeiro, gateway de mensagens, logging). Não são
  genéricos como estão: precisam ser desacoplados antes de entrar numa
  biblioteca compartilhada — ainda mais numa pública.
- **`MudThemeManagerButtonAdmin`** depende do pacote `MudBlazor.ThemeManager`.
  Incluí-lo obrigaria todo consumidor a carregar essa dependência por causa de
  um componente. O contrato de tema próprio (abaixo) o substitui.
- **Contrato de temas.** O `sufficit-blazor` já tem `ThemeService` e
  `MudThemeContainer`; eles evoluem para um contrato explícito (paleta,
  tipografia, densidade) quando houver mais de um consumidor real. Desenhar
  temas antes disso é adivinhação.
- **Testes e CI.** A serem adicionados junto com o primeiro consumidor.

## Roadmap

1. Migrar `sufficit-blazor` e `sufficit-ai` para `net10.0`.
2. Publicar este pacote num feed interno.
3. Primeiro consumidor real (`sufficit-ai`), eliminando duplicatas.
4. Contrato de temas, com dois consumidores para validá-lo.
5. Decidir sobre o `sufficit-identity`, que hoje usa CSS próprio sem MudBlazor
   — adotar a base ali é reescrever o front, não consolidar.

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
