# Migração da API tipada

O inventário encontrou 24 parâmetros públicos `object` (a estimativa inicial
era 25): 15 em Buttons, seis em Chip/TimelineItem, um em Progress, um em Switch
e `SUISelectItem.Value`.

Os 23 parâmetros visuais têm alternativas enum tipadas:

| Ponte antiga | Alternativa |
| --- | --- |
| `Color`, `IconColor` | `ColorValue`, `IconColorValue` (`SUIColor`) |
| `Variant` | `VariantValue` (`SUIVariant`) |
| `Size`, `IconSize` | `SizeValue`, `IconSizeValue` (`SUISize`) |
| `ButtonType` | `ButtonTypeValue` (`SUIButtonType`) |
| `Edge` | `EdgeValue` (`SUIEdge`) |
| Alert `Severity`, Badge `Tone` | `ToneValue` (`SUITone`) |

As pontes têm `ObsoleteAttribute` com substituição e remoção em v2.0.0, mas
continuam com a mesma assinatura binária nesta major. Valor tipado vence quando
ambos são fornecidos. `SUISelectItem.Value` permanece `object` de propósito: o
item Razor é não genérico e registra seu valor no `SUISelect<T>` pai, que faz a
comparação no tipo `T`.

Adapters para bibliotecas visuais de terceiros não entram nos componentes-base;
se ainda necessários, devem ficar num pacote/camada legada separado.
