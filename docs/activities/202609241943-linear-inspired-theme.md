# Tema discreto inspirado no Linear — 2026-09-24 19:43 -03

## Objetivo

Salvar como preset reutilizável do Sufficit.Blazor.UI a direção visual aprovada para o compositor Genius: estrutura menos evidente, foco sem halo externo e ações com contraste próprio.

## Estado inicial

`SUITheme.Light`/`Dark` eram os presets públicos. Campos, seletores e checkboxes derivavam foco de `Primary` e do halo de `PrimarySoft`; não havia tokens próprios para foco. A vitrine mantinha seus presets independentes.

## Entrega

- Adicionados `SUITheme.LinearInspiredLight` e `LinearInspiredDark`, opt-in, com paletas próprias, superfícies neutras e sombras contidas.
- Adicionados `SUIPalette.Focus` e `SUILayout.FocusShadow`, publicados como `--sui-focus-color` e `--sui-focus-shadow`. O fallback mantém exatamente o comportamento visual anterior dos temas existentes.
- Campos, seletores e checkboxes passaram a consumir os tokens de foco. No novo preset, o campo tem 2 px visuais de contorno e nenhum halo externo; os demais controles preservam indicador de teclado.
- Atualizados `DESIGN.md`, [contrato do tema](../THEME-LINEAR-INSPIRED.md), guia do provider, bundle e cópias de compatibilidade.
- Os SVGs do Genius permanecem próprios; não foram substituídos por ícones do Linear.

## Decisões e referências

- [Revisão visual do Linear](https://linear.app/now/behind-the-latest-design-refresh): inspiração para separadores e controles auxiliares discretos, sem copiar sua marca ou paleta.
- [W3C Focus Appearance](https://www.w3.org/WAI/WCAG22/Understanding/focus-appearance): o contorno conserva área de foco. Na comparação de cores definida pelo CSS, o foco claro tem razão aproximada de 3,22:1 contra a borda forte anterior; o escuro, aproximadamente 3,1:1. Isso não substitui auditoria da tela inteira.
- `PrimaryAction` separado de `Focus` mantém a ação preenchida legível.

## Validação

- `npm run build:css` e `npm run check:css`: passaram; bundle 56.618 B bruto / 10.444 B gzip / 9.118 B Brotli, dentro do orçamento do repositório.
- `dotnet test tests/Sufficit.Blazor.UI.Tests/Sufficit.Blazor.UI.Tests.csproj`: 895 testes passaram. Baseline da API pública foi atualizado para os quatro membros novos.
- Prévia renderizada com o CSS real nos modos [claro](/mnt/workspaces/sufficit/tmp/sui-theme-preview/light.png) e [escuro](/mnt/workspaces/sufficit/tmp/sui-theme-preview/dark.png).
- O pacote local do Genius foi publicado com o projeto SUI desta árvore; o CSS instalado tem o mesmo hash do bundle fonte.

## Limites

Preset opcional; não muda o tema padrão nem a vitrine. Trabalho local, sem pacote NuGet, PR ou push. A política de escolha e persistência claro/escuro continua a cargo do host consumidor.
