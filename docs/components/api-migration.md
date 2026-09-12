# Migração da API tipada

As pontes `object`/`string` foram **removidas** na release de limpeza da API
(2026-09-11, linha `2.26.911.x`). Só os parâmetros tipados existem:

| Ponte removida | Parâmetro atual |
| --- | --- |
| `Color`, `IconColor` | `ColorValue`, `IconColorValue` (`SUIColor`) |
| `Variant` | `VariantValue` (`SUIVariant`) |
| `Size`, `IconSize` | `SizeValue`, `IconSizeValue` (`SUISize`) |
| `ButtonType` | `ButtonTypeValue` (`SUIButtonType`) |
| `Edge` | `EdgeValue` (`SUIEdge`) |
| Alert `Severity`, Badge `Tone` | `ToneValue` (`SUITone`) |

Componentes afetados: `SUIButton`, `SUIIconButton`, `SUILoadingButton`,
`SUIChip`, `SUITimelineItem`, `SUIProgressLinear`, `SUISwitch`, `SUICheckbox`,
`SUIAlert`, `SUIStatusBadge`. Os padrões visuais não mudaram: um `SUIButton`
sem `ColorValue` continua `Primary`, um `SUIIconButton` sem `VariantValue`
continua `Text`, um `SUIAlert` sem `ToneValue` continua `Info`.

Mapeamento dos valores string antigos para `SUITone`: `"success"` →
`SUITone.Success`, `"warning"` → `SUITone.Warning`, `"danger"` e `"error"` →
`SUITone.Danger`, `"info"` → `SUITone.Info`, qualquer outro → `SUITone.Neutral`.
Quando o tom chega como string de uma API, converta no consumidor (uma função
`SUITone Parse(string)` de cinco linhas) e mantenha a string para as classes
CSS próprias.

Busca para localizar call sites antigos (revisar em contexto):

```bash
rg -n '<SUI(Button|IconButton|LoadingButton|Chip|TimelineItem|ProgressLinear|Switch|Checkbox|Alert|StatusBadge)\b[^>]*\s(Color|IconColor|Variant|Size|IconSize|ButtonType|Edge|Severity|Tone)=' --glob '*.razor'
```

`SUISelectItem.Value` permanece `object` de propósito: o item Razor é não
genérico e registra seu valor no `SUISelect<T>` pai, que faz a comparação no
tipo `T`.

`SUIAlert.CloseIconClicked` foi substituído por `OnClose`. `SUIItem` usa
`Xs`, `Sm`, `Md`, `Lg`, `Xl` (antes minúsculos).

Adapters para bibliotecas visuais de terceiros não entram nos componentes-base;
se ainda necessários, devem ficar num pacote/camada legada separado.
