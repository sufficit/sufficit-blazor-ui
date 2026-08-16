# Formulários

Família: `SUITextField<T>`, `SUINumericField<T>`, `SUISelect<T>`,
`SUIAutocomplete<T>`, `SUIChoiceCard<TValue>`, `SUISwitch` e
`SUISwitchButton`. `SUIFormGrid` organiza fields equivalentes em colunas.

## Layout horizontal

Use `SUIFormGrid` quando dois ou mais fields SUI compartilharem uma linha:

```razor
<SUIFormGrid Columns="2" LabelLines="2">
    <SUITextField T="string" Label="Nome público" @bind-Value="Model.Name" />
    <SUISelect T="string" Label="Região" @bind-Value="Model.Region">
        ...
    </SUISelect>
</SUIFormGrid>
```

A primitive aplica colunas `minmax(0, 1fr)`, alinha filhos pelo topo, reserva
altura equivalente para labels e empilha em uma coluna a `44rem`. Ela também
emite `data-sui-align-row`, permitindo que o auditor geométrico da skill valide
labels e controles renderizados. Use `data-sui-align-field` somente em wrappers
adicionais que escondem o field real do filho direto.

`Columns` aceita 1–4, `Spacing` usa a escala `--sui-space-0`–`6` e
`LabelLines` reserva 1–3 linhas. Valores fora dos limites são normalizados.
Defina `StackOnMobile="false"` apenas quando houver evidência de que as colunas
continuam utilizáveis em viewport estreita e zoom de 200%.

## Opções com `SUIChoiceCard`

`SUIChoiceCard<TValue>` reserva as colunas de ícone e conteúdo final somente
quando `IconContent`, `TrailingContent` ou o indicador padrão forem realmente
renderizados. Sem ícone, título e descrição ocupam a coluna principal inteira.
Textos longos quebram linha em vez de serem truncados; quando há descrição, o
indicador se alinha ao início do título para não parecer deslocado verticalmente.

Opções do mesmo grupo devem compartilhar `Name`. Use `Description` apenas para
informação necessária à decisão e mantenha `ShowSelectionIndicator="true"`
quando não houver outro `TrailingContent` que comunique o estado selecionado.

## Contrato de valor e validação

Os fields são componentes controlados com `Value`/`ValueChanged`. Eles não
herdam `InputBase<T>` e não leem automaticamente mensagens do `EditContext`.
`Invalid` e `ErrorText` pertencem ao caller; quando usados em `EditForm`, o
caller deve derivá-los do seu validador ou de `EditContext.GetValidationMessages`
e passá-los explicitamente. `SUITextField<T>.ValueExpression` mantém o contrato
de binding existente, mas não transforma o componente em `InputBase<T>`.

Essa decisão evita mudar parsing, timing de notificação e CSS em uma minor.
Se integração automática for necessária, ela deve nascer em adapters separados
derivados de `InputBase<T>` numa próxima major, sem contaminar os componentes
controlados atuais.

Todos os fields produzem IDs estáveis, label associado, helper/error em
`aria-describedby`, `aria-invalid` e `aria-errormessage`. Veja o contrato
específico do [Select](select.md).
