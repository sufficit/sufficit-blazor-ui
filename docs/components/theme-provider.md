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

## Presets e contraste

`SUITheme.Light` e `SUITheme.Dark` fornecem paletas completas. Personalize com
`with { Palette = preset.Palette with { Primary = "..." } }` e mantenha o par
`PrimaryContrast` legível. O mesmo vale para `Info`/`InfoContrast`,
`Success`/`SuccessContrast`, `Warning`/`WarningContrast` e `Error`/`ErrorContrast`.
Os tokens semânticos de contraste são usados nas notificações preenchidas.
`PrimarySoft` acompanha `--sui-color-primary`, sem fixar uma cor de marca.

O provider é global: instâncias simultâneas com temas diferentes disputam os
mesmos tokens. Claro/escuro/sistema e persistência pertencem ao host. A vitrine
implementa essa política em `ShowcaseTheme.cs` e `wwwroot/theme.js`.

## Content Security Policy

O provider publica os tokens em um `<style>` inline. Um host com
`Content-Security-Policy: style-src 'self'` bloqueia esse bloco e todos os
componentes caem no fallback claro azul. Passe o nonce da resposta pelo
parâmetro `Nonce`:

```razor
<SUIThemeProvider Theme="Theme" Nonce="@CspNonce">
    <Routes />
</SUIThemeProvider>
```

O mesmo valor precisa estar na diretiva `style-src 'nonce-…'` do cabeçalho.
Sem `Nonce`, o atributo não é emitido e o comportamento anterior é mantido.
