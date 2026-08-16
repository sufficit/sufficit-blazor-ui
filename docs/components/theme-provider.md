# SUIThemeProvider

Configure por DI:

```csharp
builder.Services.AddSufficitUI(options => options.Theme = new MeuTema());
```

e envolva a raiz interativa:

```razor
<SUIThemeProvider><Routes /></SUIThemeProvider>
```

O parâmetro `Theme` vence o tema do DI. O provider renderiza uma raiz
`data-sui-theme`, publica tokens válidos em `:root`/tema escuro e define
`color-scheme`; portais no body recebem a mesma palette. Mantenha uma instância
por aplicação e não codifique marca dentro da RCL. Sem configuração,
`DefaultSUITheme` é o fallback.

O provider também publica `--sui-color-primary-action` e
`--sui-color-primary-action-contrast`. Eles recebem os valores opcionais
`SUIPalette.PrimaryAction`/`PrimaryActionContrast` ou, por compatibilidade,
recuam para `Primary`/`PrimaryContrast`.
