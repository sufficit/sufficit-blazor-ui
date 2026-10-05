# Consolidação de design — P1: SUIDrawer fecha a base única

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Último componente fora da base. Adiado desde o lote Layout por ter
code-behind + interop JS (focus trap, backdrop, modo responsivo).

## Decisões

- Base trocada apenas na declaração: o `.razor` ganha
  `@inherits SUIComponentBase` e o code-behind perde os parâmetros
  duplicados `Class`/`Style`/`AdditionalAttributes` (nomes canônicos
  idênticos aos da base — nenhum alias necessário, nenhuma quebra de API).
- O `aside` raiz já aplicava `style="@StyleValue"` (que concatena o
  `--sui-drawer-width` com o `Style` do consumidor) e splattava
  `AdditionalAttributes`; nada muda no markup além da origem dos
  parâmetros.
- Com este commit, **todos os 72 componentes de `src/Components/` herdam
  de `SUIComponentBase`** (verificação: no `eng/PublicApiBaseline.txt`
  restam apenas `SUIComponentBase` — a própria base — e
  `SUIThemeProvider`, provedor de tema em `Themes/`, fora do escopo das
  famílias de componentes).

## Correção de registro

A nota e o CHANGELOG do lote Forms (`0392963`) afirmavam prematuramente
que aquela leva fechava os 100%. Incorreto — o Drawer ainda pendia. Os
dois arquivos foram corrigidos neste commit.

## Validação

- Suíte unitária 933/933; build limpo; catálogo regenerado.
- Baseline de API: `SUIDrawer` migra para `SUIComponentBase`.
