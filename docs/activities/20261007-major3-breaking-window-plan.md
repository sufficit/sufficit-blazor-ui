# Consolidação de design — janela de quebra v3: plano e destravamento da main

**Data:** 2026-10-07
**Branch:** `main`

## Escopo

Dois trabalhos num único turno: (1) a main estava vermelha desde `d97e206`
(`SUIExpansionPanel`) — diagnóstico e correção; (2) registro do plano da
janela de quebra da próxima major com travas por contrato — a pendência que
fechou a onda P1/F2/F3.

## Main vermelha: o exemplo copiado do catálogo não compilava

**Sintoma.** Nos check-runs de `d97e206`, os jobs `Component tests` e `build`
(Component showcase) falhavam com
`SUIExpansionPanelExample.razor(7,64): error CS1525: Invalid expression term ')'`.
Firefox/webkit/pack/Lighthouse verdes — a falha era só nos passos que compilam
o fonte *copiado* do catálogo.

**Causa raiz.** O exemplo usava

```razor
<SUITextField T="string" Label="Base URL" Value="https://api.example.com" />
```

Com `T="string"` explícito, o parâmetro `Value` é `TValue?` — o compilador
Razor trata o valor do atributo como **expressão crua**, não literal: `https:`
vira rótulo, `//` abre comentário e a expressão morre no `)`. O job
`check-catalog-examples.py` materializa exatamente esse fonte num projeto
temporário (`/tmp/sui-examples-*`) com `-warnaserror`, então a página de
exemplo do Demos nunca passou por esse compilador — só o catálogo copiado.
A falha não apareceu no push original porque esse job roda no CI, não local.

**Correção** (`878d5bd`): literal como expressão explícita —
`Value=@("https://api.example.com")` — no exemplo do Demos e no
`catalog.json` regenerado. `check-catalog-examples.py` reproduziu e validou
localmente: 74 exemplos compilam.

## Teto de transferência de CSS reajustado

O `SUIExpansionPanel` (CSS isolado + página de catálogo) levou a
transferência de CSS medida no Chromium CI para 111.900 B, acima dos 108 KiB
(`TransferredBytes_StayWithinBudget` falhou só no chromium, no primeiro run
do `878d5bd`). Teto elevado para 113 KiB pela regra da casa (medido × 1,03 =
115.257 → 113 KiB), com data e motivo no próprio teste — precedente dos
ajustes de 2026-09-13/23. Revalidado localmente contra o catálogo publicado:
`PerformanceBudget` 5/5.

## Plano da janela de quebra v3

`docs/PLAN-MAJOR3-BREAKING-WINDOW.md` inventaria o que sai na próxima major:

- **21 pontes de atributos** `UserAttributes`/`Attributes` →
  `SUIComponentBase.AdditionalAttributes` (Actions ×4, DataDisplay ×2,
  Feedback ×2, Forms ×5, Layout ×4 com merge, Navigation ×2, Overlays ×2);
- **2 sobrecargas legadas** de severidade por string (`ISuiSnackbar`,
  `ISuiToast`) → `SUITone`;
- **decisões dentro da janela:** defaults tipográficos internos
  (`SUIDrawer`, `SUIPagination`, `SUIStat`, `SUITableEmpty`) — migração muda
  peso/tamanho, exige recaptura de baselines visuais pelo canal sancionado —
  e o futuro da família Material (critério: varredura de uso nos consumers);
- **fora da janela:** família Material não é removida;
  `SUISelectItem.Value` segue `object?` por desenho.

Mecânica de execução em ordem (varredura de consumers → remoção única →
regenerar baseline/catálogo/supressões oficiais → defaults tipográficos →
bump do `PackageValidationBaselineVersion` → CHANGELOG em
*Removed (breaking)*) com gates de saída explícitos.

## Travas

`BreakingWindowContractTests` (4 contratos, na suíte unitária): todo
`[Obsolete]` de `src/` é uma ponte listada (contagem exata 21+2), toda ponte
de componente aponta para `AdditionalAttributes`, toda ponte de serviço para
`SUITone`, e o plano mantém Material e `SUISelectItem.Value` fora da janela.
Nenhuma ponte nova entra — ou sai — em silêncio.

## Validação

- Suíte unitária: 960/960 (`dotnet test`, exit 0 conferido).
- `generate-catalog.py --check`: idempotente; `check-catalog-examples.py`:
  74 exemplos compilam.
- `build-css.mjs --check`: raw 59.138 / gzip 10.853 / brotli 9.473, dentro
  dos orçamentos do script.
- `PerformanceBudgetBrowserTests` local (chromium, catálogo publicado): 5/5.
- CI da main verde após o reajuste do teto.
