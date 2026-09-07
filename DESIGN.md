---
name: Sufficit.Blazor.UI
description: Sistema visual da biblioteca e da vitrine de componentes Blazor.
colors:
  library-primary: "#2563eb"
  library-primary-dark: "#93c5fd"
  showcase-primary: "#c2410c"
  showcase-primary-dark: "#fb923c"
  showcase-action: "#b7440e"
  showcase-action-contrast: "#fff7ed"
  preset-red: "#b91c1c"
  preset-red-dark: "#fca5a5"
  primary-soft: "color-mix(in srgb, var(--sui-color-primary) 14%, transparent)"
  secondary: "#64748b"
  surface: "#ffffff"
  surface-2: "#f1f5f9"
  surface-3: "#e2e8f0"
  text-primary: "#0f172a"
  text-secondary: "#475569"
  text-disabled: "#94a3b8"
  border: "#e2e8f0"
  border-strong: "#cbd5e1"
  info: "#0369a1"
  success: "#166534"
  warning: "#92400e"
  error: "#b91c1c"
  dark-surface: "#0f172a"
  dark-surface-2: "#1e293b"
  dark-surface-3: "#334155"
  dark-text-primary: "#f1f5f9"
  dark-text-secondary: "#cbd5e1"
  dark-border: "#334155"
  dark-border-strong: "#64748b"
  dark-info: "#7dd3fc"
  dark-success: "#4ade80"
  dark-warning: "#fbbf24"
  dark-error: "#fca5a5"
typography:
  body:
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif'
    fontSize: "15px"
    lineHeight: 1.6
  display:
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif'
    fontSize: "clamp(2rem, 3.1vw, 3rem)"
    fontWeight: 700
    lineHeight: 1.13
    letterSpacing: "-.035em"
  headline:
    fontSize: "1.35rem"
    lineHeight: 1.3
    letterSpacing: "-.02em"
  label:
    fontSize: ".78rem"
    fontWeight: 600
  mono:
    fontFamily: 'ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, monospace'
    fontSize: ".8rem"
    lineHeight: 1.75
  library-display:
    fontSize: "clamp(1.55rem, 2.2vw, 2.25rem)"
    lineHeight: 1.2
    letterSpacing: "-.025em"
  library-headline:
    fontSize: "1.28rem"
    lineHeight: 1.2
  library-body:
    fontSize: ".875rem"
    lineHeight: 1.45
  library-label:
    fontSize: ".75rem"
    lineHeight: 1.2
rounded:
  sm: "4px"
  default: "8px"
  lg: "14px"
  full: "9999px"
  showcase-control: "6px"
  showcase-panel: "12px"
spacing:
  space-1: "4px"
  space-2: "8px"
  space-3: "12px"
  space-4: "16px"
  space-5: "24px"
  space-6: "32px"
components:
  button-primary:
    backgroundColor: "{colors.showcase-action}"
    textColor: "{colors.showcase-action-contrast}"
    rounded: "{rounded.default}"
    padding: "0 14px"
    height: "40px"
  button-outlined:
    backgroundColor: "transparent"
    textColor: "{colors.showcase-primary}"
    rounded: "{rounded.default}"
    padding: "0 14px"
    height: "40px"
  button-text:
    backgroundColor: "transparent"
    textColor: "{colors.showcase-primary}"
    rounded: "{rounded.default}"
    padding: "0 14px"
    height: "40px"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.default}"
    padding: "0 12px"
    height: "40px"
  card:
    backgroundColor: "{colors.surface}"
    rounded: "{rounded.lg}"
    padding: "{spacing.space-4}"
  chip:
    backgroundColor: "{colors.surface-2}"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.full}"
    padding: "0 12px"
    height: "32px"
  navigation-active:
    backgroundColor: "{colors.primary-soft}"
    textColor: "{colors.text-primary}"
    rounded: "5px"
    padding: ".4rem .65rem"
---

# Design System: Sufficit.Blazor.UI

## Overview

**Creative North Star: "Bancada de componentes"**

Descrição inferida da implementação, sem representar uma nova decisão de marca aprovada pelo usuário: uma interface de documentação sóbria, legível e operacional, em que os próprios controles demonstram o sistema. Acentos quentes pontuam ações e navegação; superfícies neutras, divisórias finas e títulos fortes organizam o conteúdo.

Este documento captura o código e as imagens revisadas em 7 de setembro de 2026. A biblioteca mantém o tema padrão azul. A vitrine aplica um tema âmbar próprio e oferece presets azul e vermelho, modos claro, escuro e sistema, e densidades confortável e compacta. Os tokens de componentes no frontmatter representam a vitrine âmbar clara e confortável; os tokens com prefixo `library` ou `dark` identificam explicitamente outros contextos. A composição editorial da vitrine não é uma obrigação para aplicações consumidoras.

**Key Characteristics:**

- Tipografia de sistema, sem dependência de fontes externas.
- Contraste e hierarquia por cor, peso e espaçamento.
- Exemplos reais acompanhados de código e contrato de API.
- Tema global compartilhado pela interface, componentes e portais.

Fontes normativas da implementação: `src/Themes/SUIPalette.cs`, `SUITheme.cs`, `SUILayout.cs`, `SUITypography.cs`, `SUIThemeProvider.razor`, `src/styles/`, `samples/Sufficit.Blazor.UI.Showcase/ShowcaseTheme.cs` e `wwwroot/showcase.css`. Contexto: `PRODUCT.md` e `.impeccable/DIRECTION.md`. Evidência visual: `.impeccable/review/{desktop,mobile,themes,component}.png`. Os nomes descritivos abaixo são documentação inferida dessas fontes.

## Colors

### Primary

O azul da biblioteca é a configuração de fallback, preservada para consumidores existentes. O âmbar queimado da vitrine marca links, foco e identidade. A ação preenchida usa `showcase-action` com seu próprio contraste; isso permite manter texto legível mesmo quando o acento do modo escuro fica mais luminoso.

O preset azul usa os mesmos primários claro/escuro da biblioteca. O vermelho usa `preset-red` e `preset-red-dark`. Nos presets azul e vermelho, ação preenchida acompanha o primário; no âmbar, a superfície da ação permanece estável nos dois modos. Contraste sobre primário é branco no claro e `text-primary` do tema claro no escuro. Contrastes semânticos seguem a mesma alternância; a ação âmbar tem seu contraste específico.

### Secondary

O cinza azulado `secondary` oferece ênfase secundária. Informação, sucesso, aviso e erro têm papéis semânticos próprios e variantes claras para superfícies escuras. Vermelho de marca e erro podem compartilhar um valor, mas continuam sendo tokens com responsabilidades distintas.

### Neutral

Superfície branca, planos recuados frios e texto azul quase preto sustentam o modo claro. No escuro, os planos passam para azul profundo e o texto fica claro. As bordas seguem o plano correspondente. Texto auxiliar usa `text-secondary`; a menor importância não implica usar o token desabilitado. A seleção e a navegação ativa empregam acento e mistura suave, respectivamente.

**The Tema Global Rule.** Consumir as variáveis `--sui-*` em superfícies novas. `SUIThemeProvider` publica valores em `:root` e `.sui-root[data-sui-theme]`, incluindo `color-scheme`; portais ligados ao body recebem a mesma paleta. O contrato atual é global: providers aninhados não constituem temas locais isolados.

## Typography

A família principal de sistema serve títulos, corpo e controles. Código usa a família monoespaçada do frontmatter; rótulos da biblioteca podem usar `FontFamilyLabel`, cujo padrão referencia a família principal.

Na vitrine, o título principal é responsivo (`display`) e passa para tamanho fixo de 2.15rem até 640px. Títulos de seção usam `headline`; subtítulos menores usam 1rem. Texto corrido usa `body`, com leitura limitada a 70ch; introduções usam 1.05rem e limite de 62ch. Rótulos de navegação usam .8rem, enquanto código de blocos usa `mono` e código inline usa .85em.

A biblioteca tem sua própria escala semântica, representada parcialmente pelos tokens `library-*`, e preserva a escala histórica h1–h6. Não aplicar a escala de títulos da vitrine globalmente aos componentes. Botões usam .875rem e peso 600; o texto de apoio dos campos usa .75rem e itálico, e erros usam peso 600.

## Layout

A vitrine usa cabeçalho fixo durante a rolagem (76px), navegação lateral (260px) e documento em grade. O conjunto tem largura máxima de 1600px, e o documento limita-se a 1200px, com respiro vertical de 3.5rem e horizontal responsivo entre 1.5rem e 5rem. A lateral rola independentemente e ocupa a altura disponível abaixo do cabeçalho.

Até 900px, a lateral cai para 220px; grupos do índice e editor de temas passam a uma coluna. Até 640px, cabeçalho e navegação entram no fluxo, a lateral ocupa o topo com altura máxima de 16rem e os grupos detalhados aparecem durante a busca. Conteúdo recebe 2rem por 1rem; o índice fica em uma coluna, e amostras de cores ficam em duas. Código e tabelas de API mantêm rolagem própria.

O ritmo da biblioteca usa a escala `space-1` a `space-6`. Controles médios têm 36px por padrão na biblioteca, 40px na vitrine confortável e 32px na compacta. Botões e campos recebem alvo de pelo menos 44px em ponteiro grosso ou largura até 599.98px. A navegação móvel da vitrine também usa 44px. Densidade compacta não elimina esses ajustes de toque.

## Elevation & Depth

A vitrine organiza planos principalmente com cor de superfície e bordas de 1px. Pré-visualizações e blocos documentais não recebem sombra decorativa. A biblioteca oferece três níveis de sombra para cards e elementos elevados, além da opção sem sombra; valores exatos estão nas extensões do sidecar.

Sobreposições usam `Overlay` da paleta, distinto para claro e escuro. Foco de campo combina borda primária com anel suave (3px). Foco visível geral usa contorno de 2px com afastamento de 3px; botões SUI usam afastamento de 2px.

## Shapes

Cantos discretamente arredondados caracterizam os controles. A biblioteca usa raios pequeno, padrão, grande e circular conforme o frontmatter. Cards SUI usam o raio grande; painéis demonstrativos da vitrine usam `showcase-panel`. Links de ação, busca e blocos de código da vitrine usam `showcase-control`. Seletores do cabeçalho e editor de temas usam o SUISelect e seus tokens, inclusive no menu aberto. Chips são cápsulas. Bordas delimitam estados e agrupamentos sem exigir sombra.

## Components

### Buttons

Botões preenchidos comunicam a ação principal; outlined e text mantêm a cor do acento em graus menores de ênfase. O filled escurece sua superfície com mistura de 90% da cor original no hover e 82% no estado ativo. Outlined e text recebem mistura do acento a 8% no hover. Desabilitados têm opacidade .55 e cursor indisponível. Transições de cor, borda e sombra usam o token padrão; a preferência por movimento reduzido remove a transição desses botões.

### Inputs / Fields

Campos SUI usam superfície base, borda forte, raio padrão e preenchimento horizontal de `space-3`. O foco destaca borda e anel; estado inválido usa borda de erro e mensagem textual. Desabilitados usam superfície secundária e texto desabilitado. O campo nativo da busca segue as regras específicas da vitrine, incluindo altura mínima de 44px. O cabeçalho organiza o label e o SUISelect horizontalmente; os seletores do editor seguem o layout de campos da biblioteca. Não presumir que toda entrada tenha exatamente a altura do token médio.

### Cards / Containers

Cards SUI têm superfície base, borda e espaçamento `space-4`; slots de cabeçalho, conteúdo e ações assumem o espaçamento quando presentes. Pré-visualizações da vitrine têm legenda superior separada por borda e conteúdo com 2rem de respiro, reduzido no mobile. O formulário de abertura usa superfície secundária para distinguir a área operável.

### Chips

Chips neutros usam superfície secundária, texto principal e formato circular. A altura padrão é 32px e a pequena é 24px. Variantes semânticas combinam texto de estado com fundo tonal a 16%. Variantes outlined e text removem preenchimento. Chips acionáveis mantêm foco visível; remoção é um controle próprio.

### Navigation

A navegação lateral usa rótulos compactos agrupados por família. Hover altera a superfície e o texto; item atual combina fundo primário suave e peso 650, com `aria-current`. A busca tem rótulo visível. No mobile, os atalhos gerais permanecem acima do documento e a busca revela as famílias correspondentes.

### Demonstração, código e temas

Cada página combina pré-visualização real, código Razor expansível, ação de copiar e tabela de parâmetros. A área de código tem fundo secundário e altura máxima de 24rem, com rolagem. O editor de tema combina controles, exemplo preenchível, amostras e exportação C#. A troca de aparência preserva os valores da demonstração. Esta é uma composição da vitrine, reutilizável nas suas páginas de documentação.

## Do's and Don'ts

### Do:

- **Do** usar tokens de tema para cores e controles, mantendo interface e portais coerentes.
- **Do** preservar a diferença entre acento primário e superfície de ação preenchida.
- **Do** mostrar foco, erro textual e estados desabilitados nos exemplos reais.
- **Do** manter código e tabelas acessíveis por teclado e com rolagem local.
- **Do** respeitar as regras de toque e movimento reduzido ao variar a densidade.

### Don't:

- **Don't** substituir o padrão azul da biblioteca pelo âmbar específico da vitrine.
- **Don't** tratar tokens de um preset como cores fixas para todos os modos.
- **Don't** apresentar providers aninhados como isolamento local de temas.
- **Don't** usar texto desabilitado para reduzir a ênfase de instruções legíveis.
- **Don't** transformar a composição documental ou as descrições inferidas em regras de marca aprovadas.
