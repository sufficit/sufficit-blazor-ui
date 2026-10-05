# Rampa tipográfica: duas famílias, uma direção

`SUITypo` tem duas famílias de membros. Este documento registra a decisão sobre
qual delas é o caminho para telas novas, o que acontece com a outra e como
traduzir entre as duas. Origem: item F3 da avaliação de 2026-10-04.

## A decisão

| Família | Membros | Papel |
| --- | --- | --- |
| **Semântica operacional** | `display`, `headline`, `title`, `body`, `label`, `mono` | **Direção** para telas operacionais novas |
| **Material** | `h1`–`h6`, `subtitle1/2`, `body1/2`, `button`, `caption`, `overline` | **Suportada**; não é obsoleta antes da próxima major |

Por que a semântica é a direção:

- **Cada papel é tematizável.** `SUITypography` expõe `FsDisplay`, `FsHeadline`,
  `FsTitle`, `FsBody`, `FsLabel`, `FsMono` e as alturas de linha
  `LineHeight*` correspondentes; o `SUIThemeCssWriter` publica tudo como
  `--sui-fs-*` / `--sui-lh-*`. Um consumidor muda o papel inteiro no tema, sem
  CSS próprio.
- **O tamanho segue o papel, não o nível do documento.** Em telas densas o
  título de página raramente precisa dos 2,5 rem do `h1`; o papel `display`
  usa um `clamp` de 1,55 a 2,25 rem.
- **A rampa Material continua necessária hoje.** Todos os componentes internos
  (`SUIDrawer`, `SUIPagination`, `SUIStat`, `SUITableEmpty`) a usam, e há
  consumidores publicados. Não há nada a remover agora.

O que **não** foi decidido (e por quê): remover ou marcar `[Obsolete]` qualquer
membro. Isso quebra consumidores e só cabe na janela de quebra da próxima major,
junto com as pontes `[Obsolete]` da migração para `SUIComponentBase`. Até lá, a
regra é de orientação, não de enforcement.

## As famílias não são intercambiáveis

Trocar `h3` por `headline` **muda o tamanho e o elemento**:

1. **Tamanhos diferentes.** Não existe equivalência exata — a tabela abaixo é de
   *vizinho mais próximo*, para orientar migração de tela, não para busca e
   substituição.
2. **Só `h1`–`h6` viram heading nativo.** Com `SUITextTag.Auto`, os seis membros
   semânticos renderizam `div`. Para título de página com semântica de heading,
   passe a tag explicitamente: `<SUIText Typo="SUITypo.display" Tag="SUITextTag.H1">`.
3. **Pesos e alturas de linha diferem.** `title` é 700 onde `h6` é 600; `label`
   é 500 onde `caption` é 400.

## Mapeamento (vizinho mais próximo)

Valores padrão do tema; o tema do consumidor pode alterá-los.

| Material | Tamanho | Semântico mais próximo | Tamanho | Observação |
| --- | --- | --- | --- | --- |
| `h1` | 2,5 rem | — | — | Maior que `display` (máx. 2,25 rem); manter `h1` |
| `h2` | 2 rem | `display` | 1,55–2,25 rem | Título de página |
| `h3` | 1,6 rem | `display` / `headline` | 1,28 rem | Depende da densidade da tela |
| `h4` | 1,35 rem | `headline` | 1,28 rem | Estado ou diagnóstico dominante |
| `h5` | 1,15 rem | `title` | 1 rem | Título de cartão ou seção |
| `h6`, `subtitle1` | 1 rem | `title` | 1 rem | Mesmo tamanho; peso 700 |
| `subtitle2`, `body2` | 0,875 rem | `body` | 0,875 rem | Mesmo tamanho |
| `body1` | 1 rem | — | — | Maior que `body`; manter `body1` |
| `caption` | 0,75 rem | `label` | 0,75 rem | Mesmo tamanho; peso 500 |
| `button`, `overline` | 0,875 / 0,6875 rem | — | — | Sem papel equivalente |
| — | — | `mono` | 0,76 rem | Sem contraparte Material |

Linhas com "—" não têm destino semântico: continue na rampa Material.

## Regras para código novo

- Tela operacional nova: comece pelos papéis semânticos.
- Precisa de heading nativo (acessibilidade, ordem de títulos): `Tag` explícita.
- Não misture as famílias **dentro de um mesmo cartão**: dois vocabulários de
  tamanho na mesma superfície é o que a avaliação chamou de "enum órfão".
- Componente interno novo da biblioteca: use papéis semânticos; os existentes só
  migram na janela de quebra (mudar o tamanho de `SUIPagination` ou `SUIStat`
  altera a aparência para todos os consumidores).

## O que o teste garante

`TypographyRampContractTests` impede que a decisão se desfaça em silêncio: todo
membro de `SUITypo` tem sua regra `.sui-text--*`; todo papel semântico tem token
de tamanho e de altura de linha, propriedade em `SUITypography` e escrita no tema;
nenhum membro virou `[Obsolete]` antes da major; e este documento continua citando
os seis papéis.
