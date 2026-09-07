# Formulários

Família: `SUITextField<T>`, `SUINumericField<T>`, `SUIDateField`, `SUISelect<T>`,
`SUIAutocomplete<T>`, `SUIChoiceCard<TValue>`, `SUICheckbox`, `SUISwitch` e
`SUISwitchButton`. `SUIFormGrid` organiza fields equivalentes em colunas.

## Data localizada

`SUIDateField` substitui o `input type="date"` quando o calendário precisa
seguir o tema e a cultura da aplicação. O valor é `DateOnly?`; a apresentação,
os nomes de mês/dia e as ações do popover usam `Culture` ou, quando omitida,
`CultureInfo.CurrentUICulture`:

```razor
<SUIDateField Label="Cadastro inicial"
              Name="registeredFrom"
              @bind-Value="Model.RegisteredFrom" />
```

O calendário usa um `dialog` não modal no top layer, fecha ao clicar fora e
aceita setas, Home/End, PageUp/PageDown, Enter, Espaço e Escape. `Min`, `Max`,
`Required`, helper e erro preservam o mesmo contrato controlado dos demais
fields. O valor enviado por `Name` permanece ISO (`yyyy-MM-dd`), independente
do formato visível.

## Checkbox

`SUICheckbox` mantém o contrato controlado `Value`/`ValueChanged` sobre um
checkbox nativo, com foco visível, nome acessível e alvo mínimo de 44px em
viewports touch:

```razor
<SUICheckbox Value="_selected"
             ValueChanged="OnSelectedChanged"
             AriaLabel="Selecionar provider" />
```

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
Textos longos quebram linha em vez de serem truncados. Ícone, bloco textual e
indicador permanecem centralizados verticalmente no card, inclusive quando há
descrição.

Opções do mesmo grupo devem compartilhar `Name`. Use `Description` apenas para
informação necessária à decisão e mantenha `ShowSelectionIndicator="true"`
quando não houver outro `TrailingContent` que comunique o estado selecionado.

## Contrato de valor e validação

Os fields continuam controlados por `Value`/`ValueChanged`, sem herdar
`InputBase<T>`. Dentro de `EditForm`, uma expressão de valor associa o campo ao
`EditContext`: alterações notificam o formulário e a primeira mensagem do
validador aparece no campo. `@bind-Value` fornece `ValueExpression` pelo binding
Razor; com callbacks explícitos, passe `ValueExpression="() => Model.Name"`.
`SUIChoiceCard` usa `SelectedValueExpression` com `@bind-SelectedValue`.

```razor
<EditForm Model="Model" OnValidSubmit="SaveAsync">
    <DataAnnotationsValidator />
    <SUITextField T="string" Label="Nome" @bind-Value="Model.Name" />
    <SUINumericField T="int" Label="Tentativas" @bind-Value="Model.Attempts" />
    <SUIButton ButtonTypeValue="SUIButtonType.Submit">Salvar</SUIButton>
</EditForm>
```

O modelo e seu validador definem regras como `[Required]` e `[Range]`.
`Required` no controle sozinho não substitui as regras do modelo. Erros de
parsing dos campos de texto/número entram no `ValidationMessageStore` e
impedem submit válido. `Min`/`Max` numéricos só geram atributos HTML quando
fornecidos. `Invalid` e `ErrorText` continuam disponíveis para validação
externa; fora de `EditForm`, o componente permanece controlado.

Essa integração altera notificações e mensagens de campos com expressão já
usados em `EditForm`; revise consumidores que combinam validação manual e
DataAnnotations para evitar mensagens duplicadas.

Todos os fields produzem IDs estáveis e associações ARIA de label, helper e
erro. Veja o contrato específico do [Select](select.md).

## Busca remota

`SUIAutocomplete.SearchFuncAsync` recebe `(texto, cancellationToken)` e tem
prioridade sobre o callback legado `SearchFunc`. Encaminhe o token ao seu
cliente HTTP. O componente cancela a busca anterior e ignora resultados
obsoletos; o callback legado permanece compatível, mas não recebe token.
