# Tema

O tema é brand-agnostic: `ISUITheme` fornece palette, typography e layout.
Tokens globais usam `--sui-*`; identidade do consumer fica no tema, não nos
componentes. Light/dark são selecionados por `IsDark` e publicados também como
`color-scheme`. Veja [ThemeProvider](theme-provider.md).

Hosts legados que já alternam a classe `.theme-dark` no elemento raiz também
recebem os tokens escuros das fundações. O atributo do provider continua sendo
o contrato preferencial para novas aplicações.
