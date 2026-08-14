# Formulários

Família: `SUITextField<T>`, `SUINumericField<T>`, `SUISelect<T>`,
`SUIAutocomplete<T>`, `SUIChoiceCard<TValue>`, `SUISwitch` e
`SUISwitchButton`.

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
