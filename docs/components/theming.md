# Tema

O tema é brand-agnostic: `ISUITheme` fornece palette, typography e layout.
Tokens globais usam `--sui-*`; identidade do consumer fica no tema, não nos
componentes. Light/dark são selecionados por `IsDark` e publicados também como
`color-scheme`. Veja [ThemeProvider](theme-provider.md).

`Primary` é o acento de marca usado por links, foco e contornos.
`PrimaryAction` é a superfície opcional dos controles primários preenchidos;
`PrimaryActionContrast` define seu texto/ícone. Quando os dois tokens de ação
não são informados, a palette recua para `Primary`/`PrimaryContrast`, preservando
temas existentes. Essa separação permite manter um âmbar vivo nos detalhes e
usar um tom mais profundo com texto claro nos botões, sem hardcode no componente.

Hosts legados que já alternam a classe `.theme-dark` no elemento raiz também
recebem os tokens escuros das fundações. O atributo do provider continua sendo
o contrato preferencial para novas aplicações.
