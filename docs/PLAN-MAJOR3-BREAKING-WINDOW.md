# Plano — janela de quebra da próxima major (v3)

**Status:** planejado (inventário travado por contrato; execução só na janela)
**Criado:** 2026-10-07
**Origem:** fecho da onda P1/F2/F3 da consolidação de design —
[DESIGN-TYPOGRAPHY-RAMP.md](DESIGN-TYPOGRAPHY-RAMP.md) e a avaliação de
2026-10-04 definiram uma **janela de quebra única** para a próxima major.

Este documento é o inventário do que entra nessa janela, o que fica de fora e
a mecânica de execução. Nada aqui é executado antes da janela abrir.

## Por que uma janela única

A linha atual (major 2, calendário `2.yy.MMdd.HHmm`) acumulou pontes de
migração deliberadas: parâmetros `[Obsolete]` que só existem para não quebrar
consumidor durante a migração para `SUIComponentBase`, e sobrecargas legadas de
string de severidade. Cada ponte tem custo permanente (superfície de API,
catálogo, supressões) e só pode sair de uma vez. A decisão F3 empurrou para
esta mesma janela a decisão sobre os defaults tipográficos internos, para que
haja **um** evento de quebra, não dois.

## O que sai na janela

### 1. As 21 pontes de atributos (`UserAttributes`/`Attributes`)

Todas com a mesma mensagem: *"Use AdditionalAttributes instead; this alias
will be removed in the next breaking release."* O parâmetro canônico é
`SUIComponentBase.AdditionalAttributes`.

| Família | Componentes | Ponte |
| --- | --- | --- |
| Actions | `SUIButton`, `SUIIconButton`, `SUILoadingButton`, `SUILink` | `UserAttributes` |
| DataDisplay | `SUITh`, `SUITd` | `UserAttributes` |
| Feedback | `SUIToast`, `SUIStatusBanner` | `UserAttributes` |
| Forms | `SUITextField`, `SUINumericField`, `SUISelect`, `SUIAutocomplete`, `SUIDateField` | `UserAttributes` |
| Layout | `SUICard`, `SUICardHeader`, `SUICardContent`, `SUICardActions` | `Attributes` + merge canônico |
| Navigation | `SUINavLink`, `SUINavGroup` (parâmetro aninhado no code-behind) | `UserAttributes` |
| Overlays | `SUIPopover`, `SUITooltip` | `UserAttributes` |

Notas de execução:

- **Família Card** (`SUICard*`): além de remover o parâmetro `Attributes`,
  sai junto o *merge* com o `AdditionalAttributes` canônico que a ponte faz
  hoje (a ponte empurra os valores no dicionário da base).
- **`SUINavGroup`**: a ponte vive em classe aninhada no code-behind;
  confirmar os dois pontos de splat (rail e item) na remoção.
- **`SUIToast`/`SUIStatusBanner`**: as pontes são dos hosts; o
  `SUINotificationHostBase` já é canônico.

### 2. As 2 sobrecargas legadas de severidade por string

| Serviço | Membro `[Obsolete]` |
| --- | --- |
| `ISuiSnackbar` | `Add(message, severity, durationMs)` — *"The string severity bridge will be removed in a future major version."* |
| `ISuiToast` | `Add(message, severity, durationMs, actionLabel, onAction)` — idem |

O caminho canônico é a sobrecarga tipada `Add(message, SUITone, …)`.

### 3. Decisões a tomar dentro da janela (não antes)

1. **Defaults tipográficos internos.** `SUIDrawer` (`subtitle1`),
   `SUIPagination` (`caption`), `SUIStat` (`h5`/`caption`) e
   `SUITableEmpty` (`h6`, via `TitleTypo`) usam a família Material. A
   migração para os papéis semânticos (`title`, `label`) **muda peso e
   tamanho** (ex.: `title` é 700 onde `h6` é 600) — é mudança visual para
   todo consumidor, e por isso só entra na janela, com recaptura de baselines
   visuais pelo canal sancionado (workflow_dispatch com
   `update-baselines=true`, PR de revisão com os PNGs — precedentes PR #47 e
   PR #49) e revisão humana das capturas. Mapeamento na
   [rampa tipográfica](DESIGN-TYPOGRAPHY-RAMP.md).
2. **Futuro da família Material.** F3 manteve `h1`–`h6`/`body1/2`/… sem
   `[Obsolete]`. Na janela decide-se: seguir suportada (decisão atual) ou
   entrar em período de obsoletagem com remoção na major seguinte. Critério:
   varredura de uso nos consumers; se telas ativas ainda dependem dela em
   superfície larga, ela fica.
3. **`SUISelectItem.Value`.** Permanece `object?` **por desenho** (documentado
   em [ARCHITECTURE-VERSIONING-AND-TFM.md](ARCHITECTURE-VERSIONING-AND-TFM.md))
   — não faz parte desta janela.

## O que fica explicitamente de fora

- A família Material do `SUITypo` não é removida nesta janela (ver decisão 2).
- Os 25+ parâmetros visuais legados já saíram em release anterior
  (seção *Removed (breaking)* do CHANGELOG).
- `sufficit-ui.css` segue como único entrypoint global público; CSS isolation
  segue carregado pelo `{Consumer}.styles.css`.

## Mecânica de execução (ordem)

1. **Varredura de consumers** (`sufficit-blazor`, `sufficit-ai-genius`,
   `sufficit-identity` ×3 superfícies, `sufficit-cloud-mobile` — escopo do
   [PLAN-CONSUMER-MIGRATION.md](PLAN-CONSUMER-MIGRATION.md)): procurar
   `UserAttributes=`/`Attributes=` em componentes SUI e chamadas `Add` com
   string de severidade. Migrar o que existir **antes** de remover.
2. **Remoção** das 21 pontes (+ merges) e das 2 sobrecargas, num único commit
   de quebra.
3. **Regenerar o que a superfície alimenta**: baseline de API pública
   (`SUI_UPDATE_PUBLIC_API=1 dotnet test`), catálogo
   (`python3 scripts/generate-catalog.py`), e as supressões de compatibilidade
   **pelo mecanismo oficial** (`ApiCompatGenerateSuppressionFile` via
   `dotnet pack`) — registro manual já produziu nome errado
   (`.AdditionalAttributes.get` vs `get_AdditionalAttributes`, ver
   `src/CompatibilitySuppressions.xml`).
4. **Defaults tipográficos** (decisão 1): migrar, recapturar baselines visuais
   pelo canal do workflow_dispatch e revisar o PR de PNGs.
5. **Bump de `PackageValidationBaselineVersion`** quando a release da janela
   sair, fechando a comparação do ApiCompat na versão com as pontes.
6. **CHANGELOG** em *Removed (breaking)* com a lista completa e o guia de
   migração (precedente: `docs/components/api-migration.md`).

## Gates de saída da janela

- Suíte unitária verde com exit code conferido (sem `grep | tail` mascarando).
- `check-catalog-examples.py` verde (exemplos copiados compilam).
- Pack + `validate-package.sh` verdes; ApiCompat com as supressões oficiais.
- Consumers listados no passo 1 compilando e verdes.
- Baselines visuais recapturadas e aprovadas em PR.
- CI da main 100% verde no commit de quebra.

## O que o contrato garante

`BreakingWindowContractTests` trava o inventário doc↔código: cada `[Obsolete]`
de `src/` é uma ponte listada aqui (contagem exata de 21 componentes + 2
serviços), toda ponte de componente aponta para `AdditionalAttributes` e toda
ponte de serviço para a sobrecarga tipada. Nenhuma ponte nova entra em silêncio
— e nenhuma sai antes da janela sem atualizar este plano.
