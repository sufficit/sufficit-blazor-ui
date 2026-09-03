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
`data-sui-theme`, publica tokens válidos em `:root` **e** na própria raiz
(`.sui-root[data-sui-theme]`) e define `color-scheme`; portais no body recebem
a mesma palette. A cópia na raiz existe porque as fundações declaram um
fallback escuro em `[data-sui-theme="dark"]`, e custom properties resolvem
pelo ancestral mais próximo: publicada só em `:root`, a palette do consumidor
perderia para esse fallback em modo escuro. Mantenha uma instância
por aplicação e não codifique marca dentro da RCL. Sem configuração,
`DefaultSUITheme` é o fallback.

O provider também publica `--sui-color-primary-action` e
`--sui-color-primary-action-contrast`. Eles recebem os valores opcionais
`SUIPalette.PrimaryAction`/`PrimaryActionContrast` ou, por compatibilidade,
recuam para `Primary`/`PrimaryContrast`.
