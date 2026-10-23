# Stat (`SUIStat`)

Card de indicador: número em destaque, rótulo, variação, legenda e mini gráfico.

## Anatomia e ordem de leitura

Sempre na mesma ordem em todos os cards: **valor** (com `Unit` opcional), **rótulo**, **variação** (`Delta` + `Trend`)
e **legenda** (`Caption`), e o **mini gráfico** (`Sparkline`) no rodapé, sem eixos nem legenda. O ícone fica num
bloco no canto, tingido na cor do card, e repete-se grande e esmaecido como marca d'água nas aparências não neutras.

## Aparências (`Appearance`)

| Valor | Uso |
|---|---|
| `Plain` (padrão) | Neutro. Para indicadores secundários. |
| `Soft` | Fundo tingido na cor, valor na cor. Para indicadores de apoio. |
| `Gradient` | Gradiente diagonal forte, texto claro. Para os 1–2 indicadores que devem chamar atenção. |
| `Image` | Imagem (`BackgroundImage`) sob gradiente na cor; sem URL usa o padrão de ondas embutido. |

Não use o mesmo tratamento em todos os cards: uma parede de cards iguais não prioriza nada. Reserve `Gradient`
para o que importa agora (por exemplo, fila com chamada aguardando) e troque a aparência quando o estado mudar.

## Variação com sentido de "bom"

`Trend` diz para onde o número foi; a cor diz se isso é bom. Em métricas em que menor é melhor (abandono, espera,
erro, latência), use `HigherIsBetter="false"`: uma queda aparece verde. Diga a janela de comparação em `Caption`
("vs. ontem").

## Carregamento e mudança de valor

- `Loading="true"` mostra um esqueleto com o formato do card (sem salto de layout quando o dado chega).
- Quando `Value` muda, o número dá um destaque curto; se o valor anterior e o novo forem inteiros, ele conta até o novo.
- Tudo isso é controlado por [`SUIAnimation`](../ANIMATIONS.md) (`Animation` no card ou padrão da aplicação).

## Acessibilidade

- O texto em `Gradient`/`Image` é branco sobre a cor escurecida no tema escuro (`[data-sui-theme="dark"]`).
- Com `prefers-contrast: more`, os efeitos saem e o fundo vira cor sólida.
- Marca d'água e mini gráfico são `aria-hidden`; a variação tem `aria-label` com a direção.
- `BackgroundImage` só aceita URL relativa, `http(s)` ou `data:` sem aspas, parênteses ou ponto e vírgula.

## Referências consultadas (22/10/2026)

O MudBlazor não tem um componente de stat card: os painéis feitos com ele compõem `MudPaper` + `MudIcon`.
As referências usadas foram:

- [SaaS metric/KPI card UX patterns](https://www.saasui.design/blog/saas-metric-kpi-card-ux-patterns): ordem valor → rótulo → variação →
  gráfico, comparação com período, cor pelo sentido de "bom" (invertida em churn/erro/latência), poucos cards em destaque,
  esqueleto no carregamento, distinguir "sem dado" de zero.
- [KPI sparkline cards (shadcn/ui, Creative Tim)](https://www.creative-tim.com/ui/block/kpi-sparkline-cards): selo de variação no topo,
  sparkline de 12 pontos, lógica invertida para churn.
- [Spark charts (Preline)](https://preline.co/blocks/data-display/spark-charts/): mini gráfico na metade inferior do card.
- [Stat Card (21st.dev)](https://21st.dev/@konsstepnov/components/stat-card.md): ícone, variação com `lowerIsBetter`, sparkline SVG.
- [StatsCard (xertica-ui)](https://cdn.jsdelivr.net/npm/xertica-ui@2.5.3/docs/components/stats-card.md) e
  [PhoenixKit StatCard](https://phoenix-kit.hexdocs.pm/PhoenixKitWeb.Components.Core.StatCard.html): card em gradiente com ícone e tema por cor.
- [KPI card example (FanRuan)](https://gallery.fanruan.com/kpi-card-example): cards que reorganizam em telas pequenas.
