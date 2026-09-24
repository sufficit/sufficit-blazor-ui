# Tema discreto inspirado no Linear

Este preset traduz para o Sufficit.Blazor.UI a direção visual aprovada para o compositor do Genius. É uma interpretação própria, **não** um espelho da paleta, dos componentes ou da marca Linear. A referência principal é a [explicação da revisão visual do Linear em 2026](https://linear.app/now/behind-the-latest-design-refresh): elementos auxiliares cedem atenção ao conteúdo, separadores ficam suaves e ícones evitam tratamentos decorativos desnecessários.

## Contrato visual

| Papel | Claro | Escuro | Regra |
| --- | --- | --- | --- |
| Superfície | `#ffffff` | `#1f2329` | Conteúdo principal limpo |
| Superfície secundária | `#f6f7f8` | `#191d23` | Painéis recuados, sem sombras decorativas |
| Texto principal | `#202630` | `#f0f2f5` | Contraste preservado |
| Borda comum | `#e3e6ea` | `#343b45` | Estrutura perceptível sem competir com conteúdo |
| Foco | `oklch(55% .055 262)` | `oklch(72% .06 262)` | Borda + linha interna: 2 px visuais, sem halo externo |
| Ação preenchida | `#35435a` | `#c7d0df` | Separada da cor de foco para não enfraquecer a CTA |

O foco de campos e seletores usa `--sui-focus-color` e `--sui-focus-shadow`. O anel padrão da biblioteca continua disponível e é preservado nos presets atuais. A opção discreta usa `inset 0 0 0 1px var(--sui-focus-color)` além da borda de 1 px. Essa área é coerente com o exemplo de perímetro do [W3C Focus Appearance](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance); a conformidade de uma tela completa depende dos estados e fundos reais do consumidor. Foco de teclado em botões e outros controles continua com contorno visível.

## Uso

```csharp
builder.Services.AddSufficitUI(options =>
    options.Theme = SUITheme.LinearInspiredLight);
```

```razor
<SUIThemeProvider Theme="@CurrentTheme">
    <Routes />
</SUIThemeProvider>

@code {
    private SUITheme CurrentTheme = SUITheme.LinearInspiredLight;
    private void SetDark(bool dark) => CurrentTheme = dark
        ? SUITheme.LinearInspiredDark
        : SUITheme.LinearInspiredLight;
}
```

O host decide se usa claro, escuro ou a preferência do sistema e persiste essa escolha. O provider é global: não usar dois presets simultaneamente na mesma página. `SUITheme.Light`/`Dark` continuam sendo os padrões existentes.

## Aplicação no Genius

O Genius consome SUI para componentes compartilhados e mantém o compositor em CSS próprio. O foco do compositor usa as mesmas cores e a mesma geometria de 2 px. O texto digitado fica em 14 px; mensagens do chat continuam em 15 px. Os SVGs em `media/attachment-icon.svg`, `microphone-icon.svg`, `pause-icon.svg` e `send-*.svg` pertencem ao Genius e conservam sua forma e comportamento: eles são a diferença deliberada em relação à referência Linear.

## Ledger

| Decisão | Fonte | Motivo |
| --- | --- | --- |
| Estrutura suave e controles auxiliares discretos | Linear | Priorizar o trabalho do usuário |
| Foco claro, sem brilho externo | Prévia aprovada + W3C | Reduzir ruído sem esconder navegação por teclado |
| Primário de ação separado do foco | Contrato `SUIPalette.PrimaryAction` | Manter contraste das ações preenchidas |
| Ícones próprios do Genius | Pedido do usuário | Preservar identidade e funcionalidades locais |
